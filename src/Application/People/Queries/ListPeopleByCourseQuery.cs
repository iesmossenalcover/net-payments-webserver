using Application.Common.Services;
using Domain.Entities.People;
using MediatR;

namespace Application.People.Queries;

# region ViewModels
public record CourseVm(long Id, string Name);
public record PersonRowVm(long Id, string DocumentId, string FirstName, string LastName, long GroupId, string GroupName, bool Amipa, long? AcademicRecordNumber);
public record ListPeopleByCourseVm(IEnumerable<PersonRowVm> People);
#endregion

public record ListPeopleByCourseQuery(long? CourseId) : IRequest<IEnumerable<PersonRowVm>>;

public class ListPeopleByCourseQuueryHandler : IRequestHandler<ListPeopleByCourseQuery, IEnumerable<PersonRowVm>>
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

    public async Task<IEnumerable<PersonRowVm>> Handle(ListPeopleByCourseQuery request, CancellationToken ct)
    {
        IEnumerable<Course> courses = await _courseRepository.GetAllAsync(ct);
        Course course = request.CourseId.HasValue ? courses.First(x => x.Id == request.CourseId.Value) : courses.First(x => x.Active);

        IQueryable<PersonGroupCourse> personGroupCourses = _personGroupCourseRepository.GetPersonGroupCourseByCourseAsync(course.Id, ct);
        personGroupCourses = personGroupCourses
                    .OrderBy(x => x.Person.Surname1)
                    .Skip(0);
                    // .Take(10);

        IEnumerable<PersonGroupCourse> respone = personGroupCourses.ToList();
        return respone
                .Select(x => ToPersonVm(x))
                .OrderBy(x => x.GroupName)
                .ThenBy(x => x.FirstName)
                .ThenBy(x => x.LastName);
    }

    public static PersonRowVm ToPersonVm(PersonGroupCourse pgc)
    {
        Person p = pgc.Person;
        return new PersonRowVm(
            p.Id,
            p.DocumentId,
            p.Name,
            $"{p.Surname1} {p.Surname2}",
            pgc.Group.Id,
            pgc.Group.Name,
            pgc.Amipa,
            p.AcademicRecordNumber
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
