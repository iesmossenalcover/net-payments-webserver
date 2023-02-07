using Application.Common.Services;
using Domain.Entities.Authentication;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure
{
    public class AuthenticationService : IAuthenticationService
    {
        #region properties

        private readonly ApplicationDbContext _dbContext;

        public AuthenticationService(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }
        #endregion

        public Task<User?> GetUserAsync(string username, CancellationToken ct)
        {
           return _dbContext.Users.FirstOrDefaultAsync(x => x.Username == username);
        }

        public Task<User?> GetUserWithClaimsAsync(string username, CancellationToken ct)
        {
           return _dbContext
                    .Users
                    .Include(x => x.UserClaims)
                    .FirstOrDefaultAsync(x => x.Username == username);
        }

        public async Task<long> InsertUserAsync(User user, CancellationToken ct)
        {
            _dbContext.Users.Add(user);
            await _dbContext.SaveChangesAsync(ct);
            return user.Id;
        }

        public async Task UpdateUserAsync(User user, CancellationToken ct)
        {
            _dbContext.Update(user);
            await _dbContext.SaveChangesAsync(ct);
        }
    }
}