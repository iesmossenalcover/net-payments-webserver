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
    public string Surname1 { get; set; } = string.Empty;
    public string? Surname2 { get; set; }
    public string DocumentId { get; set; } = string.Empty;
    public string? ContactPhone { get; set; }
    public string? ContactMail { get; set; }
    public long GroupId { get; set; }
}

public record StudentVm : PersonVm
{
    public long AcademicRecordNumber { get; set; }
    public bool PreEnrollment { get; set; }
    public bool Amipa { get; set; }
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

        IEnumerable<PersonGroupCourse> pgc = await _personGroupCourseRepository.GetPersonGroupCoursesByPersonIdAsync(person.Id, ct);
        PersonGroupCourse group = pgc.First(x => x.Course.Active == true);

        PersonVm personVm;
        if (person is Student)
        {
            var student = (Student)person;
            StudentVm studentVm = new StudentVm()
            {
                AcademicRecordNumber = student.AcademicRecordNumber,
                Amipa = group.Amipa,
                PreEnrollment = student.PreEnrollment,
                SubjectsInfo = student.SubjectsInfo
            };
            personVm = studentVm;
        }
        else
        {
            personVm = new PersonVm();
        }

        personVm.id = person.Id;
        personVm.Name = person.Name;
        personVm.DocumentId = person.DocumentId;
        personVm.Surname1 = person.Surname1;
        personVm.Surname2 = person.Surname2;
        personVm.ContactMail = person.ContactMail;
        personVm.ContactPhone = person.ContactPhone;
        personVm.GroupId = group.Id;

        return Response<PersonVm>.Ok(personVm);
    }
}
