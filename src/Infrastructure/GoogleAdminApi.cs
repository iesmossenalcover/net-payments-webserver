using Application.Common.Services;
using Domain.Entities.Authentication;
using Google.Apis.Admin.Directory.directory_v1;
using Google.Apis.Admin.Directory.directory_v1.Data;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;

namespace Infrastructure;

public class GoogleAdminApi : IGoogleAdminApi
{
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
        GoogleCredential credential = GoogleCredential.FromFile(CredentialFilePath);
        // Specify the scope of access and enable domain-wide delegation.
        string[] scopes = new string[]
        {
            DirectoryService.Scope.AdminDirectoryUser,
            DirectoryService.Scope.AdminDirectoryGroupMember,
            DirectoryService.Scope.AdminDirectoryGroup,
        };

        credential = credential.CreateScoped(scopes).CreateWithUser(UserEmailToImpersonate);

        // Use the credential to authenticate your API requests.
        DirectoryService service = new DirectoryService(new BaseClientService.Initializer()
        {
            HttpClientInitializer = credential,
            ApplicationName = ApplicationName,
        });
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


        var claims = new List<string>(2);
        if (isSuperuser)
        {
            claims.Add(ClaimValues.SUPER_USER);
        }
        else if (isAdmin)
        {
            claims.Add(ClaimValues.ADMIN);
        }
        else if (isReader)
        {
            claims.Add(ClaimValues.READER);
        }

        return claims;
    }

    public async Task Test(CancellationToken ct)
    {
        GoogleCredential credential = GoogleCredential.FromFile(CredentialFilePath);
        // Specify the scope of access and enable domain-wide delegation.
        string[] scopes = new string[]
        {
            DirectoryService.Scope.AdminDirectoryUser,
            DirectoryService.Scope.AdminDirectoryGroupMember,
            DirectoryService.Scope.AdminDirectoryGroup,
        };

        credential = credential.CreateScoped(scopes).CreateWithUser(UserEmailToImpersonate);

        // Use the credential to authenticate your API requests.
        DirectoryService service = new DirectoryService(new BaseClientService.Initializer()
        {
            HttpClientInitializer = credential,
            ApplicationName = ApplicationName,
        });

        // Move to UO
        // ApiResult<bool> response = await MoveUserToOU(service, "noreply@iesmossenalcover.cat", "/exalumnes");
        // var userResult = await CreateUser(service, "test1234@iesmossenalcover.cat", "Prova", "Prova1 Prova2", "12345678", "/alumnes/bat/1rbat/grup L");
        // Console.WriteLine(userResult.Success);
        var suspendResult = await SuspendByOU(service, "/alumnes/BAT/1rbat/grup L");
        Console.WriteLine(suspendResult.Success);
    }

    private async Task<ApiResult<bool>> SuspendByOU(
        DirectoryService service,
        string ouPath
        )
    {
        try
        {
            UsersResource.ListRequest userListRequest = service.Users.List();
            userListRequest.Query = $"orgUnitPath='{ouPath}'";
            userListRequest.Domain = Domain;
            userListRequest.MaxResults = 50;
            Users users;
            do
            {
                users = await userListRequest.ExecuteAsync();

                foreach (var user in users.UsersValue)
                {
                    if (user.OrgUnitPath == ouPath && user.Suspended == false)
                    {
                        user.Suspended = true;
                        await service.Users.Update(user, user.Id).ExecuteAsync();
                    }
                }
                userListRequest.PageToken = users.NextPageToken;
            }
            while (!string.IsNullOrEmpty(users.NextPageToken));

            return new ApiResult<bool>(true);
        }
        catch (System.Exception e)
        {
            return new ApiResult<bool>(e.Message);
        }
    }

    private async Task<ApiResult<Google.Apis.Admin.Directory.directory_v1.Data.User>> CreateUser(
        DirectoryService service,
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
            newUser = await service.Users.Insert(newUser).ExecuteAsync();
            return new ApiResult<Google.Apis.Admin.Directory.directory_v1.Data.User>(newUser);
        }
        catch (System.Exception e)
        {
            return new ApiResult<Google.Apis.Admin.Directory.directory_v1.Data.User>(e.Message);
        }
    }

    private async Task<ApiResult<bool>> MoveUserToOU(DirectoryService service, string email, string ouPath)
    {
        try
        {
            string memberId = email;
            var memberRequest = service.Users.Get(memberId);
            var member = await memberRequest.ExecuteAsync();
            if (member == null)
            {
                return new ApiResult<bool>("user not found");
            }

            member.OrgUnitPath = ouPath;

            var updateRequest = service.Users.Update(member, memberId);
            member = await updateRequest.ExecuteAsync();
            return new ApiResult<bool>(true);
        }
        catch (System.Exception e)
        {
            return new ApiResult<bool>(e.Message);
        }
    }

    private class ApiResult<T>
    {
        public bool Success { get; set; }
        public string? ErrorMessage { get; set; } = null;
        public T? Data { get; set; }

        public ApiResult(string message)
        {
            Success = false;
            ErrorMessage = message;
        }

        public ApiResult(T data)
        {
            Success = true;
            Data = data;
        }
    }
}