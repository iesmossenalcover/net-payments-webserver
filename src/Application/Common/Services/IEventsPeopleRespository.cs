using Domain.Entities.Events;

namespace Application.Common.Services;

public interface IEventsPeopleRespository : IRepository<EventPerson>
{
    Task<IEnumerable<EventPerson>> GetAllByOrderId(long orderId, CancellationToken ct);
    Task<IEnumerable<EventPerson>> GetAllByEventIdAsync(long eventId, CancellationToken ct);
    Task<IEnumerable<EventPerson>> GetAllByPersonAndCourse(long personId, long courseId, CancellationToken ct);
}