using Domain.Entities.People;

namespace Domain.Services;

public interface ICoursesRepository : IRepository<Course>
{
    public Task<Course> GetCurrentCoursAsync(CancellationToken ct);
    public Task<Course?> GetCourseByNameAsync(string name, CancellationToken ct);
}