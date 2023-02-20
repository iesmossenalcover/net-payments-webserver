using Domain.Entities.Events;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repos;

public class EventsPeopleRepository : Repository<EventPerson>, Application.Common.Services.IEventsPeopleRespository
{
    public EventsPeopleRepository(AppDbContext dbContext) : base(dbContext, dbContext.EventPersons) {}

    public async Task<IEnumerable<EventPerson>> GetAllByEventIdAsync(long eventId, CancellationToken ct)
    {
        return await _dbSet.Where(x => x.EventId == eventId).ToListAsync();
    }
}