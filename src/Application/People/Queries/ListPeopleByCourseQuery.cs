using Application.Common.Services;
using Domain.Entities.People;
using MediatR;

namespace Application.People.Queries;

# region ViewModels
public record CourseVm(long Id, string Name);
public record PersonSummaryVm(long Id, string DocumentId, string FirstName, string LastName, long GroupId, string GroupName, long? AcademicRecordNumber);
public record ListPeopleByCourseVm(IEnumerable<PersonSummaryVm> People);
#endregion

public record ListPeopleByCourseQuery(long? CourseId) : IRequest<IEnumerable<PersonSummaryVm>>;

public class ListPeopleByCourseQuueryHandler : IRequestHandler<ListPeopleByCourseQuery, IEnumerable<PersonSummaryVm>>
{
    # region IOC
    private readonly ICoursesRepository _courseRepository;
    private readonly IPersonGroupCourseRepository _personGroupCourseRepository;

    public ListPeopleByCourseQuueryHandler(ICoursesRepository courseRepository, IPersonGroupCourseRepository personGroupCourseRepository)
    {
        _courseRepository = courseRepository;
        _personGroupCourseRepository = personGroupCourseRepository;
    }

    #endregion

    public async Task<IEnumerable<PersonSummaryVm>> Handle(ListPeopleByCourseQuery request, CancellationToken ct)
    {
        IEnumerable<Course> courses = await _courseRepository.GetAllAsync(ct);
        Course course = request.CourseId.HasValue ? courses.First(x => x.Id == request.CourseId.Value) : courses.First(x => x.Active);

        IQueryable<PersonGroupCourse> personGroupCourses = _personGroupCourseRepository.GetPersonGroupCourseByCourseAsync(course.Id, ct);
        personGroupCourses = personGroupCourses
                    .OrderBy(x => x.Person.Surname1)
                    .Skip(0);
                    // .Take(10);

        return personGroupCourses.Select(x => ToPersonVm(x));
    }

    public static PersonSummaryVm ToPersonVm(PersonGroupCourse pgc)
    {
        Person p = pgc.Person;
        Student? s = p as Student;
        long? academicRecordNumber = s != null ? s.AcademicRecordNumber : null;
        return new PersonSummaryVm(
            p.Id,
            p.DocumentId,
            p.Name,
            $"{p.Surname1} {p.Surname2}",
            pgc.Group.Id,
            pgc.Group.Name,
            academicRecordNumber
        );
    }

    private static CourseVm ToCourseVm(Course c)
    {
        return new CourseVm(
            c.Id,
            c.Name
        );
    }
}
