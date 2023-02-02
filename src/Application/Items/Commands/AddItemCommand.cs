using FluentValidation;
using MediatR;

namespace Application.Items.Commands;

// Model we receive
public record AddItemCommand(string text) : IRequest<long>;

// Validator for the model
public class AddItemCommandValidator : AbstractValidator<AddItemCommand>
{
    public AddItemCommandValidator()
    {
        RuleFor(x => x.text).NotEmpty().WithMessage("Text must be not empty");
    }
}

// Optionally define a view model

// Handler
public class AddItemCommandHandler : IRequestHandler<AddItemCommand, long>
{
    public Task<long> Handle(AddItemCommand request, CancellationToken cancellationToken)
    {
        // program logic
        return Task.FromResult(1L);
    }
}