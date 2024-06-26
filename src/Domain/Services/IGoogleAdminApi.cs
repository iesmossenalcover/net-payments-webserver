using Application.Common.Models;

namespace Domain.Services;

public interface IGoogleAdminApi
{
    public Task<IEnumerable<string>> GetUserClaims(string email, CancellationToken ct);
    public Task<GoogleApiResult<bool>> SetSuspendByOU(string ouPath, bool suspend, bool exactOu);
    public Task<GoogleApiResult<bool>> UserExists(string email);
    public Task<GoogleApiResult<bool>> SetPassword(string email, string password, bool changePasswordNexLogin = true);
    public Task<GoogleApiResult<bool>> MoveUserToOU(string email, string ouPath);
    public Task<GoogleApiResult<bool>> SetUserStatus(string email, bool active);
    public Task<GoogleApiResult<IEnumerable<string>>> GetAllUsers(string ouPath);
    public Task<GoogleApiResult<bool>> AddUserToGroup(string email, string group);
    public Task<GoogleApiResult<bool>> ClearGroupMembers(string group);
    public Task<GoogleApiResult<bool>> RemoveUserFromGroup(string email, string group);
    public Task<GoogleApiResult<bool>> CreateUser(
        string email,
        string firstName,
        string lastName,
        string password,
        string ouPath,
        bool changePasswordNexLogin = true
    );

}