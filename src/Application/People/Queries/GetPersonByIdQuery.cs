using Application.Common;
using Application.Common.Services;
using Application.People.Common.ViewModels;
using Domain.Entities.People;
using MediatR;

namespace Application.People.Queries;

public record GetPersonByIdQuery(long Id) : IRequest<Response<PersonVm>>;

public class GetPersonByIdQueryHandler : IRequestHandler<GetPersonByIdQuery, Response<PersonVm>>
{
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

    public async Task<Response<PersonVm>> Handle(GetPersonByIdQuery request, CancellationToken ct)
    {
        Person? person = await _peopleRepository.GetByIdAsync(request.Id, ct);
        if (person == null) return Response<PersonVm>.Error(ResponseCode.NotFound, "There is no person with this id");

        IEnumerable<PersonGroupCourse> pgc = await _personGroupCourseRepository.GetPersonGroupCoursesByPersonIdAsync(person.Id, ct);
        PersonGroupCourse group = pgc.First(x => x.Course.Active == true);

        Common.ViewModels.PersonVm personVm;
        if (person is Student)
        {
            var student = (Student)person;
            StudentVm studentVm = new Common.ViewModels.StudentVm()
            {
                AcademicRecordNumber = student.AcademicRecordNumber,
                Amipa = student.Amipa,
                PreEnrollment = student.PreEnrollment,
                SubjectsInfo = student.SubjectsInfo
            };
            personVm = studentVm;
        }
        else
        {
            personVm = new Common.ViewModels.PersonVm();
        }

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
