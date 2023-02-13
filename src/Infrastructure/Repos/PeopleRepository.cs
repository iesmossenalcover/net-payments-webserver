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

    // Maybe create PersonGroupCourse repo.
    public async Task<IEnumerable<PersonGroupCourse>> GetCurrentCourseGroupByPeopleIdsAsync(IEnumerable<long> peopleIds, CancellationToken ct)
    {
        return await _dbContext.PersonGroupCourses
                    .Include(x => x.Person)
                    .Include(x => x.Group)
                    .Include(x => x.Course)
                    .Where(x => x.Course.Active == true && peopleIds.Distinct().Contains(x.PersonId)).ToListAsync();
    }
}