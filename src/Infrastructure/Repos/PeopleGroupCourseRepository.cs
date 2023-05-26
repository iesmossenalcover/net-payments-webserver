using Application.Common.Services;
using Domain.Entities.People;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repos;

public class PeopleGroupCourseRepository : Repository<PersonGroupCourse>, Application.Common.Services.IPersonGroupCourseRepository
{
    public PeopleGroupCourseRepository(AppDbContext dbContext) : base(dbContext, dbContext.PersonGroupCourses) {}

    public async Task<IEnumerable<PersonGroupCourse>> FilterPeople(FilterPeople filter, CancellationToken ct)
    {
        return await _dbSet
                .Include(x => x.Person)
                .Include(x => x.Group)
                .Include(x => x.Course)
                .Where(x => 
                    (
                        EF.Functions.ILike(EF.Functions.Unaccent(x.Person.Surname1), $"%{filter.Query}%") ||
                        EF.Functions.ILike(EF.Functions.Unaccent(x.Person.Name), $"%{filter.Query}%") ||
                        (x.Person.Surname2 != null && EF.Functions.ILike(EF.Functions.Unaccent(x.Person.Surname2), $"%{filter.Query}%")) ||
                        (x.Person.AcademicRecordNumber.HasValue && EF.Functions.ILike(EF.Functions.Unaccent(x.Person.AcademicRecordNumber.Value.ToString()), $"%{filter.Query}%")) ||
                        EF.Functions.ILike(EF.Functions.Unaccent(x.Group.Name), $"%{filter.Query}%")
                    ) && x.Course.Active == true
                )
                .OrderBy(x => x.Course.Name)
                .ThenBy(x => x.Person.Surname1)
                .ThenBy(x => x.Person.Surname2)
                .ThenBy(x => x.Person.Name)
                .ToListAsync(ct);
    }

    public async Task<PersonGroupCourse?> GetCoursePersonGroupByDocumentId(string documentId, long courseId, CancellationToken ct)
    {
        return await _dbSet
                .Include(x => x.Person)
                .Include(x => x.Group)
                .Include(x => x.Course)
                .FirstOrDefaultAsync(x => x.Person.DocumentId == documentId.ToUpperInvariant() && x.CourseId== courseId, ct);
    }

    public async Task<PersonGroupCourse?> GetCoursePersonGroupById(long personId, long courseId, CancellationToken ct)
    {
        return await _dbSet
                .Include(x => x.Person)
                .Include(x => x.Group)
                .Include(x => x.Course)
                .FirstOrDefaultAsync(x => x.PersonId == personId && x.CourseId== courseId, ct);
    }

    public async Task<IEnumerable<PersonGroupCourse>> GetCurrentCourseGroupByPeopleIdsAsync(IEnumerable<long> peopleIds, CancellationToken ct)
    {
        return await _dbContext.PersonGroupCourses
                    .Include(x => x.Person)
                    .Include(x => x.Group)
                    .Include(x => x.Course)
                    .Where(x => x.Course.Active == true && peopleIds.Distinct().Contains(x.PersonId)).ToListAsync(ct);
    }

    public IQueryable<PersonGroupCourse> GetPersonGroupCourseByCourseAsync(long courseId, CancellationToken ct)
    {
        return _dbContext.PersonGroupCourses
                    .Where(x => x.CourseId == courseId)
                    .Include(x => x.Person)
                    .Include(x => x.Group)
                    .Include(x => x.Course);
    }

    public async Task<IEnumerable<PersonGroupCourse>> GetPersonGroupCoursesByPersonIdAsync(long personId, CancellationToken ct)
    {
        return await _dbSet
                    .Where(x => x.PersonId == personId)
                    .Include(x => x.Person)
                    .Include(x => x.Group)
                    .Include(x => x.Course).ToListAsync(ct);
    }
}