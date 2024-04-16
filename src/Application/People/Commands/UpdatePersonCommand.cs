using Application.Common;
using Domain.Services;
using Domain.Entities.People;
using FluentValidation;
using MediatR;

namespace Application.People.Commands;

public record UpdatePersonCommand : CreatePersonCommand, IRequest<Response<long?>>
{
    public long Id { get; set; }

    public string? Email { get; set; }

    public UpdatePersonCommand(long id)
    {
        Id = id;
    }
}

// Validator
public class UpdatePersonCommandValidator : AbstractValidator<UpdatePersonCommand>
{
    private readonly IPeopleRepository _peopleRepository;

    public UpdatePersonCommandValidator(IPeopleRepository peopleRepository)
    {
        _peopleRepository = peopleRepository;

        RuleFor(x => x.Id)
            .NotEmpty()
            .WithMessage("El camp no pot ser buid.");

        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("El camp no pot ser buid.");

        RuleFor(x => x.GroupId)
            .NotNull().WithMessage("S'ha d'especificar un grup")
            .GreaterThan(0).WithMessage("S'ha d'especificar un grup");

        RuleFor(x => x.Surname1)
            .NotEmpty().WithMessage("El camp no pot ser buid.");

        RuleFor(x => x.DocumentId)
            .NotEmpty().WithMessage("És obligatori posar un document d'indentitat")
            .MaximumLength(50).WithMessage("Màxim 50 caràcters")
            .MustAsync(CheckUniqueDocumentIdAsync).WithMessage("Aquest document d'identitat ja està associat a una altra persona");

        RuleFor(x => x.AcademicRecordNumber)
            .MustAsync(CheckUniqueAcademicRecordNumberdAsync).WithMessage("Aquest expedient acadèmic ja està associat a una altra persona");

        RuleFor(x => x.ContactPhone)
            .MaximumLength(50).WithMessage("Màxim 15 caràcters");

        RuleFor(x => x.Email)
            .MustAsync(CheckUniqueEmailAsync).WithMessage("Aquest email ja està associat a una altra persona.");
    }

    private async Task<bool> CheckUniqueAcademicRecordNumberdAsync(UpdatePersonCommand cmd, long? academicRecordNumber, CancellationToken ct)
    {
        if (!academicRecordNumber.HasValue) return true;

        Person? p = await _peopleRepository.GetPersonByAcademicRecordAsync(academicRecordNumber.Value, ct);
        if (p == null)
        {
            return true;
        }
        else
        {
            return p.Id == cmd.Id;
        }
    }

    private async Task<bool> CheckUniqueDocumentIdAsync(UpdatePersonCommand cmd, string documentId, CancellationToken ct)
    {
        if (string.IsNullOrEmpty(documentId)) return false;
        Person? p = await _peopleRepository.GetPersonByDocumentIdAsync(documentId, ct);
        if (p == null)
        {
            return true;
        }
        else
        {
            return p.Id == cmd.Id;
        }
    }

    private async Task<bool> CheckUniqueEmailAsync(UpdatePersonCommand cmd, string? email, CancellationToken ct)
    {
        if (string.IsNullOrEmpty(email)) return true;
        Person? p = await _peopleRepository.GetPersonByEmailAsync(email, ct);
        if (p == null)
        {
            return true;
        }
        else
        {
            return p.Id == cmd.Id;
        }
    }
}

public class UpdatePersonCommandHandler : IRequestHandler<UpdatePersonCommand, Response<long?>>
{
    private readonly IPeopleRepository _peopleRepo;
    private readonly IPersonGroupCourseRepository _personGroupCourseRepo;
    private readonly ICoursesRepository _coursesRespository;

    public UpdatePersonCommandHandler(IPeopleRepository peopleRepo, IPersonGroupCourseRepository personGroupCourseRepo, ICoursesRepository coursesRespository)
    {
        _peopleRepo = peopleRepo;
        _personGroupCourseRepo = personGroupCourseRepo;
        _coursesRespository = coursesRespository;
    }

    public async Task<Response<long?>> Handle(UpdatePersonCommand request, CancellationToken ct)
    {
        Person? p = await _peopleRepo.GetByIdAsync(request.Id, ct);
        if (p == null)
        {
            return Response<long?>.Error(ResponseCode.BadRequest, "La persona que es vol actualitzar no existeix.");
        }

        p.Name = request.Name;
        p.ContactPhone = request.ContactPhone;
        p.ContactMail = request.Email;
        p.DocumentId = request.DocumentId;
        p.Surname1 = request.Surname1;
        p.Surname2 = request.Surname2;
        p.AcademicRecordNumber = request.AcademicRecordNumber;

        await _peopleRepo.UpdateAsync(p, CancellationToken.None);

        Course c = await _coursesRespository.GetCurrentCoursAsync(ct);

        // Update / create pgc
        PersonGroupCourse? pgc = await _personGroupCourseRepo.GetCoursePersonGroupById(p.Id, c.Id, CancellationToken.None);
        if (pgc == null && request.GroupId.HasValue) // Create PGC
        {
            pgc = new PersonGroupCourse()
            {
                CourseId = c.Id,
                GroupId = request.GroupId.Value,
                PersonId = p.Id,
                Amipa = request.Amipa,
                Enrolled = request.Enrolled,
            };
            await _personGroupCourseRepo.InsertAsync(pgc, CancellationToken.None);
        }
        else if (pgc != null && request.GroupId.HasValue) // Update PGC
        {
            pgc.GroupId = request.GroupId.Value;
            pgc.Amipa = request.Amipa;
            if (!request.Enrolled)
            {
                pgc.EnrollmentEvent = null;
                pgc.EnrollmentEventId = null;
            }
            pgc.Enrolled = request.Enrolled;


            pgc.SubjectsInfo = request.SubjectsInfo;
            await _personGroupCourseRepo.UpdateAsync(pgc, CancellationToken.None);
        }
        else if (pgc != null && !request.GroupId.HasValue) // Delete PGC
        {
            await _personGroupCourseRepo.DeleteAsync(pgc, CancellationToken.None);
        }

        return Response<long?>.Ok(p.Id);
    }
}