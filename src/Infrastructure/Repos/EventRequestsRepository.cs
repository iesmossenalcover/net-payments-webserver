using Domain.Entities.EventRequests;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repos;

public class EventRequestsRepository : Repository<EventRequest>, Domain.Services.IEventRequestsRepository
{
    public EventRequestsRepository(AppDbContext dbContext) : base(dbContext, dbContext.EventRequests)
    {
    }

    public async Task<EventRequest?> GetWithDetailsAsync(long id, CancellationToken ct)
    {
        return await _dbSet
            .Where(x => x.Id == id && x.Status != EventRequestStatus.Deleted)
            .Include(x => x.Requester)
            .Include(x => x.Event)
            .Include(x => x.Recipients)
            .Include(x => x.Transitions.OrderBy(t => t.Date)).ThenInclude(t => t.Actor)
            .AsSplitQuery()
            .FirstOrDefaultAsync(ct);
    }

    public async Task<IEnumerable<EventRequest>> GetAllByRequesterAsync(long userId, CancellationToken ct)
    {
        return await _dbSet
            .Where(x => x.RequesterUserId == userId && x.Status != EventRequestStatus.Deleted)
            .Include(x => x.Requester)
            .OrderByDescending(x => x.UpdatedAt)
            .ToListAsync(ct);
    }

    public async Task<IEnumerable<EventRequest>> GetAllAsync(EventRequestStatus? status, CancellationToken ct)
    {
        return await _dbSet
            .Where(x => x.Status != EventRequestStatus.Deleted && (!status.HasValue || x.Status == status.Value))
            .Include(x => x.Requester)
            .OrderByDescending(x => x.UpdatedAt)
            .ToListAsync(ct);
    }

    public async Task<EventRequest?> GetByEventIdAsync(long eventId, CancellationToken ct)
    {
        return await _dbSet
            .Where(x => x.EventId == eventId)
            .FirstOrDefaultAsync(ct);
    }

    public async Task<IEnumerable<EventRequest>> GetCalendarOverlappingAsync(DateTimeOffset from, DateTimeOffset to, CancellationToken ct)
    {
        DateTimeOffset fromUtc = from.ToUniversalTime();
        DateTimeOffset toUtc = to.ToUniversalTime();

        // Totes menys les eliminades i les creades (d'aquestes ja hi surt el seu esdeveniment).
        return await _dbSet
            .Where(x => x.Status != EventRequestStatus.Deleted && x.Status != EventRequestStatus.Created)
            .Where(x => x.StartDate <= toUtc && (x.EndDate ?? x.StartDate) >= fromUtc)
            .OrderBy(x => x.StartDate)
            .ToListAsync(ct);
    }

    public async Task<bool> SaveAsync(CancellationToken ct)
    {
        try
        {
            await _dbContext.SaveChangesAsync(ct);
            return true;
        }
        catch (DbUpdateConcurrencyException)
        {
            return false;
        }
    }
}
