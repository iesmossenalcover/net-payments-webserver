using Application.Common.Services;
using Domain.Entities.People;
using FluentValidation;
using MediatR;

namespace Application.People.Commands;

// Model we receive
public record CreatePersonCommand : IRequest<long>
{
    public long? AcademicRecordNumber { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Surname1 { get; set; } = string.Empty;
    public string? Surname2 { get; set; }
    public string DocumentId { get; set; } = string.Empty;
    public string? ContactPhone { get; set; }
    public string? ContactMail { get; set; }
    public string? SubjectsInfo { get; set; }
    public bool PreEnrollment { get; set; }
}


public class CreatePersonCommandValidator : AbstractValidator<CreatePersonCommand>
{
    private readonly IPeopleService _peopleService;

    public CreatePersonCommandValidator(IPeopleService peopleService)
    {
        _peopleService = peopleService;
        RuleFor(x => x.Name).NotEmpty().WithMessage("Text must be not empty");
        RuleFor(x => x.DocumentId)
            .NotEmpty().WithMessage("Text must be not empty")
            .MustAsync(async (DocumentId, ct) =>
            {
                return !await _peopleService.IfPersonExistsAsync(DocumentId, ct);
            }).WithMessage("Ja existeix una persona amb aquest document identificatiu");

        RuleFor(x => x.AcademicRecordNumber)
            .MustAsync(async (x, ct) =>
            {
                if (!x.HasValue) return true;

                return !await _peopleService.IfStudentExistsAsync(x.Value, ct);
            }).WithMessage("L'alumne que es vol introduir ja existeix a la BBDD");
    }
}

// Handler
public class CreatePersonCommandHandler : IRequestHandler<CreatePersonCommand, long>
{
    private readonly IPeopleService _peopleService;

    public CreatePersonCommandHandler(IPeopleService peopleService)
    {
        _peopleService = peopleService;
    }
    public async Task<long> Handle(CreatePersonCommand request, CancellationToken cancellationToken)
    {
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

        await _peopleService.InsertPersonAsync(p);

        return p.Id;
    }
}