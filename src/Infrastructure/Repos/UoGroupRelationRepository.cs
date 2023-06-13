using Application.Common.Services;
using Domain.Entities.GoogleApi;

namespace Infrastructure.Repos;

public class UoGroupRelationRepository : Repository<UoGroupRelation>, IOUGroupRelationsRepository
{
    public UoGroupRelationRepository(AppDbContext dbContext) : base(dbContext, dbContext.UoGroupRelations) { }
}