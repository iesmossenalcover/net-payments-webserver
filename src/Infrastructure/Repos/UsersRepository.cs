using Domain.Entities.Authentication;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repos;

public class UserRepository : Repository<User>, Application.Common.Services.IUsersRepository
{
    public UserRepository(AppDbContext dbContext) : base(dbContext, dbContext.Users) {}

    public Task<User?> GetUserByUsernameAsync(string username, CancellationToken ct)
    {
        return _dbSet.FirstOrDefaultAsync(x => x.Username == username);
    }
}