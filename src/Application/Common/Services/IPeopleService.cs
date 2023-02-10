using Application.Common.Models;
using Domain.Entities.People;

namespace Application.Common.Services;

public interface IPeopleService
{
    public Task<IEnumerable<Group>> GetGroupsByNameAsync(IEnumerable<string> names, CancellationToken ct);

    // Students
    public Task InsertStudentAsync(Student student);
    public Task<Student?> GetStudentByAcademicRecordAsync(long academicRecordNumber, CancellationToken ct);
    public Task<IEnumerable<Student>> GetStudentsByAcademicRecordAsync(IEnumerable<long> academicRecordNumbers, CancellationToken ct);

    // People
    public Task<IEnumerable<Person>> GetPeopleAsync(IEnumerable<string> documents, CancellationToken ct);

    public Task<Person?> GetPersonByDocumentIdAsync(string documentId, CancellationToken ct);

    public Task InsertPersonAsync(Person person);



    // Courses
    public Task<Course> GetCurrentCoursAsync(CancellationToken ct);

    // Person Group Course
    public Task<IEnumerable<PersonGroupCourse>> GetCurrentCoursePersonGroupByPeopleIdsAsync(IEnumerable<long> peopleIds, CancellationToken ct);

    // Transactions
    public Task InsertAndUpdateTransactionAsync(BatchUploadModel batchUploadModel, CancellationToken ct);
}