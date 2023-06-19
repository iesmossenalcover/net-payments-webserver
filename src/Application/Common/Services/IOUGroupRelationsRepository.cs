using Domain.Entities.GoogleApi;

namespace Application.Common.Services;

public interface IOUGroupRelationsRepository  : IRepository<UoGroupRelation>
{
    public Task<UoGroupRelation?> GetByGroupIdAsync(long groupId, CancellationToken ct);
}