using Application.Common;
using Application.Common.Services;
using Domain.Entities.People;
using MediatR;

namespace Application.People.Queries;

# region ViewModels
public record PersonVm
{
    public long id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string DocumentId { get; set; } = string.Empty;
    public string? ContactPhone { get; set; }
    public string? ContactMail { get; set; }
    public long? GroupId { get; set; }
    public long? AcademicRecordNumber { get; set; }
    public bool Amipa { get; set; }
    public bool Enrolled { get; set; } = false;
    public string? SubjectsInfo { get; set; }
}

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
        personVm.LastName = person.LastName;
        personVm.ContactMail = person.ContactMail;
        personVm.ContactPhone = person.ContactPhone;
        personVm.GroupId = pgc?.GroupId;
        // PGC can be null for a person not in the current course.
        personVm.SubjectsInfo = pgc?.SubjectsInfo ?? "";
        personVm.Enrolled = pgc?.Enrolled ?? false;
        personVm.Amipa = pgc?.Amipa ?? false;

        return Response<PersonVm>.Ok(personVm);
    }
}
