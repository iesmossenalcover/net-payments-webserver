using Domain.Services;
using Domain.Entities.GoogleApi;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repos;

public class UoGroupRelationRepository : Repository<OuGroupRelation>, IOUGroupRelationsRepository
{
    public UoGroupRelationRepository(AppDbContext dbContext) : base(dbContext, dbContext.UoGroupRelations) { }

    public Task<OuGroupRelation?> GetByGroupIdAsync(long groupId, CancellationToken ct)
    {
        return _dbSet.FirstOrDefaultAsync(x => x.GroupId == groupId);
    }
}