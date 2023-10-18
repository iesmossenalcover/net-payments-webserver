using Domain.Entities.Events;

namespace Domain.Services;

public interface IEventsRespository : IRepository<Event>
{
    Task<Event?> GetEventByCodeAsync(string code, CancellationToken ct);
    Task<IEnumerable<Event>> GetAllEventsByCourseIdAsync(long courseId, CancellationToken ct);
    Task<IEnumerable<Event>> GetAllUnexpiredEventsByCourseIdAsync(long courseId, CancellationToken ct);
}