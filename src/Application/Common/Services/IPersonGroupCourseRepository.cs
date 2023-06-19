using Domain.Entities.People;

namespace Application.Common.Services;

public interface IPersonGroupCourseRepository : IRepository<PersonGroupCourse>
{
    public IQueryable<PersonGroupCourse> GetPersonGroupCourseByCourseAsync(long courseId, CancellationToken ct);
    public Task<IEnumerable<PersonGroupCourse>> GetPersonGroupCoursesByPersonIdAsync(long personId, CancellationToken ct);

    public Task<IEnumerable<PersonGroupCourse>> GetCurrentCourseGroupByPeopleIdsAsync(IEnumerable<long> peopleIds, CancellationToken ct);
    public Task<IEnumerable<PersonGroupCourse>> GetPeopleGroupByPeopleIdsAndCourseIdAsync(long courseId, IEnumerable<long> peopleIds, CancellationToken ct);
    public Task<PersonGroupCourse?> GetCoursePersonGroupByDocumentId(string documentId, long courseId, CancellationToken ct);
    public Task<PersonGroupCourse?> GetCoursePersonGroupById(long personId, long courseId, CancellationToken ct);
    public Task<IEnumerable<PersonGroupCourse>> FilterPeople(FilterPeople filter, int maxResults, CancellationToken ct);
    public Task<IEnumerable<PersonGroupCourse>> GetPeopleGroupByGroupIdAndCourseIdAsync(long courseId, long groupId, CancellationToken ct);

}

public record FilterPeople(string Query);