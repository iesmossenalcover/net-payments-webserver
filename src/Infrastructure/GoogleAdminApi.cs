using Application.Common.Models;
using Application.Common.Services;
using Domain.Entities.Authentication;
using Google.Apis.Admin.Directory.directory_v1;
using Google.Apis.Admin.Directory.directory_v1.Data;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Requests;
using Google.Apis.Services;

namespace Infrastructure;

public class GoogleAdminApi : IGoogleAdminApi
{
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

    public GoogleAdminApi(IConfiguration configuration)
    {
        CredentialFilePath = configuration.GetValue<string>("GoogleApiCredentialFilePath") ?? throw new Exception("GoogleApiCredentialFilePath");
        UserEmailToImpersonate = configuration.GetValue<string>("GoogleApiUserEmailToImpersonate") ?? throw new Exception("GoogleApiUserEmailToImpersonate");
        ApplicationName = configuration.GetValue<string>("GoogleApiApplicationName") ?? throw new Exception("GoogleApiApplicationName");
        SuperuserGroupEmail = configuration.GetValue<string>("GoogleApiSuperuserGroupEmail") ?? throw new Exception("GoogleApiSuperuserGroupEmail");
        AdminGroupEmail = configuration.GetValue<string>("GoogleApiAdminGroupEmail") ?? throw new Exception("GoogleApiAdminGroupEmail");
        ReaderGroupEmail = configuration.GetValue<string>("GoogleApiEmailGroupReader") ?? throw new Exception("GoogleApiEmailGroupReader");
        Domain = configuration.GetValue<string>("GoogleApiDomain") ?? throw new Exception("GoogleApiDomain");
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

            var batchRequest = new BatchRequest(service);
            foreach (var user in usersToProcess)
            {
                if ((user.OrgUnitPath == ouPath || !exactOu) && user.Suspended == false)
                {
                    user.Suspended = suspend;
                    batchRequest.Queue(service.Users.Update(user, user.Id),
                    (UsersResource.UpdateRequest content, RequestError error, int index, HttpResponseMessage message) => {
                        // 
                    });
                }
            }
            await batchRequest.ExecuteAsync();

            return new GoogleApiResult<bool>(true);
        }
        catch (System.Exception e)
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
        catch (System.Exception e)
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
            DirectoryService service = CreateService();

            string groupId = group;
            var groupRequest = service.Groups.Get(groupId);
            var gp = await groupRequest.ExecuteAsync();
            if (gp == null)
            {
                return new GoogleApiResult<bool>("group not found");
            }

            MembersResource.ListRequest listRequest = service.Members.List(groupId);
            Members members = await listRequest.ExecuteAsync();

            // Delete each member from the group.
            try
            {
                if (members.MembersValue != null)
                {
                    foreach (Member member in members.MembersValue)
                    {
                        await service.Members.Delete(groupId, member.Id).ExecuteAsync();
                    }
                }

                return new GoogleApiResult<bool>(true);
            }
            catch (System.Exception)
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
        catch (Google.GoogleApiException e)
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
        catch (Google.GoogleApiException e)
        {
            if (e.Error.Code == 404)
            {
                return new GoogleApiResult<bool>("No s'ha trobat l'usuari");
            }
            return new GoogleApiResult<bool>(e.Message);
        }
    }
}