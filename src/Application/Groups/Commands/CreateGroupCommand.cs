using Application.Common;
using Domain.Services;
using Domain.Entities.People;
using FluentValidation;
using MediatR;

namespace Application.Groups.Commands;

// Model we receive
public record CreateGroupCommand : IRequest<Response<long?>>
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }

}

// Validator
public class CreateGroupCommandValidator : AbstractValidator<CreateGroupCommand>
{
    private readonly IGroupsRepository _groupsRepository;

    public CreateGroupCommandValidator(IGroupsRepository groupsRepository)
    {
        _groupsRepository = groupsRepository;

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("S'ha d'indicar un nom pe grup.")
            .MustAsync(CheckUniqueNameAsync).WithMessage("Ja existeix un grup amb aquest nom");
    }

    private async Task<bool> CheckUniqueNameAsync(CreateGroupCommand cmd, string name, CancellationToken ct)
    {
        IEnumerable<Group> groups = await _groupsRepository.GetGroupsByNameAsync(new string[] { name }, ct);
        return !groups.Any();
    }
}

// Handler
public class CreateGroupCommandHandler : IRequestHandler<CreateGroupCommand, Response<long?>>
{

    private readonly IGroupsRepository _groupsRepo;

    public CreateGroupCommandHandler(
        IGroupsRepository groupsRepo
    )
    {
        _groupsRepo = groupsRepo;
    }

    public async Task<Response<long?>> Handle(CreateGroupCommand request, CancellationToken ct)
    {

        Group g = new Group()
        {
            Name = request.Name,
            Description = request.Description,
            Created = DateTimeOffset.UtcNow
        };

        await _groupsRepo.InsertAsync(g, CancellationToken.None);

        return Response<long?>.Ok(g.Id);
    }
}