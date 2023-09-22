using Domain.Entities.GoogleApi;

namespace Domain.Services;

public interface IOUGroupRelationsRepository  : IRepository<OuGroupRelation>
{
    public Task<OuGroupRelation?> GetByGroupIdAsync(long groupId, CancellationToken ct);
}