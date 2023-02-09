using MediatR;

namespace Application.People.Commands;

// Model we receive
public record UpdatePersonCommand : CreatePersonCommand, IRequest
{
    
}

