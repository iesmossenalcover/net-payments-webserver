using System.Text;
using Application.Common.Models;
using Domain.Services;
using Domain.Entities.Authentication;
using Google.Apis.Admin.Directory.directory_v1;
using Google.Apis.Admin.Directory.directory_v1.Data;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Calendar.v3;
using Google.Apis.Calendar.v3.Data;
using Google.Apis.Requests;
using Google.Apis.Services;
using Google;
using GoogleUser = Google.Apis.Admin.Directory.directory_v1.Data.User;
// Amb alias: el namespace de Gmail duu un UsersResource que xoca amb el de Directory.
using GmailService = Google.Apis.Gmail.v1.GmailService;
using GmailMessage = Google.Apis.Gmail.v1.Data.Message;

namespace Infrastructure;

public class GoogleAdminApi : IGoogleAdminApi
{
    private const int GOOGLE_API_ERROR_CONFLICT = 409;

    // Operacions simultànies contra l'API. La quota per defecte de l'Admin SDK Directory
    // és de 2.400 consultes/minut (40/s); amb 5 fils i ~1 crida cada 300 ms hi anam molt per davall.
    private const int GOOGLE_API_PARALLELISM = 5;

    // Intents totals (el primer inclòs) davant d'errors temporals de l'API.
    private const int MAX_ATTEMPTS = 3;

    // Google Calendar event color id. "4" = Flamingo (pink).
    private const string CALENDAR_EVENT_COLOR_ID = "4";

    private static readonly string[] SCOPES = new string[]
    {
        DirectoryService.Scope.AdminDirectoryUser,
        DirectoryService.Scope.AdminDirectoryGroupMember,
        DirectoryService.Scope.AdminDirectoryGroup,
        CalendarService.Scope.Calendar,
        CalendarService.Scope.CalendarEvents,
    };

    // Gmail va a part de SCOPES: si la delegació de domini encara no té autoritzat gmail.send,
    // només falla l'enviament de correu i no la resta de crides.
    private static readonly string[] GMAIL_SCOPES = new string[]
    {
        GmailService.Scope.GmailSend,
    };

    // "me" fa referència a l'usuari suplantat per la credencial.
    private const string GMAIL_AUTHENTICATED_USER = "me";

    private readonly string CredentialFilePath;
    private readonly string UserEmailToImpersonate;
    private readonly string Domain;
    private readonly string ApplicationName;
    private readonly string? EmailSenderName;
    // Rol -> grup de Google, ordenat de més a menys privilegi. Un usuari rep només el primer rol que li correspon.
    // TODO: moure aquesta relació a la bbdd (GoogleGroupClaimRelation).
    private readonly (string Role, string GroupEmail)[] RoleGroups;
    private readonly string[] excludeEmails;
    private readonly ILogger _logger;

    // Els serveis de Google són thread-safe i reutilitzables: crear-ne un per cada crida
    // rellegia el fitxer de credencials i negociava un token OAuth nou a cada operació.
    private readonly Lazy<DirectoryService> _directoryService;
    private readonly Lazy<CalendarService> _calendarService;
    private readonly Lazy<GmailService> _gmailService;


