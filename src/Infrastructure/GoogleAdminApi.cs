using Application.Common.Models;
using Domain.Services;
using Domain.Entities.Authentication;
using Google.Apis.Admin.Directory.directory_v1;
using Google.Apis.Admin.Directory.directory_v1.Data;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Requests;
using Google.Apis.Services;
using Google;

namespace Infrastructure;

public class GoogleAdminApi : IGoogleAdminApi
{
    private const int GOOGLE_API_ERROR_CONFLICT = 409;

    private static readonly string[] SCOPES = new string[]
    {
        DirectoryService.Scope.AdminDirectoryUser,
        DirectoryService.Scope.AdminDirectoryGroupMember,
        DirectoryService.Scope.AdminDirectoryGroup,
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
        DirectoryService service = CreateService();

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
            DirectoryService service = CreateService();
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

            return new GoogleApiResult<bool>(true);
        }
        catch (Exception e)
        {
            return new GoogleApiResult<bool>(e.Message);
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
            DirectoryService service = CreateService();
            newUser = await service.Users.Insert(newUser).ExecuteAsync();
            return new GoogleApiResult<bool>(true);
        }
        catch (System.Exception e)
        {
            return new GoogleApiResult<bool>(e.Message);
        }
    }


    public async Task<GoogleApiResult<bool>> AddUserToGroup(string email, string group)
    {
        try
        {
            DirectoryService service = CreateService();

            string memberId = email;
            var memberRequest = service.Users.Get(memberId);
            var m = await memberRequest.ExecuteAsync();
            if (m == null)
            {
                return new GoogleApiResult<bool>("user not found");
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
                return new GoogleApiResult<bool>("group not found");
            }


            var addRequest = service.Members.Insert(member, gp.Id);
            member = await addRequest.ExecuteAsync();
            return new GoogleApiResult<bool>(true);
        }
        catch (GoogleApiException apiEx)
        {
            // https://developers.google.com/webmaster-tools/v1/errors
            return apiEx.Error.Code switch
            {
                GOOGLE_API_ERROR_CONFLICT => new GoogleApiResult<bool>(true),
                _ => new GoogleApiResult<bool>(apiEx.Error.Message),
            };
        }
        catch (Exception e)
        {
            return new GoogleApiResult<bool>(e.Message);
        }
    }

    public async Task<GoogleApiResult<bool>> RemoveUserFromGroup(string email, string group)
    {
        try
        {
            DirectoryService service = CreateService();

            string memberId = email;
            var memberRequest = service.Users.Get(memberId);
            var m = await memberRequest.ExecuteAsync();
            if (m == null)
            {
                return new GoogleApiResult<bool>("user not found");
            }


            string groupId = group;
            var groupRequest = service.Groups.Get(groupId);
            var gp = await groupRequest.ExecuteAsync();
            if (gp == null)
            {
                return new GoogleApiResult<bool>("group not found");
            }

            MembersResource.GetRequest getRequest = service.Members.Get(groupId, email);
            Member member = await getRequest.ExecuteAsync();

            // Delete the member from the group.
            await service.Members.Delete(groupId, member.Id).ExecuteAsync();

            return new GoogleApiResult<bool>(true);
        }
        catch (System.Exception e)
        {
            return new GoogleApiResult<bool>(e.Message);
        }
    }

    public async Task<GoogleApiResult<bool>> ClearGroupMembers(string group)
    {
        try
        {
            _logger.LogInformation("Start ClearGroupMembers");
            DirectoryService service = CreateService();

            string groupId = group;
            var groupRequest = service.Groups.Get(groupId);
            var gp = await groupRequest.ExecuteAsync();
            if (gp == null)
            {
                return new GoogleApiResult<bool>("group not found");
            }

            _logger.LogInformation($"Group {gp.Email}");


            // Delete each member from the group.
            try
            {
                MembersResource.ListRequest listRequest = service.Members.List(groupId);
                Members members;
                do
                {
                    members = await listRequest.ExecuteAsync();
                    foreach (Member member in members.MembersValue)
                    {
                        /*
                            https://developers.google.com/admin-sdk/directory/v1/guides/manage-group-members?hl=es-419
                            {
                                "kind": "directory#member",
                                "id": "group member's unique ID",
                                "email": "liz@example.com",
                                "role": "MEMBER",
                                "type": "GROUP"
                            }
                            El type de un miembro del grupo puede ser:

                            GROUP: el miembro es otro grupo.
                            MEMBER: el miembro es un usuario
                        */
                        _logger.LogInformation($"Member {member.Email}, type: {member.Type}");
                        if (member.Type != "USER") continue;
                        var deleteResponse = await service.Members.Delete(groupId, member.Id).ExecuteAsync();
                        _logger.LogInformation($"Delete response {deleteResponse}");
                    }
                    listRequest.PageToken = members.NextPageToken;
                }
                while (!string.IsNullOrEmpty(members.NextPageToken));

                return new GoogleApiResult<bool>(true);
            }
            catch (Exception)
            {

                return new GoogleApiResult<bool>("Error");
            }

        }
        catch (System.Exception e)
        {
            return new GoogleApiResult<bool>(e.Message);
        }
    }

    public async Task<GoogleApiResult<bool>> MoveUserToOU(string email, string ouPath)
    {
        try
        {
            DirectoryService service = CreateService();
            string memberId = email;
            var memberRequest = service.Users.Get(memberId);
            var member = await memberRequest.ExecuteAsync();
            if (member == null)
            {
                return new GoogleApiResult<bool>("user not found");
            }

            member.OrgUnitPath = ouPath;

            var updateRequest = service.Users.Update(member, memberId);
            member = await updateRequest.ExecuteAsync();
            return new GoogleApiResult<bool>(true);
        }
        catch (System.Exception e)
        {
            return new GoogleApiResult<bool>(e.Message);
        }
    }

    public async Task<GoogleApiResult<IEnumerable<string>>> GetAllUsers(string ouPath)
    {
        try
        {
            DirectoryService service = CreateService();
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

            return new GoogleApiResult<IEnumerable<string>>(usersList.AsEnumerable());
        }
        catch (System.Exception e)
        {
            return new GoogleApiResult<IEnumerable<string>>(e.Message);
        }
    }

    private DirectoryService CreateService()
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

    public async Task<GoogleApiResult<bool>> UserExists(string email)
    {
        try
        {
            DirectoryService service = CreateService();

            string memberId = email;
            var memberRequest = service.Users.Get(memberId);
            var m = await memberRequest.ExecuteAsync();
            return new GoogleApiResult<bool>(m != null);

        }
        catch (GoogleApiException e)
        {
            if (e.Error.Code == 404)
            {
                return new GoogleApiResult<bool>(false);
            }
            return new GoogleApiResult<bool>(e.Message);
        }

    }

    public async Task<GoogleApiResult<bool>> SetPassword(string email, string password, bool changePasswordNexLogin = true)
    {
        try
        {
            DirectoryService service = CreateService();
            var userRequest = service.Users.Get(email);
            var user = await userRequest.ExecuteAsync();
            if (user == null) return new GoogleApiResult<bool>("No s'ha trobat l'usuari");

            user.Password = password;
            user.ChangePasswordAtNextLogin = changePasswordNexLogin;

            var updateRequest = service.Users.Update(user, email);
            user = await updateRequest.ExecuteAsync();

            return new GoogleApiResult<bool>(user != null);
        }
        catch (GoogleApiException e)
        {
            if (e.Error.Code == 404)
            {
                return new GoogleApiResult<bool>("No s'ha trobat l'usuari");
            }
            return new GoogleApiResult<bool>(e.Message);
        }
    }

    public async Task<GoogleApiResult<bool>> SetUserStatus(string email, bool active)
    {
        DirectoryService service = CreateService();

        string memberId = email;
        var userRequest = service.Users.Get(memberId);
        var user = await userRequest.ExecuteAsync();
        if (user == null)
        {
            return new GoogleApiResult<bool>("user not found");
        }

        user.Suspended = !active;

        var updateRequest = service.Users.Update(user, email);
        user = await updateRequest.ExecuteAsync();

        return new GoogleApiResult<bool>(user != null);
    }
}