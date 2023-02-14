using Application.Common.Services;
using Domain.Entities.People;
using MediatR;

namespace Application.People.Queries;

public record GroupVm(long Id, string Name);
public record CourseVm(long Id, string Name, bool SelectedCourse);
public record PersonVm(long Id, string DocumentId, string FirstName, string LastName, GroupVm Group, long? AcademicRecordNumber);
public record ListPeopleByCourseVm(IEnumerable<CourseVm> Courses, IEnumerable<PersonVm> People);
public record ListPeopleByCourseQuery(long? CourseId) : IRequest<ListPeopleByCourseVm>;

public class ListPeopleByCourseQuueryHandler : IRequestHandler<ListPeopleByCourseQuery, ListPeopleByCourseVm>
{
    private readonly ICoursesRepository _courseRepository;
    private readonly IPeopleRepository _peopleRepository;
    private readonly IPersonGroupCourseRepository _personGroupCourseRepository;

    public ListPeopleByCourseQuueryHandler(ICoursesRepository courseRepository, IPeopleRepository peopleRepository, IPersonGroupCourseRepository personGroupCourseRepository)
    {
        _courseRepository = courseRepository;
        _peopleRepository = peopleRepository;
        _personGroupCourseRepository = personGroupCourseRepository;
    }

    public async Task<ListPeopleByCourseVm> Handle(ListPeopleByCourseQuery request, CancellationToken ct)
    {
        IEnumerable<Course> courses = await _courseRepository.GetAllAsync(ct);
        Course course = request.CourseId.HasValue ? courses.First(x => x.Id == request.CourseId.Value) : courses.First(x => x.Active);

        IQueryable<PersonGroupCourse> personGroupCourses = _personGroupCourseRepository.GetPersonGroupCourseByCourseAsync(course.Id, ct);
        personGroupCourses = personGroupCourses
                    .OrderBy(x => x.Person.Surname1)
                    .Skip(0);
                    // .Take(10);

        var peopleVm = personGroupCourses.Select(x => ToPersonVm(x));
        var corusesVm = courses.Select(x => ToCourseVm(x, course.Id));
        return new ListPeopleByCourseVm(corusesVm, peopleVm);
    }

    public static PersonVm ToPersonVm(PersonGroupCourse pgc)
    {
        Person p = pgc.Person;
        Student? s = p as Student;
        long? academicRecordNumber = s != null ? s.AcademicRecordNumber : null;
        return new PersonVm(
            p.Id,
            p.DocumentId,
            p.Name,
            $"{p.Surname1} {p.Surname2}",
            new GroupVm(pgc.Group.Id, pgc.Group.Name),
            academicRecordNumber
        );
    }

    public static CourseVm ToCourseVm(Course c, long currentCourseId)
    {
        return new CourseVm(
            c.Id,
            c.Name,
            c.Id == currentCourseId
        );
    }
}