    public GoogleAdminApi(IConfiguration configuration, ILogger<GoogleAdminApi> logger)
    {
        CredentialFilePath = configuration.GetValue<string>("GoogleApiCredentialFilePath") ?? throw new Exception("GoogleApiCredentialFilePath");
        UserEmailToImpersonate = configuration.GetValue<string>("GoogleApiUserEmailToImpersonate") ?? throw new Exception("GoogleApiUserEmailToImpersonate");
        ApplicationName = configuration.GetValue<string>("GoogleApiApplicationName") ?? throw new Exception("GoogleApiApplicationName");
        EmailSenderName = configuration.GetValue<string>("GoogleApiEmailSenderName");
        RoleGroups =
        [
            (RoleClaimValues.SUPER_USER, configuration.GetValue<string>("GoogleApiSuperuserGroupEmail") ?? throw new Exception("GoogleApiSuperuserGroupEmail")),
            (RoleClaimValues.ADVANCED_ADMIN, configuration.GetValue<string>("GoogleApiAdvancedadminGroupEmail") ?? throw new Exception("GoogleApiAdvancedadminGroupEmail")),
            (RoleClaimValues.ADMIN, configuration.GetValue<string>("GoogleApiAdminGroupEmail") ?? throw new Exception("GoogleApiAdminGroupEmail")),
            (RoleClaimValues.READER, configuration.GetValue<string>("GoogleApiEmailGroupReader") ?? throw new Exception("GoogleApiEmailGroupReader")),
        ];
        Domain = configuration.GetValue<string>("GoogleApiDomain") ?? throw new Exception("GoogleApiDomain");
        excludeEmails = configuration.GetValue<string>("GoogleApiExcludeAccounts")?.Split(" ") ?? throw new Exception("GoogleApiExcludeAccounts");
        _logger = logger;

        _directoryService = new Lazy<DirectoryService>(CreateDirectoryService);
        _calendarService = new Lazy<CalendarService>(CreateCalendarService);
        _gmailService = new Lazy<GmailService>(CreateGmailService);
    }

    public async Task<IEnumerable<string>> GetUserClaims(string email, CancellationToken ct)
    {
        DirectoryService service = _directoryService.Value;

        // Totes les comprovacions van en una sola petició HTTP (batch). Es fa servir HasMember
        // (i no Groups.List) perquè també té en compte la pertinença indirecta a través de subgrups.
        var isMember = new bool[RoleGroups.Length];
        var batch = new BatchRequest(service);
        for (int i = 0; i < RoleGroups.Length; i++)
        {
            int index = i;
            string groupEmail = RoleGroups[i].GroupEmail;
            batch.Queue<MembersHasMember>(service.Members.HasMember(groupEmail, email),
                (content, error, _, _) =>
                {
                    if (error != null)
                    {
                        _logger.LogWarning("GetUserClaims: error comprovant si {Email} pertany a {Group}: {Error}", email, groupEmail, error.Message);
                        return;
                    }
                    isMember[index] = content?.IsMember ?? false;
                });
        }

        try
        {
            await batch.ExecuteAsync(ct);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "GetUserClaims: error consultant els grups de {Email}", email);
            return Array.Empty<string>();
        }

        for (int i = 0; i < RoleGroups.Length; i++)
        {
            if (isMember[i])
            {
                return [RoleGroups[i].Role];
            }
        }

