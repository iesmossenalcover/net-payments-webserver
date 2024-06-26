using Domain.Entities.Events;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repos;

public class EventsPeopleRepository : Repository<EventPerson>, Domain.Services.IEventsPeopleRespository
{
    public EventsPeopleRepository(AppDbContext dbContext) : base(dbContext, dbContext.EventPersons)
    {
    }

    public async Task<IEnumerable<EventPerson>> GetWithRelationsByIdsAsync(IEnumerable<long> ids, CancellationToken ct)
    {
        return await _dbSet
            .Where(x => ids.Contains(x.Id))
            .Include(x => x.Person)
            .Include(x => x.Event)
            .Include(x => x.PaidOrder)
            .ToListAsync(ct);
    }

    public async Task<IEnumerable<EventPerson>> GetAllByCourseId(long courseId, CancellationToken ct)
    {
        return await _dbSet
            .Where(x => x.Event.CourseId == courseId)
            .Include(x => x.Person)
            .Include(x => x.Event)
            .Include(x => x.PaidOrder)
            .ToListAsync(ct);
    }

    public async Task<IEnumerable<EventPerson>> GetAllByEventIdAsync(long eventId, CancellationToken ct)
    {
        return await _dbSet.Where(x => x.EventId == eventId).ToListAsync(ct);
    }

    public async Task<IEnumerable<EventPerson>> GetAllByOrderId(long orderId, CancellationToken ct)
    {
        return await _dbSet
            .Where(x => x.PaidOrderId == orderId)
            .Include(x => x.Person)
            .Include(x => x.Event)
            .Include(x => x.PaidOrder)
            .ToListAsync(ct);
    }

    public async Task<IEnumerable<EventPerson>> GetAllByPersonAndCourse(long personId, long courseId,
        CancellationToken ct)
    {
        return await _dbSet
            .Where(x => x.PersonId == personId && x.Event.CourseId == courseId)
            .Include(x => x.Person)
            .Include(x => x.Event)
            .Include(x => x.PaidOrder)
            .ToListAsync(ct);
    }

    public async Task<IEnumerable<EventPerson>> GetAllByPersonId(long personId, bool onlyPaid, CancellationToken ct)
    {
        var query = _dbSet.Where(x => x.PersonId == personId);

        if (onlyPaid)
        {
            query = query.Where(x => x.PersonId == personId);
        }

        return await query
            .Include(x => x.Person)
            .Include(x => x.Event).ThenInclude(x => x.Course)
            .Include(x => x.PaidOrder)
            .ToListAsync(ct);
    }

    public async Task<EventPerson?> GetWithRelationsByIdAsync(long id, CancellationToken ct)
    {
        return await _dbSet
            .Include(x => x.Person)
            .Include(x => x.Event)
            .Include(x => x.PaidOrder)
            .FirstAsync(x => x.Id == id, ct);
    }
}