using Application.Common;
using Application.Common.Services;
using Domain.Entities.People;
using FluentValidation;
using MediatR;

namespace Application.People.Commands;

// Model we receive
public record CreatePersonCommand : IRequest<Response<long?>>
{
    public string Name { get; set; } = string.Empty;
    public string Surname1 { get; set; } = string.Empty;
    public string? Surname2 { get; set; }
    public string DocumentId { get; set; } = string.Empty;
    public string? ContactPhone { get; set; }
    public string? ContactMail { get; set; }
    public long GroupId { get; set; }

    // Options student info
    public long? AcademicRecordNumber { get; set; }
    public string? SubjectsInfo { get; set; }
    public bool Amipa { get; set; }
}

// Validator
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

        RuleFor(x => x.GroupId)
            .NotNull().NotEmpty().WithMessage("Com a mínim s'ha d'especificar un grup.");

        RuleFor(x => x.DocumentId)
            .NotEmpty().WithMessage("Text must be not empty")
            .MaximumLength(50).WithMessage("Màxim 50 caràcters")
            .MustAsync(async (DocumentId, ct) =>
            {
                return await _peopleRepo.GetPersonByDocumentIdAsync(DocumentId, ct) == null;
            }).WithMessage("Ja existeix una persona amb aquest document identificatiu.");

        RuleFor(x => x.AcademicRecordNumber)
            .Must(x => {
                if (x.HasValue && x.Value == 0) return false;

                return true;
            })
            .WithMessage("Si és un estudiant, s'ha d'indicar l'expedient acadèmic.")
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
public class CreatePersonCommandHandler : IRequestHandler<CreatePersonCommand, Response<long?>>
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

    public async Task<Response<long?>> Handle(CreatePersonCommand request, CancellationToken ct)
    {
        Course course = await _coursesRepo.GetCurrentCoursAsync(ct);
        Group? group = await _groupsRepo.GetByIdAsync(request.GroupId, ct);

        if (group == null) return Response<long?>.Error(ResponseCode.NotFound, nameof(request.GroupId), "Specified group does not exist");

        Person p;
        if (request.AcademicRecordNumber.HasValue)
        {
            p = new Student()
            {
                AcademicRecordNumber = request.AcademicRecordNumber.Value,
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

        var pgc = new PersonGroupCourse()
        {
            Person = p,
            Course = course,
            Group = group,
            Amipa = request.Amipa,
        };

        await _peopleRepo.InsertAsync(p, CancellationToken.None);
        await _personGroupCourseRepo.InsertAsync(pgc, CancellationToken.None);

        return Response<long?>.Ok(p.Id);
    }
}