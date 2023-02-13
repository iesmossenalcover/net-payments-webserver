using Domain.Entities.People;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repos;

public class PeopleGroupCourseRepository : Repository<PersonGroupCourse>, Application.Common.Services.IPersonGroupCourseRepository
{
    public PeopleGroupCourseRepository(AppDbContext dbContext) : base(dbContext, dbContext.PersonGroupCourses) {}

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
                    .Include(x => x.Person)
                    .Include(x => x.Group)
                    .Include(x => x.Course)
                    .Where(x => x.CourseId == courseId);
    }
}