using Application.Common;
using Domain.Services;
using Domain.Entities.People;
using MediatR;

namespace Application.People.Queries;

# region ViewModels
public record PersonVm
{
    public long id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Surname1 { get; set; } = string.Empty;
    public string? Surname2 { get; set; }
    public string DocumentId { get; set; } = string.Empty;
    public string? ContactPhone { get; set; }
    public string? Email { get; set; }
    public long? GroupId { get; set; }
    public long? AcademicRecordNumber { get; set; }
    public bool Amipa { get; set; }
    public bool Enrolled { get; set; } = false;
    public string? SubjectsInfo { get; set; }
    public string? SchoolAlert { get; set; }
    public bool WalkingAuthorization { get; set; }
    public bool TransportAuthorization { get; set; }
    public IEnumerable<PersonCourseVm> Courses { get; set; } = new List<PersonCourseVm>();
}

public record PersonCourseVm(
    long CourseId, string CourseName, bool Active, string GroupName, bool Amipa, bool Enrolled,
    bool WalkingAuthorization, DateTimeOffset? WalkingAuthorizationDate,
    bool TransportAuthorization, DateTimeOffset? TransportAuthorizationDate);

#endregion

#region Query
public record GetPersonByIdQuery(long Id) : IRequest<Response<PersonVm>>;
#endregion

public class GetPersonByIdQueryHandler : IRequestHandler<GetPersonByIdQuery, Response<PersonVm>>
{
    #region  IOC
    private readonly ICoursesRepository _courseRepository;
    private readonly IPeopleRepository _peopleRepository;
    private readonly IPersonGroupCourseRepository _personGroupCourseRepository;

    public GetPersonByIdQueryHandler(
        ICoursesRepository courseRepository,
        IPeopleRepository peopleRepository,
        IPersonGroupCourseRepository personGroupCourseRepository)
    {
        _courseRepository = courseRepository;
        _peopleRepository = peopleRepository;
        _personGroupCourseRepository = personGroupCourseRepository;
    }
    #endregion

    public async Task<Response<PersonVm>> Handle(GetPersonByIdQuery request, CancellationToken ct)
    {
        Person? person = await _peopleRepository.GetByIdAsync(request.Id, ct);
        if (person == null) return Response<PersonVm>.Error(ResponseCode.NotFound, "There is no person with this id");

        IEnumerable<PersonGroupCourse> personGroupCourses = await _personGroupCourseRepository.GetPersonGroupCoursesByPersonIdAsync(person.Id, ct);
        PersonGroupCourse? pgc = personGroupCourses.FirstOrDefault(x => x.Course.Active == true);

        PersonVm personVm = new PersonVm();

        personVm.AcademicRecordNumber = person.AcademicRecordNumber;
        personVm.id = person.Id;
        personVm.Name = person.Name;
        personVm.DocumentId = person.DocumentId;
        personVm.Surname1 = person.Surname1;
        personVm.Surname2 = person.Surname2;
        personVm.Email = person.ContactMail;
        personVm.ContactPhone = person.ContactPhone;
        personVm.SchoolAlert = person.SchoolAlert;
        personVm.GroupId = pgc?.GroupId;
        
        // PGC can be null for a person not in the current course.
        personVm.SubjectsInfo = pgc?.SubjectsInfo ?? "";
        personVm.Enrolled = pgc?.Enrolled ?? false;
        personVm.Amipa = pgc?.Amipa ?? false;
        personVm.WalkingAuthorization = pgc?.WalkingAuthorization ?? false;
        personVm.TransportAuthorization = pgc?.TransportAuthorization ?? false;

        personVm.Courses = personGroupCourses
            .OrderByDescending(x => x.Course.StartDate)
            .Select(x => new PersonCourseVm(
                x.CourseId, x.Course.Name, x.Course.Active, x.Group.Name, x.Amipa, x.Enrolled,
                x.WalkingAuthorization, x.WalkingAuthorizationDate,
                x.TransportAuthorization, x.TransportAuthorizationDate))
            .ToList();

        return Response<PersonVm>.Ok(personVm);
    }
}
