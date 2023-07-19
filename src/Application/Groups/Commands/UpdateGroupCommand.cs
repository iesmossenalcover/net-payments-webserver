using Application.Common;
using Domain.Services;
using Domain.Entities.People;
using FluentValidation;
using MediatR;

namespace Application.Groups.Commands;

// Model we receive
public record UpdateGroupCommand : CreateGroupCommand, IRequest<Response<long?>>
{
    public long Id { get; set; }

    public UpdateGroupCommand(long id)
    {
        Id = id;
    }
}

// Validator
public class UpdateGroupCommandValidator : AbstractValidator<UpdateGroupCommand>
{
    private readonly IGroupsRepository _groupRepository;

    public UpdateGroupCommandValidator(IGroupsRepository groupRepository)
    {
        _groupRepository = groupRepository;

        RuleFor(x => x.Id)
            .NotEmpty()
            .WithMessage("El camp no pot ser buid.");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("S'ha d'indicar un nom pe grup.")
            .MustAsync(CheckUniqueNameAsync).WithMessage("Ja existeix un grup amb aquest nom");
    }

    private async Task<bool> CheckUniqueNameAsync(UpdateGroupCommand cmd, string name, CancellationToken ct)
    {
        IEnumerable<Group> groups = await _groupRepository.GetGroupsByNameAsync(new string[] { name }, ct);
        return !groups.Any(x => x.Id != cmd.Id);
    }
}

public class UpdateGroupCommandHandler : IRequestHandler<UpdateGroupCommand, Response<long?>>
{

    private readonly IGroupsRepository _groupsRepo;

    public UpdateGroupCommandHandler(IGroupsRepository groupsRepo)
    {
        _groupsRepo = groupsRepo;

    }

    public async Task<Response<long?>> Handle(UpdateGroupCommand request, CancellationToken ct)
    {
        Group? g = await _groupsRepo.GetByIdAsync(request.Id, ct);

        if (g == null)
        {
            return Response<long?>.Error(ResponseCode.BadRequest, "Bad request from UpdateGroupCommand");
        }

        g.Name = request.Name;
        g.Description = request.Description;

        await _groupsRepo.UpdateAsync(g, CancellationToken.None);

        return Response<long?>.Ok(g.Id);
    }
}