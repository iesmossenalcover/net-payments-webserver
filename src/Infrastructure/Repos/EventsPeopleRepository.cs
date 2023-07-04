using Domain.Entities.Events;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repos;

public class EventsRepository : Repository<Event>, Domain.Services.IEventsRespository
{
    public EventsRepository(AppDbContext dbContext) : base(dbContext, dbContext.Events) {}

    public async Task<IEnumerable<Event>> GetAllEventsByCourseIdAsync(long courseId, CancellationToken ct)
    {
        return await _dbSet
                        .Where(x => x.CourseId == courseId)
                        .OrderByDescending(x => x.Date)
                        .ToListAsync(ct);
    }

    public async Task<Event?> GetEventByCodeAsync(string code, CancellationToken ct)
    {
        return await _dbSet.FirstOrDefaultAsync(x => x.Code == code, ct);
    }
}