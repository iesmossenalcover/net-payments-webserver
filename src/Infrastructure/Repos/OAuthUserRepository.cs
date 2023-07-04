using Domain.Entities.Authentication;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repos;

public class OAuthUserRepository : Repository<OAuthUser>, Domain.Services.IOAuthUsersRepository
{
    public OAuthUserRepository(AppDbContext dbContext) : base(dbContext, dbContext.OAuthUsers) {}

    public Task<OAuthUser?> GetUserBySubjectAndCodeAsync(string subject, string oauthCodeProvider, CancellationToken ct)
    {
        return _dbSet
            .Include(x => x.User)
            .ThenInclude(x => x.UserClaims)
            .FirstOrDefaultAsync(x => x.OAuthProviderCode == oauthCodeProvider && x.Subject == subject, ct);
    }
}