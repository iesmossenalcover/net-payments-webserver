using Domain.Entities.People;

namespace Application.Common.Services;

public interface ICoursesRepository : IRepository<Course>
{
    public Task<Course> GetCurrentCoursAsync(CancellationToken ct);
}