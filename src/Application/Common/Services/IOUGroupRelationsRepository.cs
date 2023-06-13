namespace Application.Common.Services;

public interface IOUGroupRelationsRepository
{
    public Task<Domain.Entities.GoogleApi.UoGroupRelation?> GetByGroupIdAsync(long groupId, CancellationToken ct);
}