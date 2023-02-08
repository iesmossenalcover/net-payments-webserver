using Domain.Entities.People;

namespace Application.Common.Services;

public interface IPeopleService
{
    public Task<IEnumerable<Group>> GetGroupsByNameAsync(IEnumerable<string> groupCodes, CancellationToken ct);

    // Students
    public Task<Student> InsertStudentAsync(Student student);
    public Task<IEnumerable<Student>> GetManyStudentsAsync(IEnumerable<long> expidients, bool loadPeople, CancellationToken ct);

    // People
    public Task<IEnumerable<Person>> GetPeopleAsync(IEnumerable<string> documents, CancellationToken ct);

    // Courses
    public Task<Course> GetCurrentCoursAsync(CancellationToken ct);

    // Person Group Course
    public Task<IEnumerable<PersonGroupCourse>> GetCurrentCoursePersonGroupByPeopleIdsAsync(IEnumerable<long> peopleIds, CancellationToken ct);

    // Transactions
    public Task InsertAndUpdateTransactionAsync(IEnumerable<Group> groups, IEnumerable<Person> persons, IEnumerable<Student> students, IEnumerable<PersonGroupCourse> personGroupCourses, CancellationToken ct);
}