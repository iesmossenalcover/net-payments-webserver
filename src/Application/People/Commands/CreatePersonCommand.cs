using Application.Common.Services;
using Domain.Entities.People;
using FluentValidation;
using MediatR;

namespace Application.People.Commands;

// Model we receive
public record PersonGroupCourseModel(long GroupId, long CourseId);
public record CreatePersonCommand : IRequest<long>
{
    public string Name { get; set; } = string.Empty;
    public string Surname1 { get; set; } = string.Empty;
    public string? Surname2 { get; set; }
    public string DocumentId { get; set; } = string.Empty;
    public string? ContactPhone { get; set; }
    public string? ContactMail { get; set; }

    // Group Courses info
    public IEnumerable<PersonGroupCourseModel> GroupCourses { get; set; } = Enumerable.Empty<PersonGroupCourseModel>();

    // Options student info
    public long? AcademicRecordNumber { get; set; }
    public string? SubjectsInfo { get; set; }
    public bool PreEnrollment { get; set; }
    public bool Amipa { get; set; }
}

public class CreatePersonCommandValidator : AbstractValidator<CreatePersonCommand>
{
    private readonly IPeopleRepository _peopleRepo;
    private readonly IStudentsRepository _studentRepo;

    public CreatePersonCommandValidator(IPeopleRepository peopleRepo, IStudentsRepository studentRepo)
    {
        _peopleRepo = peopleRepo;
        _studentRepo = studentRepo;

        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("El camp no pot ser buid.");

        RuleFor(x => x.Surname1)
            .NotEmpty().WithMessage("El camp no pot ser buid.");

        RuleFor(x => x.GroupCourses)
            .NotNull().NotEmpty().WithMessage("Com a mínim s'ha d'especificar un group i curs.");                

        RuleFor(x => x.DocumentId)
            .NotEmpty().WithMessage("Text must be not empty")
            .MaximumLength(50).WithMessage("Màxim 50 caràcters")
            .MustAsync(async (DocumentId, ct) =>
            {
                return await _peopleRepo.GetPersonByDocumentIdAsync(DocumentId, ct) == null;
            }).WithMessage("Ja existeix una persona amb aquest document identificatiu.");

        RuleFor(x => x.AcademicRecordNumber)
            .MustAsync(async (x, ct) =>
            {
                if (!x.HasValue) return true;
                return await studentRepo.GetStudentByAcademicRecordAsync(x.Value, ct) == null;
                
            }).WithMessage("Ja existeix un alumne amb aquest número d'expedient.");

        RuleFor(x => x.ContactPhone)
            .MaximumLength(50).WithMessage("Màxim 15 caràcters");

        RuleFor(x => x.ContactMail)
            .MaximumLength(50).WithMessage("Màxim 100 caràcters");
    }
}

// Handler
public class CreatePersonCommandHandler : IRequestHandler<CreatePersonCommand, long>
{
    private readonly IPeopleRepository _peopleRepo;
    private readonly ICoursesRepository _coursesRepo;
    private readonly IGroupsRepository _groupsRepo;
    private readonly IPersonGroupCourseRepository _personGroupCourseRepo;

    public CreatePersonCommandHandler(
        IPeopleRepository peopleRepo,
        IPersonGroupCourseRepository personGroupCourseRepo,
        ICoursesRepository coursesRepo,
        IGroupsRepository groupsRepo)
    {
        _peopleRepo = peopleRepo;
        _personGroupCourseRepo = personGroupCourseRepo;
        _coursesRepo = coursesRepo;
        _groupsRepo = groupsRepo;
    }

    public async Task<long> Handle(CreatePersonCommand request, CancellationToken ct)
    {
        IEnumerable<Course> courses = await _coursesRepo.GetAllAsync(ct);
        IEnumerable<Group> groups = await _groupsRepo.GetByIdAsync(request.GroupCourses.Select(x => x.GroupId), ct);

        Person p;
        if (request.AcademicRecordNumber.HasValue)
        {
            p = new Student()
            {
                AcademicRecordNumber = request.AcademicRecordNumber.Value,
                Amipa = false,
                PreEnrollment = request.PreEnrollment,
                SubjectsInfo = request.SubjectsInfo,
            };
        }
        else
        {
            p = new Person();
        }
        
        p.Name = request.Name;
        p.DocumentId = request.DocumentId;
        p.ContactMail = request.ContactMail;
        p.ContactPhone = request.ContactPhone;
        p.Surname1 = request.Surname1;
        p.Surname2 = request.Surname2;
        

        // Create group courses assignations.
        List<PersonGroupCourse> personGroupCourses = new List<PersonGroupCourse>(request.GroupCourses.Count());
        foreach (var pgc in request.GroupCourses)
        {
            var group = groups.FirstOrDefault(x => x.Id == pgc.GroupId);
            if (group == null)
            {
                throw new Exception("Bad request");
            }

            var course = courses.FirstOrDefault(x => x.Id == pgc.CourseId);
            if (course == null)
            {
                throw new Exception("Bad request");
            }
            personGroupCourses.Add(new PersonGroupCourse()
            {
                Person = p,
                Group = group,
                Course = course,
            });
        }

        await _peopleRepo.InsertAsync(p, CancellationToken.None);
        await _personGroupCourseRepo.InsertManyAsync(personGroupCourses, CancellationToken.None);

        return p.Id;
    }
}