using Application.Common;
using Domain.Services;
using Domain.Entities.People;
using MediatR;

namespace Application.People.Queries;

# region ViewModels

public record PersonVm
{
    public long Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Surname1 { get; set; } = string.Empty;
    public string? Surname2 { get; set; }
    public string DocumentId { get; set; } = string.Empty;
    public string? ContactPhone { get; set; }
    public string? Email { get; set; }
    public long? GroupId { get; set; }
    public long? AcademicRecordNumber { get; set; }
    public bool Amipa { get; set; }
    public bool Enrolled { get; set; }
    public string? SubjectsInfo { get; set; }
}

#endregion

#region Query

public record GetPersonByIdQuery(long Id) : IRequest<Response<PersonVm>>;

#endregion

public class GetPersonByIdQueryHandler : IRequestHandler<GetPersonByIdQuery, Response<PersonVm>>
{
    #region IOC

    private readonly IPeopleRepository _peopleRepository;
    private readonly IPersonGroupCourseRepository _personGroupCourseRepository;

    public GetPersonByIdQueryHandler(IPeopleRepository peopleRepository,
        IPersonGroupCourseRepository personGroupCourseRepository)
    {
        _peopleRepository = peopleRepository;
        _personGroupCourseRepository = personGroupCourseRepository;
    }

    #endregion

    public async Task<Response<PersonVm>> Handle(GetPersonByIdQuery request, CancellationToken ct)
    {
        Person? person = await _peopleRepository.GetByIdAsync(request.Id, true, ct);
        if (person == null) return Response<PersonVm>.Error(ResponseCode.NotFound, "There is no person with this id");

        IEnumerable<PersonGroupCourse> personGroupCourses =
            await _personGroupCourseRepository.GetPersonGroupCoursesByPersonIdAsync(person.Id, ct);
        
        PersonGroupCourse? pgc = personGroupCourses.FirstOrDefault(x => x.Course.Active);

        var personVm = new PersonVm()
        {
            AcademicRecordNumber = person.AcademicRecordNumber,
            Id = person.Id,
            Name = person.Name,
            DocumentId = person.DocumentId,
            Surname1 = person.Surname1,
            Surname2 = person.Surname2,
            Email = person.ContactMail,
            ContactPhone = person.ContactPhone,
            GroupId = pgc?.GroupId,

            // PGC can be null for a person not in the current course.
            SubjectsInfo = pgc?.SubjectsInfo ?? "",
            Enrolled = pgc?.Enrolled ?? false,
            Amipa = pgc?.Amipa ?? false,
        };


        return Response<PersonVm>.Ok(personVm);
    }
}