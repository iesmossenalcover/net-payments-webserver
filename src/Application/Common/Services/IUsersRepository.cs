using Domain.Entities.Authentication;

namespace Application.Common.Services;

public interface IUsersRepository : IRepository<User>
{
    public Task<User?> GetUserByUsernameAsync(string username, CancellationToken ct);
}