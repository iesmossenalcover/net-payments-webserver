using Domain.Entities.Events;

namespace Application.Common.Services;

public interface IEventsPeopleRespository : IRepository<EventPerson>
{
    Task<IEnumerable<EventPerson>> GetAllByEventIdAsync(long eventId, CancellationToken ct);
}