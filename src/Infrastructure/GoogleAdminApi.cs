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

namespace Infrastructure;

public class GoogleAdminApi : IGoogleAdminApi
{
    private const int GOOGLE_API_ERROR_CONFLICT = 409;

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

    private readonly string CredentialFilePath;
    private readonly string UserEmailToImpersonate;
    private readonly string Domain;
    private readonly string ApplicationName;
    private readonly string SuperuserGroupEmail;
    private readonly string AdminGroupEmail;
    private readonly string ReaderGroupEmail;
    private readonly string[] excludeEmails;
    private readonly ILogger _logger;


    public GoogleAdminApi(IConfiguration configuration, ILogger<GoogleAdminApi> logger)
    {
        CredentialFilePath = configuration.GetValue<string>("GoogleApiCredentialFilePath") ?? throw new Exception("GoogleApiCredentialFilePath");
        UserEmailToImpersonate = configuration.GetValue<string>("GoogleApiUserEmailToImpersonate") ?? throw new Exception("GoogleApiUserEmailToImpersonate");
        ApplicationName = configuration.GetValue<string>("GoogleApiApplicationName") ?? throw new Exception("GoogleApiApplicationName");
        SuperuserGroupEmail = configuration.GetValue<string>("GoogleApiSuperuserGroupEmail") ?? throw new Exception("GoogleApiSuperuserGroupEmail");
        AdminGroupEmail = configuration.GetValue<string>("GoogleApiAdminGroupEmail") ?? throw new Exception("GoogleApiAdminGroupEmail");
        ReaderGroupEmail = configuration.GetValue<string>("GoogleApiEmailGroupReader") ?? throw new Exception("GoogleApiEmailGroupReader");
        Domain = configuration.GetValue<string>("GoogleApiDomain") ?? throw new Exception("GoogleApiDomain");
        excludeEmails = configuration.GetValue<string>("GoogleApiExcludeAccounts")?.Split(" ") ?? throw new Exception("GoogleApiExcludeAccounts");
        _logger = logger;
    }

    public async Task<IEnumerable<string>> GetUserClaims(string email, CancellationToken ct)
    {
        DirectoryService service = CreateDirectoryService();

        var request = service.Groups.List();
        request.UserKey = email;

        // TODO: move this mapping to db. For the moment hard coded.
        bool isReader = false;
        bool isAdmin = false;
        bool isSuperuser = false;
        try
        {
            var readerResponseTask = service.Members.HasMember(ReaderGroupEmail, email).ExecuteAsync(ct);
            var adminResponseTask = service.Members.HasMember(AdminGroupEmail, email).ExecuteAsync(ct);
            var superuserResponseTask = service.Members.HasMember(SuperuserGroupEmail, email).ExecuteAsync(ct);
            Task.WaitAll(readerResponseTask, adminResponseTask, superuserResponseTask);

            var readerResponse = await readerResponseTask;
            var adminResponse = await adminResponseTask;
            var superUserResponse = await superuserResponseTask;

            isReader = readerResponse.IsMember ?? false;
            isAdmin = adminResponse.IsMember ?? false;
            isSuperuser = superUserResponse.IsMember ?? false;
        }
        catch (System.Exception)
        { }


        var claims = new List<string>(1);
        if (isSuperuser)
        {
            claims.Add(RoleClaimValues.SUPER_USER);
        }
        else if (isAdmin)
        {
            claims.Add(RoleClaimValues.ADMIN);
        }
        else if (isReader)
        {
            claims.Add(RoleClaimValues.READER);
        }

        return claims;
    }

