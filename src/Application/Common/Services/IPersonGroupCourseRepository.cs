using Domain.Entities.People;

namespace Application.Common.Services;

public interface IPersonGroupCourseRepository : IRepository<PersonGroupCourse>
{
    public IQueryable<PersonGroupCourse> GetPersonGroupCourseByCourseAsync(long courseId, CancellationToken ct);
    public Task<IEnumerable<PersonGroupCourse>> GetPersonGroupCoursesByPersonIdAsync(long personId, CancellationToken ct);

    public Task<IEnumerable<PersonGroupCourse>> GetCurrentCourseGroupByPeopleIdsAsync(IEnumerable<long> peopleIds, CancellationToken ct);
    public Task<PersonGroupCourse?> GetCoursePersonGroupByDocumentId(string documentId, long courseId, CancellationToken ct);
    public Task<PersonGroupCourse?> GetCoursePersonGroupById(long personId, long courseId, CancellationToken ct);
}