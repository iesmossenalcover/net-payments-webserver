using Application.Common.Services;
using Domain.Entities.People;
using FluentValidation;
using MediatR;

namespace Application.People.Commands;

// Model we receive
public record UpdatePersonCommand : CreatePersonCommand
{
    public long Id { get; set; }
}

// Validator
public class UpdatePersonCommandValidator : AbstractValidator<UpdatePersonCommand>
{

    public UpdatePersonCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("El camp no pot ser buid.");

        RuleFor(x => x.Surname1)
            .NotEmpty().WithMessage("El camp no pot ser buid.");

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
    public Task<long> Handle(UpdatePersonCommand request, CancellationToken cancellationToken)
    {
        // validacion més complexes
        // Si el ja existeix una persona diferent a la que actualitze amb el mateix id --> error
        // Si ja existeix un alumne amb el mateix expedient diferent al que actualitzem --> error
        throw new NotImplementedException();
    }
}