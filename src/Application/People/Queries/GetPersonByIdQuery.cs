using Application.Common.Services;
using Application.People.Common.ViewModels;
using Domain.Entities.People;
using MediatR;

namespace Application.People.Queries;

public record GetPersonGroupCoursesVm(IEnumerable<PersonGroupCourseVm> PersonGroupCourses, IEnumerable<GroupVm> Groups);
public record GetPersonByIdVm(GetPersonGroupCoursesVm PersonGroups, Common.ViewModels.PersonVm Person, Common.ViewModels.StudentVm? Student);

public record GetPersonByIdQuery(long Id) : IRequest<GetPersonByIdVm>;

public class GetPersonByIdQueryHandler : IRequestHandler<GetPersonByIdQuery, GetPersonByIdVm>
{
    private readonly ICoursesRepository _courseRepository;
    private readonly IPeopleRepository _peopleRepository;
    private readonly IGroupsRepository _groupsRepository;
    private readonly IPersonGroupCourseRepository _personGroupCourseRepository;

    public GetPersonByIdQueryHandler(
        ICoursesRepository courseRepository,
        IPeopleRepository peopleRepository,
        IPersonGroupCourseRepository personGroupCourseRepository,
        IGroupsRepository groupsRepository)
    {
        _courseRepository = courseRepository;
        _peopleRepository = peopleRepository;
        _groupsRepository = groupsRepository;
        _personGroupCourseRepository = personGroupCourseRepository;
    }

    public async Task<GetPersonByIdVm> Handle(GetPersonByIdQuery request, CancellationToken ct)
    {
        Person? person = await _peopleRepository.GetByIdAsync(request.Id, ct);
        if (person == null) throw new Exception("Bad request");
        
        IEnumerable<Group> groups = await _groupsRepository.GetAllAsync(ct);
        IEnumerable<PersonGroupCourse> pgc = await _personGroupCourseRepository.GetPersonGroupCoursesByPersonIdAsync(person.Id, ct);

        IEnumerable<PersonGroupCourseVm> pgcVm = pgc.Select(x => new PersonGroupCourseVm(x.Id, x.CourseId, x.Course.Name, x.GroupId, x.Group.Name));
        IEnumerable<GroupVm> groupsVm = groups.Select(x => new GroupVm(x.Id, x.Name));
        GetPersonGroupCoursesVm gcVm = new GetPersonGroupCoursesVm(pgcVm, groupsVm);

        Common.ViewModels.PersonVm personVm = new Common.ViewModels.PersonVm()
        {
            Name = person.Name,
            DocumentId = person.DocumentId,
            Surname1 = person.Surname1,
            Surname2 = person.Surname2,
            ContactMail = person.ContactMail,
            ContactPhone = person.ContactPhone,
        };

        Common.ViewModels.StudentVm? studentVm = null;
        Student? s = person as Student;
        if (s != null)
        {
            studentVm = new Common.ViewModels.StudentVm()
            {
                AcademicRecordNumber = s.AcademicRecordNumber,
                Amipa = s.Amipa,
                PreEnrollment = s.PreEnrollment,
                SubjectsInfo = s.SubjectsInfo,
            };
        }
        
        return new GetPersonByIdVm(
            gcVm,
            personVm,
            studentVm
        );
    }
}
