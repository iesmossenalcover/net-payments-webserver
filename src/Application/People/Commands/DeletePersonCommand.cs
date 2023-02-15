using Application.Common.Services;
using Domain.Entities.People;
using FluentValidation;
using MediatR;

namespace Application.People.Commands;

// Model we receive
public record DeletePersonCommand : IRequest<long>
{
    public long Id { get; set; }

    public DeletePersonCommand(long id)
    {
        Id = id;
    }
}

// Validator
public class DeletePersonCommandValidator : AbstractValidator<DeletePersonCommand>
{

    public DeletePersonCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithMessage("El camp no pot ser buid.");
    }
}

public class DeletePersonCommandHandler : IRequestHandler<DeletePersonCommand, long>
{
    private readonly IPeopleRepository _peopleRepo;

    public DeletePersonCommandHandler(
    IPeopleRepository peopleRepo)
    {
        _peopleRepo = peopleRepo;
    }
    public async Task<long> Handle(DeletePersonCommand request, CancellationToken ct)
    {
        Person ? p = await _peopleRepo.GetByIdAsync(request.Id, ct);
        if (p == null)
        {
            throw new Exception("Bad request");
        }

        await _peopleRepo.DeleteAsync(p, CancellationToken.None);
        
        return p.Id;
        
    }
}