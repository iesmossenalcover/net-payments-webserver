using Application.Common;
using Domain.Services;
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
    public long? AcademicRecordNumber { get; set; }
    // Current course  data
    public long? GroupId { get; set; }
    public string? SubjectsInfo { get; set; }
    public bool Amipa { get; set; } = false;
    public bool Enrolled { get; set; } = false;
}

// Validator
public class CreatePersonCommandValidator : AbstractValidator<CreatePersonCommand>
{
    private readonly IPeopleRepository _peopleRepo;

    public CreatePersonCommandValidator(IPeopleRepository peopleRepo)
    {
        _peopleRepo = peopleRepo;

        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("El camp no pot ser buid.");

        // Comment rule to allow none grup.
        RuleFor(x => x.GroupId)
            .NotNull().WithMessage("S'ha d'especificar un grup")
            .GreaterThan(0).WithMessage("S'ha d'especificar un grup");

        RuleFor(x => x.Surname1)
            .NotEmpty().WithMessage("El camp no pot ser buid.");

        RuleFor(x => x.DocumentId)
            .NotEmpty().WithMessage("És obligatori posar un document d'indentitat")
            .MaximumLength(50).WithMessage("Màxim 50 caràcters.")
            .MustAsync(async (DocumentId, ct) =>
            {
                return await _peopleRepo.GetPersonByDocumentIdAsync(DocumentId, ct) == null;
            }).WithMessage("Ja existeix una persona amb aquest document identificatiu.");

        RuleFor(x => x.AcademicRecordNumber)
            .Must(x =>
            {
                if (x.HasValue && x.Value == 0) return false;

                return true;
            })
            .WithMessage("Si és un estudiant, s'ha d'indicar l'expedient acadèmic.")
            .MustAsync(async (x, ct) =>
            {
                if (!x.HasValue) return true;
                return await _peopleRepo.GetPersonByAcademicRecordAsync(x.Value, ct) == null;

            }).WithMessage("Ja existeix un alumne amb aquest número d'expedient.");

        RuleFor(x => x.ContactPhone)
            .MaximumLength(50).WithMessage("Màxim 15 caràcters");
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

        Person p = new Person()
        {
            Name = request.Name,
            DocumentId = request.DocumentId,
            ContactPhone = request.ContactPhone,
            Surname1 = request.Surname1,
            Surname2 = request.Surname2,
            AcademicRecordNumber = request.AcademicRecordNumber,
        };

        if (request.GroupId.HasValue)
        {
            Group? group = await _groupsRepo.GetByIdAsync(request.GroupId.Value, ct);
            if (group == null) return Response<long?>.Error(ResponseCode.NotFound, nameof(request.GroupId), "El grup especificat no existeix.");

            var pgc = new PersonGroupCourse()
            {
                Person = p,
                Course = course,
                Group = group,
                Amipa = request.Amipa,
                Enrolled = request.Enrolled,
                SubjectsInfo = request.SubjectsInfo,
            };
            await _personGroupCourseRepo.InsertAsync(pgc, CancellationToken.None);
        }
        else
        {
            await _peopleRepo.InsertAsync(p, CancellationToken.None);
        }

        return Response<long?>.Ok(p.Id);
    }
}