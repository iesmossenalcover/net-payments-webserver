using Domain.Entities.Events;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repos;

public class EventPersonOrderRepository : Repository<EventPersonOrder>, Domain.Services.IEventPersonOrderRepository
{
    public EventPersonOrderRepository(AppDbContext dbContext) : base(dbContext, dbContext.EventPersonOrders)
    {
    }

    public async Task<IEnumerable<EventPersonOrder>> GetAllByOrderIdAsync(long orderId, CancellationToken ct)
    {
        return await _dbSet.Where(x => x.OrderId == orderId).ToListAsync(ct);
    }

    public async Task<IEnumerable<EventPersonOrder>> GetAllByPersonEventIdAsync(long personEventId,
        CancellationToken ct)
    {
        return await _dbSet.Where(x => x.EventPersonId == personEventId).ToListAsync(ct);
    }
}