    public async Task<GoogleApiResult<bool>> SetSuspendByOU(
        string ouPath,
        bool suspend,
        bool exactOu
        )
    {
        try
        {
            DirectoryService service = CreateDirectoryService();
            UsersResource.ListRequest userListRequest = service.Users.List();
            userListRequest.Query = $"orgUnitPath='{ouPath}'";
            userListRequest.Domain = Domain;
            userListRequest.MaxResults = 50;
            Users users;
            List<Google.Apis.Admin.Directory.directory_v1.Data.User> usersToProcess = new List<Google.Apis.Admin.Directory.directory_v1.Data.User>();

            _logger.LogInformation("Start SetSuspendByOU");
            do
            {
                users = await userListRequest.ExecuteAsync();
                if (users.UsersValue != null)
                {
                    usersToProcess.AddRange(users.UsersValue);
                    userListRequest.PageToken = users.NextPageToken;
                }
            }
            while (!string.IsNullOrEmpty(users.NextPageToken));

            _logger.LogInformation($"Users to suspend: {usersToProcess.Count}");

            int batchSize = 500;
            _logger.LogInformation($"Num batch {usersToProcess.Count / batchSize}");
            for (int i = 0; i < usersToProcess.Count; i += batchSize)
            {
                var batchList = usersToProcess.Skip(i).Take(batchSize);

                var batchRequest = new BatchRequest(service);
                foreach (var user in batchList)
                {
                    // IMPORTANT: Exclude members
                    if (excludeEmails.Contains(user.PrimaryEmail)) continue;


                    // If we want exactOU, orga path must be the same, not descdendant.
                    if ((user.OrgUnitPath == ouPath || !exactOu) && user.Suspended == false)
                    {
                        user.Suspended = suspend;
                        _logger.LogInformation($"Queue user {user.PrimaryEmail}");
                        batchRequest.Queue(service.Users.Update(user, user.Id),
                        (UsersResource.UpdateRequest content, RequestError error, int index, HttpResponseMessage message) =>
                        {
                            _logger.LogInformation($"Callback: {user.PrimaryEmail} Error?: {error?.Message} Message:? {message.Content}");
                        });
                    }
                }
                await batchRequest.ExecuteAsync();
                _logger.LogInformation($"Batch executed");
            }

            return GoogleApiResult<bool>.Ok(true);
        }
        catch (Exception e)
        {
            return GoogleApiResult<bool>.Fail(e.Message);
        }
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
            DirectoryService service = CreateDirectoryService();
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
            DirectoryService service = CreateDirectoryService();

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
            DirectoryService service = CreateDirectoryService();

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
            DirectoryService service = CreateDirectoryService();

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
                Members response = await listRequest.ExecuteAsync();
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
            List<string> errors = new();

            foreach (Member member in members)
            {
                /*
                    https://developers.google.com/admin-sdk/directory/v1/guides/manage-group-members?hl=es-419
                    El type d'un membre del grup pot ser:
                    GROUP: el membre és un altre grup.
                    USER: el membre és un usuari.
                */
                if (member.Type != "USER")
                {
                    skipped++;
                    _logger.LogInformation(
                        "ClearGroupMembers: grup {Group}, membre {Member} ignorat (type: {Type})",
                        group, member.Email, member.Type);
                    continue;
                }

                try
                {
                    await service.Members.Delete(group, member.Id).ExecuteAsync();
                    removed++;
                    _logger.LogInformation(
                        "ClearGroupMembers: grup {Group}, membre {Member} (id: {MemberId}) esborrat",
                        group, member.Email, member.Id);
                }
                catch (GoogleApiException apiEx) when (apiEx.Error?.Code == 404 || apiEx.Error?.Code == 410)
                {
                    skipped++;
                    _logger.LogWarning(
                        "ClearGroupMembers: grup {Group}, membre {Member} ja no hi era (codi {Code})",
                        group, member.Email, apiEx.Error?.Code);
                }
                catch (Exception e)
                {
                    errors.Add($"{member.Email}: {DescribeError(e)}");
                    _logger.LogError(e,
                        "ClearGroupMembers: error esborrant el membre {Member} del grup {Group}",
                        member.Email, group);
                }
            }

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
            DirectoryService service = CreateDirectoryService();
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
            DirectoryService service = CreateDirectoryService();
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
            DirectoryService service = CreateDirectoryService();

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
            DirectoryService service = CreateDirectoryService();
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
        DirectoryService service = CreateDirectoryService();

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
            CalendarService service = CreateCalendarService();

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
            CalendarService service = CreateCalendarService();

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
            CalendarService service = CreateCalendarService();
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
}
