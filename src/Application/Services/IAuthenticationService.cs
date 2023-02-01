using Domain.Authentication;

namespace Application.Services;

public interface IAuthenticationService
{
    public Task<User?> GetUserAsync(string username, CancellationToken ct);
    public Task<User?> GetUserWithClaimsAsync(string username, CancellationToken ct);
    public Task<long> InsertUserAsync(User user, CancellationToken ct);
    public Task UpdateUserAsync(User user, CancellationToken ct);
}