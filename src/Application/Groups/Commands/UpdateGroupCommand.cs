using Application.Common;
using Application.Common.Services;
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

    public UpdateGroupCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithMessage("El camp no pot ser buid.");
        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("El camp no pot ser buid.");
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