using Domain.Entities.Authentication;

namespace Domain.Services;

public interface IOAuthUsersRepository : IRepository<OAuthUser>
{
    public Task<OAuthUser?> GetUserBySubjectAndCodeAsync(string subject, string oauthCodeProvider, CancellationToken ct);
}