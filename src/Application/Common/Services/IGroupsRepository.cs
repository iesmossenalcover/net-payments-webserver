using Domain.Entities.People;

namespace Application.Common.Services;

public interface IGroupsRepository : IRepository<Group>
{
    public Task<IEnumerable<Group>> GetGroupsByNameAsync(IEnumerable<string> names, CancellationToken ct);
}