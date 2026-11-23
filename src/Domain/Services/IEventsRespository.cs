using Domain.Entities.Events;

namespace Domain.Services;

public interface IEventsRespository : IRepository<Event>
{
    Task<Event?> GetEventByCodeAsync(string code, CancellationToken ct);
    Task<IEnumerable<Event>> GetAllEventsByCourseIdAsync(long courseId, CancellationToken ct);
    Task<IEnumerable<Event>> GetAllUnexpiredEventsByCourseIdAsync(long courseId, CancellationToken ct);
    Task<IEnumerable<Event>> GetEventsStartingBetweenAsync(DateTimeOffset from, DateTimeOffset to, CancellationToken ct);
    /// <summary>Esdeveniments que ocupen algun moment de [from, to] (comencen abans i acaben dins també compten).</summary>
    Task<IEnumerable<Event>> GetEventsOverlappingAsync(DateTimeOffset from, DateTimeOffset to, CancellationToken ct);
    Task<IEnumerable<Event>> GetEventsPendingPublicationNoticeAsync(DateTimeOffset now, CancellationToken ct);
}