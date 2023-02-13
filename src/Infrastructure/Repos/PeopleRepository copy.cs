using Domain.Entities.People;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repos;

public class PeopleRepository : Repository<Person>, Application.Common.Services.IPeopleRepository
{
    public PeopleRepository(AppDbContext dbContext) : base(dbContext, dbContext.People) {}

    public async Task<IEnumerable<Person>> GetPeopleByDocumentIdsAsync(IEnumerable<string> documentIds, CancellationToken ct)
    {
        return await _dbSet.Where(x => documentIds.Distinct().Contains(x.DocumentId)).ToListAsync(ct);
    }

    public async Task<Person?> GetPersonByDocumentIdAsync(string documentId, CancellationToken ct)
    {
        return await _dbSet.FirstOrDefaultAsync(x => x.DocumentId == documentId, ct);
    }

    public async Task<IEnumerable<Person>> GetPeopleByCourseAsync(long courseId, int position, int? max, CancellationToken ct)
    {
        bool includeGroupCourses = true;

        var query = _dbContext
                    .PersonGroupCourses
                    .Where(x => x.CourseId == courseId);
                    

        if (includeGroupCourses)
        {
            query = query.Include(x => x.Person).ThenInclude(x => x.GroupCourses);
        }
        else
        {
            query = query.Include(x => x.Person);
        }

        var q = query.Select(x => x.Person)
                    .OrderBy(x => x.Surname1)
                    .Skip(position);

        if (max.HasValue)
        {
            query = query.Take(max.Value);
        }
        
        return await q.ToListAsync();
    }
}