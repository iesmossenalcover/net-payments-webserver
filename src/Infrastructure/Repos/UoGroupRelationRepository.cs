using Application.Common.Services;
using Domain.Entities.GoogleApi;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repos;

public class UoGroupRelationRepository : Repository<UoGroupRelation>, IOUGroupRelationsRepository
{
    public UoGroupRelationRepository(AppDbContext dbContext) : base(dbContext, dbContext.UoGroupRelations) { }

    public Task<UoGroupRelation?> GetByGroupIdAsync(long groupId, CancellationToken ct)
    {
        return _dbSet.FirstOrDefaultAsync(x => x.GroupId == groupId);
    }
}