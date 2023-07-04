using Domain.Entities.Authentication;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repos;

public class UserRepository : Repository<User>, Domain.Services.IUsersRepository
{
    public UserRepository(AppDbContext dbContext) : base(dbContext, dbContext.Users) {}

    public Task<User?> GetUserByUsernameAsync(string username, CancellationToken ct)
    {
        return _dbSet
            .Include(x => x.UserClaims)
            .FirstOrDefaultAsync(x => x.Username == username);
    }
}