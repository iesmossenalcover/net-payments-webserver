using Domain.Entities.GoogleApi;

namespace Infrastructure.Repos;

public class UoGroupRelationRepository : Repository<UoGroupRelation>
{
    public UoGroupRelationRepository(AppDbContext dbContext) : base(dbContext, dbContext.UoGroupRelations) { }
}