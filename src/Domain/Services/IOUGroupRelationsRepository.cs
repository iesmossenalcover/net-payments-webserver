using Domain.Entities.GoogleApi;

namespace Domain.Services;

public interface IOUGroupRelationsRepository  : IRepository<UoGroupRelation>
{
    public Task<UoGroupRelation?> GetByGroupIdAsync(long groupId, CancellationToken ct);
}