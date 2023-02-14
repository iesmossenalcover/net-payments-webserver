using Domain.Entities.People;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repos;

public class StudentsRepository : Repository<Student>, Application.Common.Services.IStudentsRepository
{
    public StudentsRepository(AppDbContext dbContext) : base(dbContext, dbContext.Students) {}

    public async Task<Student?> GetStudentByAcademicRecordAsync(long academicRecord, CancellationToken ct)
    {
        return await _dbSet
            .FirstOrDefaultAsync(x => x.AcademicRecordNumber == academicRecord, ct);
    }

    public async Task<IEnumerable<Student>> GetStudentsByAcademicRecordAsync(IEnumerable<long> academicRecords, CancellationToken ct)
    {
        return await _dbSet
            .Where(x => academicRecords.Distinct().Contains(x.AcademicRecordNumber))
            .ToListAsync(ct);
    }
}