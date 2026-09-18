using Domain.Entities.Events;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repos;

public class EventsRepository : Repository<Event>, Domain.Services.IEventsRespository
{
    public EventsRepository(AppDbContext dbContext) : base(dbContext, dbContext.Events)
    {
    }

    public async Task<IEnumerable<Event>> GetAllEventsByCourseIdAsync(long courseId, CancellationToken ct)
    {
        return await _dbSet
            .Where(x => x.CourseId == courseId)
            .OrderByDescending(x => x.Date)
            .ToListAsync(ct);
    }

    public async Task<IEnumerable<Event>> GetAllUnexpiredEventsByCourseIdAsync(long courseId, CancellationToken ct)
    {
        return await _dbSet
            .Where(x =>
                x.CourseId == courseId &&
                (
                    !x.UnpublishDate.HasValue ||
                    (
                        x.UnpublishDate.HasValue && x.UnpublishDate.Value > DateTimeOffset.UtcNow
                    )
                )
            )
            .OrderByDescending(x => x.Date)
            .ToListAsync(ct);
    }

    // [from, to) sobre la data d'inici de l'event.
    public async Task<IEnumerable<Event>> GetEventsStartingBetweenAsync(DateTimeOffset from, DateTimeOffset to, CancellationToken ct)
    {
        // Npgsql només accepta offset 0 als paràmetres 'timestamp with time zone': els límits
        // poden arribar amb l'offset local (+01:00/+02:00) i s'han de passar a UTC. És el mateix
        // instant, així que el filtre no canvia.
        DateTimeOffset fromUtc = from.ToUniversalTime();
        DateTimeOffset toUtc = to.ToUniversalTime();

        return await _dbSet
            .Where(x => x.Date >= fromUtc && x.Date < toUtc)
            .OrderBy(x => x.Date)
            .ToListAsync(ct);
    }

    public async Task<Event?> GetEventByCodeAsync(string code, CancellationToken ct)
    {
        return await _dbSet.FirstOrDefaultAsync(x => x.Code == code, ct);
    }
}