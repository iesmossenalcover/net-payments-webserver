using Domain.Entities.Events;

namespace Application.Common.Services;

public interface IEventsRespository : IRepository<Event>
{
    Task<Event?> GetEventByCodeAsync(string code, CancellationToken ct);
    Task<IEnumerable<Event>> GetAllEventsByCourseIdAsync(long courseId, CancellationToken ct);
}