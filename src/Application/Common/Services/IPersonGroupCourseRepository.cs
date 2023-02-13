using Domain.Entities.People;

namespace Application.Common.Services;

public interface IPersonGroupCourseRepository : IRepository<PersonGroupCourse>
{
    public IQueryable<PersonGroupCourse> GetPersonGroupCourseByCourseAsync(long courseId, CancellationToken ct);

    public Task<IEnumerable<PersonGroupCourse>> GetCurrentCourseGroupByPeopleIdsAsync(IEnumerable<long> peopleIds, CancellationToken ct);
}