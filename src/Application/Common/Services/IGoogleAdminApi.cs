using Application.Common.Models;

namespace Application.Common.Services;

public interface IGoogleAdminApi
{
    public Task Test(CancellationToken ct);
    public Task<IEnumerable<string>> GetUserClaims(string email, CancellationToken ct);
    public Task<GoogleApiResult<bool>> SuspendByOU(string ouPath);
    public Task<GoogleApiResult<bool>> MoveUserToOU(string email, string ouPath);
    public Task<GoogleApiResult<bool>> CreateUser(
        string email,
        string firstName,
        string lastName,
        string password,
        string ouPath,
        bool changePasswordNexLogin = false
    );

}