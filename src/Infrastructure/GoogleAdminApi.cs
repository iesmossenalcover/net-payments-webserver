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
    private readonly string ApplicationName;
    private readonly string AdminGroupEmail;
    private readonly string ReaderGroupEmail;

    public GoogleAdminApi(IConfiguration configuration)
    {
        CredentialFilePath = configuration.GetValue<string>("GoogleApiCredentialFilePath") ?? throw new Exception("GoogleApiCredentialFilePath");
        UserEmailToImpersonate = configuration.GetValue<string>("GoogleApiUserEmailToImpersonate") ?? throw new Exception("GoogleApiUserEmailToImpersonate");
        ApplicationName = configuration.GetValue<string>("GoogleApiApplicationName") ?? throw new Exception("GoogleApiApplicationName");
        AdminGroupEmail = configuration.GetValue<string>("GoogleApiAdminGroupEmail") ?? throw new Exception("GoogleApiAdminGroupEmail");
        ReaderGroupEmail = configuration.GetValue<string>("GoogleApiEmailGroupReader") ?? throw new Exception("GoogleApiEmailGroupReader");
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
        try
        {
            var readerResponseTask = service.Members.HasMember(ReaderGroupEmail, email).ExecuteAsync(ct);
            var adminResponseTask = service.Members.HasMember(AdminGroupEmail, email).ExecuteAsync(ct);
            Task.WaitAll(readerResponseTask, adminResponseTask);

            var readerResponse = await readerResponseTask;
            var adminResponse = await adminResponseTask;

            isReader = readerResponse.IsMember ?? false;
            isAdmin = adminResponse.IsMember ?? false;
        }
        catch (System.Exception)
        { }


        var claims = new List<string>(2);
        if (isAdmin)
        {
            claims.Add(ClaimValues.ADMIN);
        }

        if (isReader)
        {
            claims.Add(ClaimValues.READER);
        }

        return claims;
    }
}