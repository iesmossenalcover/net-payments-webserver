using Domain.Entities.Authentication;

namespace Domain.Services;

public interface IUsersRepository : IRepository<User>
{
    public Task<User?> GetUserByUsernameAsync(string username, CancellationToken ct);
}