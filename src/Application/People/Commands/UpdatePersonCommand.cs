using Application.Common.Services;
using Domain.Entities.People;
using FluentValidation;
using MediatR;

namespace Application.People.Commands;

// Model we receive
public record UpdatePersonCommand : CreatePersonCommand, IRequest<long>
{
    public long Id { get; set; }

    public UpdatePersonCommand(long id)
    {
        Id = id;
    }
}

// Validator
public class UpdatePersonCommandValidator : AbstractValidator<UpdatePersonCommand>
{

    public UpdatePersonCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithMessage("El camp no pot ser buid.");
        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("El camp no pot ser buid.");

        RuleFor(x => x.Surname1)
            .NotEmpty().WithMessage("El camp no pot ser buid.");

        RuleFor(x => x.GroupId)
            .NotNull().NotEmpty().WithMessage("Com a mínim s'ha d'especificar un group ");

        RuleFor(x => x.DocumentId)
            .NotEmpty().WithMessage("Text must be not empty")
            .MaximumLength(50).WithMessage("Màxim 50 caràcters");

        RuleFor(x => x.ContactPhone)
            .MaximumLength(50).WithMessage("Màxim 15 caràcters");

        RuleFor(x => x.ContactMail)
            .MaximumLength(50).WithMessage("Màxim 100 caràcters");
    }
}

public class UpdatePersonCommandHandler : IRequestHandler<UpdatePersonCommand, long>
{
    private readonly IPeopleRepository _peopleRepo;
    private readonly IStudentsRepository _studentRepo;

    public UpdatePersonCommandHandler(
    IPeopleRepository peopleRepo,
    IStudentsRepository studentRepo)
    {
        _peopleRepo = peopleRepo;
        _studentRepo = studentRepo;
    }
    public async Task<long> Handle(UpdatePersonCommand request, CancellationToken ct)
    {
        //Comprovam que no existeix un usuari amb un DocumentID o Academic Record Number igual a la BBDD que no sigui l'usuari que volem actualitzar.

        Person ? p1 = await _peopleRepo.GetPersonByDocumentIdAsync(request.DocumentId, ct);
        Person ? p2 = await _studentRepo.GetStudentByAcademicRecordAsync(request.AcademicRecordNumber, ct);

        Person ? p = await _peopleRepo.GetByIdAsync(request.Id, ct);


        if (p == null || (p1 != null && p.Id != p1.Id) || (p2 != null && p.Id != p2.Id))
        {
            throw new Exception("Bad request");
        }

        //Actualitzar usuari
        Console.WriteLine("-------------" + request.Name);
        return p.Id;
    }
}