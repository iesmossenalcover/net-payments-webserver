using Domain.Services;
using Domain.Entities.GoogleApi;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repos;

public class UoGroupRelationRepository : Repository<OuGroupRelation>, IOUGroupRelationsRepository
{
    public UoGroupRelationRepository(AppDbContext dbContext) : base(dbContext, dbContext.UoGroupRelations)
    {
    }

    public Task<OuGroupRelation?> GetByGroupIdAsync(long groupId, CancellationToken ct)
    {
        return _dbSet.FirstOrDefaultAsync(x => x.GroupId == groupId, ct);
    }

    public async Task<IEnumerable<OuGroupRelation>> GetAllWithRelationsAsync(CancellationToken ct)
    {
        return await _dbSet
            .Include(x => x.Group)
            .ToListAsync(ct);
    }
}