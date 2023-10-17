using System.Collections;
using Domain.Entities.Events;

namespace Domain.Services;

public interface IEventPersonOrderRepository : IRepository<EventPersonOrder>
{
    Task<IEnumerable<EventPersonOrder>> GetAllByOrderIdAsync(long orderId, CancellationToken ct);
    Task<IEnumerable<EventPersonOrder>> GetAllByPersonEventIdAsync(long personEventId, CancellationToken ct);
}