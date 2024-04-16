using Domain.Entities.People;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repos;

public class CoursesRepository : Repository<Course>, Domain.Services.ICoursesRepository
{
    public CoursesRepository(AppDbContext dbContext) : base(dbContext, dbContext.Courses) {}

    public async Task<Course?> GetCourseByNameAsync(string name, CancellationToken ct)
    {
        return await _dbSet.FirstOrDefaultAsync(x => x.Name == name);
    }

    public async Task<Course> GetCurrentCoursAsync(CancellationToken ct)
    {
        return await _dbSet.FirstAsync(x => x.Active == true);
    }
}