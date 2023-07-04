using Domain.Entities.People;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repos;

public class GroupsRepository : Repository<Group>, Domain.Services.IGroupsRepository
{
    public GroupsRepository(AppDbContext dbContext) : base(dbContext, dbContext.Groups) { }

    public async Task<IEnumerable<Group>> GetGroupsByNameAsync(IEnumerable<string> names, CancellationToken ct)
    {
        return await _dbSet.Where(x => names.Distinct().Contains(x.Name)).ToListAsync(ct);
    }
}