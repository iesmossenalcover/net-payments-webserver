using Domain.Entities.Events;

namespace Application.Common.Services;

public interface IEventsPeopleRespository : IRepository<EventPerson>
{
    Task<EventPerson?> GetWithRelationsByIdAsync(long id, CancellationToken ct);
    Task<IEnumerable<EventPerson>> GetAllByCourseId(long courseId, CancellationToken ct);
    Task<IEnumerable<EventPerson>> GetAllByOrderId(long orderId, CancellationToken ct);
    Task<IEnumerable<EventPerson>> GetAllByEventIdAsync(long eventId, CancellationToken ct);
    Task<IEnumerable<EventPerson>> GetAllByPersonAndCourse(long personId, long courseId, CancellationToken ct);
}