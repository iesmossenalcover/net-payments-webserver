using Domain.Entities.People;

namespace Application.Common.Services;

public interface IStudentsRepository : IRepository<Student>
{
    public Task<Student?> GetStudentByAcademicRecordAsync(long academicRecord, CancellationToken ct);
    public Task<IEnumerable<Student>> GetStudentsByAcademicRecordAsync(IEnumerable<long> academicRecords, CancellationToken ct);

    public Task AddStudentsExistingPersonAsync(Student s, Person p, CancellationToken ct);

}