        return [];
    }

    public async Task<GoogleApiResult<int>> SetSuspendByOU(
        string ouPath,
        bool suspend,
        bool exactOu
        )
    {
        _logger.LogInformation(
            "SetSuspendByOU: inici. OU: {OuPath}, suspendre: {Suspend}, només OU exacta: {ExactOu}",
            ouPath, suspend, exactOu);

        // Sense aquesta comprovació la consulta seria orgUnitPath='' i el filtre per OU
        // deixaria de tenir sentit.
        if (string.IsNullOrWhiteSpace(ouPath))
        {
            _logger.LogWarning("SetSuspendByOU: la ruta de la unitat organitzativa és buida");
            return GoogleApiResult<int>.Fail("la ruta de la unitat organitzativa és buida");
        }

        try
        {
            DirectoryService service = _directoryService.Value;

            UsersResource.ListRequest userListRequest = service.Users.List();
            userListRequest.Query = $"orgUnitPath='{ouPath}'";
            userListRequest.Domain = Domain;
            userListRequest.MaxResults = 500;

            List<GoogleUser> usersInOu = new();
            string? pageToken = null;
            int page = 0;
            do
            {
                userListRequest.PageToken = pageToken;
                Users response = await ExecuteWithRetryAsync(
                    () => userListRequest.ExecuteAsync(), $"llistat OU {ouPath}");
                page++;

                // Una pàgina pot arribar sense usuaris i amb token de pàgina següent.
                // El token s'ha d'avançar sempre: si només s'avançava quan hi havia usuaris,
                // es repetia la mateixa petició indefinidament i el procés es penjava.
                IList<GoogleUser> pageUsers = response?.UsersValue ?? new List<GoogleUser>();
                pageToken = response?.NextPageToken;

                _logger.LogInformation(
                    "SetSuspendByOU: OU {OuPath}, pàgina {Page}: {Count} usuaris, més pàgines: {HasMore}",
                    ouPath, page, pageUsers.Count, !string.IsNullOrEmpty(pageToken));

                usersInOu.AddRange(pageUsers);
            }
            while (!string.IsNullOrEmpty(pageToken));

            int excluded = 0;
            int otherOu = 0;
            int alreadyInState = 0;
            List<GoogleUser> pendingUsers = new();

            foreach (GoogleUser user in usersInOu)
            {
                // IMPORTANT: Exclude members
                if (excludeEmails.Contains(user.PrimaryEmail))
                {
                    excluded++;
                    _logger.LogInformation(
                        "SetSuspendByOU: usuari {User} exclòs per configuració", user.PrimaryEmail);
                    continue;
                }

                // If we want exactOU, orga path must be the same, not descdendant.
                if (exactOu && user.OrgUnitPath != ouPath)
                {
                    otherOu++;
                    continue;
                }

                // Suspended és bool?: quan l'API no retorna el camp val null. Comparar-lo amb
                // "== false" feia que aquests usuaris no s'arribassin a suspendre mai.
                bool currentlySuspended = user.Suspended == true;
                if (currentlySuspended == suspend)
                {
                    alreadyInState++;
                    continue;
                }

                pendingUsers.Add(user);
            }

            _logger.LogInformation(
                "SetSuspendByOU: OU {OuPath}, trobats: {Found}, a processar: {Pending}, ja en l'estat desitjat: {Already}, exclosos: {Excluded}, d'altres OU: {OtherOu}",
                ouPath, usersInOu.Count, pendingUsers.Count, alreadyInState, excluded, otherOu);

            // Els errors es desen per posició per mantenir-los en el mateix ordre que la
            // llista d'usuaris encara que les actualitzacions acabin desordenades.
            string?[] userErrors = new string?[pendingUsers.Count];
            int changed = 0;

            await Parallel.ForEachAsync(
                Enumerable.Range(0, pendingUsers.Count),
                new ParallelOptions { MaxDegreeOfParallelism = GOOGLE_API_PARALLELISM },
                async (index, _) =>
                {
                    GoogleUser user = pendingUsers[index];
                    try
                    {
                        // Patch amb només el camp que canvia: amb Update es reenviava tot el
                        // perfil llegit abans, cosa que pot desfer canvis fets mentrestant.
                        GoogleUser patch = new GoogleUser() { Suspended = suspend };
                        await ExecuteWithRetryAsync(
                            () => service.Users.Patch(patch, user.Id).ExecuteAsync(), user.PrimaryEmail);

                        Interlocked.Increment(ref changed);
                        _logger.LogInformation(
                            "SetSuspendByOU: usuari {User} (OU {UserOu}) actualitzat a suspès={Suspend}",
                            user.PrimaryEmail, user.OrgUnitPath, suspend);
                    }
                    catch (Exception e)
                    {
                        userErrors[index] = $"{user.PrimaryEmail}: {DescribeError(e)}";
                        _logger.LogError(e,
                            "SetSuspendByOU: error actualitzant l'usuari {User} de l'OU {OuPath}",
                            user.PrimaryEmail, ouPath);
                    }
                });

            List<string> errors = userErrors.Where(e => e != null).Select(e => e!).ToList();

            _logger.LogInformation(
                "SetSuspendByOU: fi. OU {OuPath}, actualitzats: {Changed}/{Pending}, errors: {Errors}",
                ouPath, changed, pendingUsers.Count, errors.Count);

            // Abans els errors de cada usuari només s'escrivien al log i el mètode retornava
            // sempre Ok: el procés deia [OK] mentre quedaven usuaris sense suspendre.
            if (errors.Count > 0)
            {
                return GoogleApiResult<int>.Fail(
                    $"{changed}/{pendingUsers.Count} usuaris actualitzats. Errors: {string.Join(" | ", errors)}");
            }

            return GoogleApiResult<int>.Ok(changed);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "SetSuspendByOU: error processant l'OU {OuPath}", ouPath);
            return GoogleApiResult<int>.Fail(DescribeError(e));
        }
    }

    /// <summary>
    /// Executa una crida a l'API reintentant-la quan l'error és temporal (quota o error de
    /// servidor). Sense això una errada puntual deixava l'usuari sense processar.
    /// </summary>
    private async Task<T> ExecuteWithRetryAsync<T>(Func<Task<T>> action, string subject)
    {
        for (int attempt = 1; ; attempt++)
        {
            try
            {
                return await action();
            }
            catch (GoogleApiException apiEx) when (attempt < MAX_ATTEMPTS && IsTransientError(apiEx))
            {
                TimeSpan delay = TimeSpan.FromSeconds(Math.Pow(2, attempt - 1));
                _logger.LogWarning(apiEx,
                    "Error temporal (codi {Code}) a {Subject}. Reintent {Attempt}/{MaxAttempts} d'aquí a {Delay}s",
                    apiEx.Error?.Code, subject, attempt, MAX_ATTEMPTS, delay.TotalSeconds);
                await Task.Delay(delay);
            }
        }
    }

    private static bool IsTransientError(GoogleApiException e)
    {
        int? code = e.Error?.Code;

        if (code == 429) return true;
        if (code >= 500 && code < 600) return true;

        // La quota superada arriba com un 403 amb un d'aquests motius.
        return code == 403
            && (e.Error?.Errors?.Any(x =>
                x.Reason == "rateLimitExceeded"
                || x.Reason == "userRateLimitExceeded"
                || x.Reason == "quotaExceeded") ?? false);
    }

    public async Task<GoogleApiResult<bool>> CreateUser(
        string email,
        string firstName,
        string lastName,
        string password,
        string ouPath,
        bool changePasswordNexLogin = true
        )
    {
        Google.Apis.Admin.Directory.directory_v1.Data.User newUser = new Google.Apis.Admin.Directory.directory_v1.Data.User()
        {
            PrimaryEmail = email,
            Name = new UserName() { GivenName = firstName, FamilyName = lastName },
            Password = password,
            ChangePasswordAtNextLogin = changePasswordNexLogin,
            OrgUnitPath = ouPath,
        };
        try
        {
            DirectoryService service = _directoryService.Value;
            newUser = await service.Users.Insert(newUser).ExecuteAsync();
            return GoogleApiResult<bool>.Ok(true);
        }
        catch (System.Exception e)
        {
            return GoogleApiResult<bool>.Fail(e.Message);
        }
    }


    public async Task<GoogleApiResult<bool>> AddUserToGroup(string email, string group)
    {
        try
        {
            DirectoryService service = _directoryService.Value;

            string memberId = email;
            var memberRequest = service.Users.Get(memberId);
            var m = await memberRequest.ExecuteAsync();
            if (m == null)
            {
                return GoogleApiResult<bool>.Fail("user not found");
            }

            Member member = new Member()
            {
                Email = email
            };


            string groupId = group;
            var groupRequest = service.Groups.Get(groupId);
            var gp = await groupRequest.ExecuteAsync();
            if (gp == null)
            {
                return GoogleApiResult<bool>.Fail("group not found");
            }


            var addRequest = service.Members.Insert(member, gp.Id);
            member = await addRequest.ExecuteAsync();
            return GoogleApiResult<bool>.Ok(true);
        }
        catch (GoogleApiException apiEx)
        {
            // https://developers.google.com/webmaster-tools/v1/errors
            return apiEx.Error.Code switch
            {
                GOOGLE_API_ERROR_CONFLICT => GoogleApiResult<bool>.Ok(true),
                _ => GoogleApiResult<bool>.Fail(apiEx.Error.Message),
            };
        }
        catch (Exception e)
        {
            return GoogleApiResult<bool>.Fail(e.Message);
        }
    }

    public async Task<GoogleApiResult<bool>> RemoveUserFromGroup(string email, string group)
    {
        try
        {
            DirectoryService service = _directoryService.Value;

            string memberId = email;
            var memberRequest = service.Users.Get(memberId);
            var m = await memberRequest.ExecuteAsync();
            if (m == null)
            {
                return GoogleApiResult<bool>.Fail("user not found");
            }


            string groupId = group;
            var groupRequest = service.Groups.Get(groupId);
            var gp = await groupRequest.ExecuteAsync();
            if (gp == null)
            {
                return GoogleApiResult<bool>.Fail("group not found");
            }

            MembersResource.GetRequest getRequest = service.Members.Get(groupId, email);
            Member member = await getRequest.ExecuteAsync();

            // Delete the member from the group.
            await service.Members.Delete(groupId, member.Id).ExecuteAsync();

            return GoogleApiResult<bool>.Ok(true);
        }
        catch (System.Exception e)
        {
            return GoogleApiResult<bool>.Fail(e.Message);
        }
    }

    public async Task<GoogleApiResult<int>> ClearGroupMembers(string group)
    {
        _logger.LogInformation("ClearGroupMembers: inici. Grup: {Group}", group);
        try
        {
            DirectoryService service = _directoryService.Value;

            var gp = await service.Groups.Get(group).ExecuteAsync();
            if (gp == null)
            {
                _logger.LogWarning("ClearGroupMembers: grup no trobat. Grup: {Group}", group);
                return GoogleApiResult<int>.Fail($"grup no trobat ({group})");
            }

            _logger.LogInformation(
                "ClearGroupMembers: grup trobat. Grup: {Group}, id: {GroupId}, membres directes: {DirectMembers}",
                gp.Email, gp.Id, gp.DirectMembersCount);

            // Primer llistam tots els membres i després els esborram. Si esborram mentre paginam,
            // el PageToken queda invalidat i les pàgines següents poden tornar sense membres.
            List<Member> members = new();
            MembersResource.ListRequest listRequest = service.Members.List(group);
            listRequest.MaxResults = 200;
            string? pageToken = null;
            int page = 0;
            do
            {
                listRequest.PageToken = pageToken;
                Members response = await ExecuteWithRetryAsync(
                    () => listRequest.ExecuteAsync(), $"llistat grup {group}");
                page++;

                // Quan la pàgina no té cap membre l'API omet "members" del JSON, per tant
                // MembersValue és null (i no una llista buida).
                IList<Member> pageMembers = response?.MembersValue ?? new List<Member>();
                pageToken = response?.NextPageToken;

                _logger.LogInformation(
                    "ClearGroupMembers: grup {Group}, pàgina {Page}: {Count} membres, més pàgines: {HasMore}",
                    group, page, pageMembers.Count, !string.IsNullOrEmpty(pageToken));

                members.AddRange(pageMembers);
            }
            while (!string.IsNullOrEmpty(pageToken));

            int removed = 0;
            int skipped = 0;

            /*
                https://developers.google.com/admin-sdk/directory/v1/guides/manage-group-members?hl=es-419
                El type d'un membre del grup pot ser:
                GROUP: el membre és un altre grup.
                USER: el membre és un usuari.
            */
            List<Member> usersToRemove = new();
            foreach (Member member in members)
            {
                if (member.Type == "USER")
                {
                    usersToRemove.Add(member);
                    continue;
                }

                skipped++;
                _logger.LogInformation(
                    "ClearGroupMembers: grup {Group}, membre {Member} ignorat (type: {Type})",
                    group, member.Email, member.Type);
            }

            // Els errors es desen per posició per mantenir-los sempre en el mateix ordre
            // que la llista de membres, encara que els esborrats acabin desordenats.
            string?[] memberErrors = new string?[usersToRemove.Count];

            await Parallel.ForEachAsync(
                Enumerable.Range(0, usersToRemove.Count),
                new ParallelOptions { MaxDegreeOfParallelism = GOOGLE_API_PARALLELISM },
                async (index, _) =>
                {
                    Member member = usersToRemove[index];
                    try
                    {
                        await ExecuteWithRetryAsync(
                            () => service.Members.Delete(group, member.Id).ExecuteAsync(), member.Email);
                        Interlocked.Increment(ref removed);
                        _logger.LogInformation(
                            "ClearGroupMembers: grup {Group}, membre {Member} (id: {MemberId}) esborrat",
                            group, member.Email, member.Id);
                    }
                    catch (GoogleApiException apiEx) when (apiEx.Error?.Code == 404 || apiEx.Error?.Code == 410)
                    {
                        Interlocked.Increment(ref skipped);
                        _logger.LogWarning(
                            "ClearGroupMembers: grup {Group}, membre {Member} ja no hi era (codi {Code})",
                            group, member.Email, apiEx.Error?.Code);
                    }
                    catch (Exception e)
                    {
                        memberErrors[index] = $"{member.Email}: {DescribeError(e)}";
                        _logger.LogError(e,
                            "ClearGroupMembers: error esborrant el membre {Member} del grup {Group}",
                            member.Email, group);
                    }
                });

            List<string> errors = memberErrors.Where(e => e != null).Select(e => e!).ToList();

            _logger.LogInformation(
                "ClearGroupMembers: fi. Grup: {Group}, trobats: {Found}, esborrats: {Removed}, ignorats: {Skipped}, errors: {Errors}",
                group, members.Count, removed, skipped, errors.Count);

            if (errors.Count > 0)
            {
                return GoogleApiResult<int>.Fail(
                    $"{removed}/{members.Count} membres esborrats. Errors: {string.Join(" | ", errors)}");
            }

            return GoogleApiResult<int>.Ok(removed);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "ClearGroupMembers: error buidant el grup {Group}", group);
            return GoogleApiResult<int>.Fail(DescribeError(e));
        }
    }

    private static string DescribeError(Exception e)
    {
        if (e is GoogleApiException apiEx)
        {
            return $"GoogleApiException {apiEx.Error?.Code}: {apiEx.Error?.Message ?? apiEx.Message}";
        }

        return $"{e.GetType().Name}: {e.Message}";
    }

    public async Task<GoogleApiResult<bool>> MoveUserToOU(string email, string ouPath)
    {
        try
        {
            DirectoryService service = _directoryService.Value;
            string memberId = email;
            var memberRequest = service.Users.Get(memberId);
            var member = await memberRequest.ExecuteAsync();
            if (member == null)
            {
                return GoogleApiResult<bool>.Fail("user not found");
            }

            member.OrgUnitPath = ouPath;

            var updateRequest = service.Users.Update(member, memberId);
            member = await updateRequest.ExecuteAsync();
            return GoogleApiResult<bool>.Ok(true);
        }
        catch (System.Exception e)
        {
            return GoogleApiResult<bool>.Fail(e.Message);
        }
    }

    public async Task<GoogleApiResult<IEnumerable<string>>> GetAllUsers(string ouPath)
    {
        try
        {
            DirectoryService service = _directoryService.Value;
            List<string> usersList = new List<string>();
            UsersResource.ListRequest userListRequest = service.Users.List();
            userListRequest.Query = $"orgUnitPath='{ouPath}'";
            userListRequest.Domain = Domain;
            userListRequest.MaxResults = 50;
            Users users;

            do
            {
                users = await userListRequest.ExecuteAsync();
                if (users.UsersValue != null)
                {
                    foreach (var user in users.UsersValue)
                    {
                        usersList.Add(user.PrimaryEmail);
                    }
                    userListRequest.PageToken = users.NextPageToken;
                }

            }
            while (!string.IsNullOrEmpty(users.NextPageToken));

            return GoogleApiResult<IEnumerable<string>>.Ok(usersList.AsEnumerable());
        }
        catch (System.Exception e)
        {
            return GoogleApiResult<IEnumerable<string>>.Fail(e.Message);
        }
    }

    // private GoogleCredential CreateCredential() =>
    //     CredentialFactory.FromFile<ServiceAccountCredential>(CredentialFilePath)
    //         .ToGoogleCredential()
    //         .CreateScoped(SCOPES)
    //         .CreateWithUser(UserEmailToImpersonate);

    private DirectoryService CreateDirectoryService()
    {
        GoogleCredential credential = GoogleCredential.FromFile(CredentialFilePath);
        credential = credential.CreateScoped(SCOPES).CreateWithUser(UserEmailToImpersonate);

        // Use the credential to authenticate your API requests.
        DirectoryService service = new DirectoryService(new BaseClientService.Initializer()
        {
            HttpClientInitializer = credential,
            ApplicationName = ApplicationName,
        });
        return service;
    }

    private GmailService CreateGmailService()
    {
        GoogleCredential credential = GoogleCredential.FromFile(CredentialFilePath);
        credential = credential.CreateScoped(GMAIL_SCOPES).CreateWithUser(UserEmailToImpersonate);

        GmailService service = new GmailService(new BaseClientService.Initializer()
        {
            HttpClientInitializer = credential,
            ApplicationName = ApplicationName,
        });
        return service;
    }

    private CalendarService CreateCalendarService()
    {
        GoogleCredential credential = GoogleCredential.FromFile(CredentialFilePath);
        credential = credential.CreateScoped(SCOPES).CreateWithUser(UserEmailToImpersonate);

        // Use the credential to authenticate your API requests.
        CalendarService service = new CalendarService(new BaseClientService.Initializer()
        {
            HttpClientInitializer = credential,
            ApplicationName = ApplicationName,
        });
        return service;
    }

    public async Task<GoogleApiResult<bool>> UserExists(string email)
    {
        try
        {
            DirectoryService service = _directoryService.Value;

            string memberId = email;
            var memberRequest = service.Users.Get(memberId);
            var m = await memberRequest.ExecuteAsync();
            return GoogleApiResult<bool>.Ok(m != null);

        }
        catch (GoogleApiException e)
        {
            if (e.Error.Code == 404)
            {
                return GoogleApiResult<bool>.Ok(false);
            }
            return GoogleApiResult<bool>.Fail(e.Message);
        }

    }

    public async Task<GoogleApiResult<bool>> SetPassword(string email, string password, bool changePasswordNexLogin = true)
    {
        try
        {
            DirectoryService service = _directoryService.Value;
            var userRequest = service.Users.Get(email);
            var user = await userRequest.ExecuteAsync();
            if (user == null) return GoogleApiResult<bool>.Fail("No s'ha trobat l'usuari");

            user.Password = password;
            user.ChangePasswordAtNextLogin = changePasswordNexLogin;

            var updateRequest = service.Users.Update(user, email);
            user = await updateRequest.ExecuteAsync();

            return GoogleApiResult<bool>.Ok(user != null);
        }
        catch (GoogleApiException e)
        {
            if (e.Error.Code == 404)
            {
                return GoogleApiResult<bool>.Fail("No s'ha trobat l'usuari");
            }
            return GoogleApiResult<bool>.Fail(e.Message);
        }
    }

    public async Task<GoogleApiResult<bool>> SetUserStatus(string email, bool active)
    {
        DirectoryService service = _directoryService.Value;

        string memberId = email;
        var userRequest = service.Users.Get(memberId);
        var user = await userRequest.ExecuteAsync();
        if (user == null)
        {
            return GoogleApiResult<bool>.Fail("user not found");
        }

        user.Suspended = !active;

        var updateRequest = service.Users.Update(user, email);
        user = await updateRequest.ExecuteAsync();

        return GoogleApiResult<bool>.Ok(user != null);
    }


    public async Task<GoogleApiResult<string>> CreateCalendarEvent(
        string calendarId,
        string summary,
        string description,
        DateTimeOffset start,
        DateTimeOffset end
        )
    {
        try
        {
            CalendarService service = _calendarService.Value;

            Event calendarEvent = new Event()
            {
                Summary = summary,
                Description = description,
                Start = new EventDateTime() { DateTimeDateTimeOffset = start },
                End = new EventDateTime() { DateTimeDateTimeOffset = end },
                ColorId = CALENDAR_EVENT_COLOR_ID,
            };

            Event result = await service.Events.Insert(calendarEvent, calendarId).ExecuteAsync();
            if (result.Id == null)
            {
                return GoogleApiResult<string>.Fail("Error creating calendar event");
            }

            return GoogleApiResult<string>.Ok(result.Id);
        }
        catch (System.Exception e)
        {
            return GoogleApiResult<string>.Fail(e.Message);
        }
    }

    public async Task<GoogleApiResult<bool>> UpdateCalendarEvent(
        string calendarId,
        string eventId,
        string summary,
        string description,
        DateTimeOffset start,
        DateTimeOffset end
        )
    {
        try
        {
            CalendarService service = _calendarService.Value;

            Event calendarEvent = new Event()
            {
                Summary = summary,
                Description = description,
                Start = new EventDateTime() { DateTimeDateTimeOffset = start },
                End = new EventDateTime() { DateTimeDateTimeOffset = end },
                ColorId = CALENDAR_EVENT_COLOR_ID,
            };

            Event result = await service.Events.Update(calendarEvent, calendarId, eventId).ExecuteAsync();
            return GoogleApiResult<bool>.Ok(result != null);
        }
        catch (GoogleApiException apiEx) when (apiEx.Error?.Code == 404 || apiEx.Error?.Code == 410)
        {
            return GoogleApiResult<bool>.Fail("not_found");
        }
        catch (Exception e)
        {
            return GoogleApiResult<bool>.Fail(e.Message);
        }
    }

    public async Task<GoogleApiResult<bool>> DeleteCalendarEvent(string calendarId, string eventId)
    {
        try
        {
            CalendarService service = _calendarService.Value;
            await service.Events.Delete(calendarId, eventId).ExecuteAsync();
            return GoogleApiResult<bool>.Ok(true);
        }
        catch (GoogleApiException apiEx) when (apiEx.Error?.Code == 404 || apiEx.Error?.Code == 410)
        {
            // Event already deleted or never existed; treat as success.
            return GoogleApiResult<bool>.Ok(true);
        }
        catch (Exception e)
        {
            return GoogleApiResult<bool>.Fail(e.Message);
        }
    }

    public async Task<GoogleApiResult<string>> SendHtmlEmail(string to, string subject, string htmlBody, CancellationToken ct)
    {
        try
        {
            GmailService service = _gmailService.Value;

            GmailMessage message = new GmailMessage()
            {
                Raw = ToBase64Url(Encoding.UTF8.GetBytes(BuildMimeMessage(to, subject, htmlBody))),
            };

            GmailMessage result = await service.Users.Messages.Send(message, GMAIL_AUTHENTICATED_USER).ExecuteAsync(ct);
            if (result.Id == null)
            {
                return GoogleApiResult<string>.Fail("Error enviant el correu");
            }

            _logger.LogInformation("Correu enviat a {To} amb assumpte '{Subject}' (id {MessageId})", to, subject, result.Id);
            return GoogleApiResult<string>.Ok(result.Id);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error enviant el correu a {To}", to);
            return GoogleApiResult<string>.Fail(e.Message);
        }
    }

    // El cos i l'assumpte es codifiquen en base64 perquè els accents no depenguin del transport.
    private string BuildMimeMessage(string to, string subject, string htmlBody)
    {
        string from = string.IsNullOrWhiteSpace(EmailSenderName)
            ? UserEmailToImpersonate
            : $"{EncodeHeader(EmailSenderName)} <{UserEmailToImpersonate}>";

        StringBuilder mime = new StringBuilder();
        mime.Append($"From: {from}\r\n");
        mime.Append($"To: {to}\r\n");
        mime.Append($"Subject: {EncodeHeader(subject)}\r\n");
        mime.Append("MIME-Version: 1.0\r\n");
        mime.Append("Content-Type: text/html; charset=UTF-8\r\n");
        mime.Append("Content-Transfer-Encoding: base64\r\n");
        mime.Append("\r\n");
        mime.Append(Convert.ToBase64String(Encoding.UTF8.GetBytes(htmlBody), Base64FormattingOptions.InsertLineBreaks));

        return mime.ToString();
    }

    // RFC 2047: capçaleres amb caràcters no ASCII.
    private static string EncodeHeader(string value)
    {
        return $"=?UTF-8?B?{Convert.ToBase64String(Encoding.UTF8.GetBytes(value))}?=";
    }

    private static string ToBase64Url(byte[] bytes)
    {
        return Convert.ToBase64String(bytes)
            .Replace('+', '-')
            .Replace('/', '_')
            .Replace("=", string.Empty);
    }
}
