using Application.Common;
using Domain.Services;
using FluentValidation;
using MediatR;
using Domain.Entities.GoogleApi;

namespace Application.OURelations.Commands;

// Model we receive
public record UpdateOURelationCommand : CreateOURelationCommand, IRequest<Response<long?>>
{
    public long Id { get; set; }

    public UpdateOURelationCommand(long id)
    {
        Id = id;
    }
}

// Validator
public class UpdateOURelationCommandValidator : AbstractValidator<UpdateOURelationCommand>
{
    private readonly IOUGroupRelationsRepository _groupsRelationRepo;

    public UpdateOURelationCommandValidator(IOUGroupRelationsRepository groupsRelationRepo)

    {
        _groupsRelationRepo = groupsRelationRepo;
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithMessage("El camp no pot ser buid.");
    }
}

public class UpdateOURelationCommandHandler : IRequestHandler<UpdateOURelationCommand, Response<long?>>
{

    private readonly IOUGroupRelationsRepository _groupsRelationRepo;
    private readonly IGroupsRepository _groupsRepo;

    public UpdateOURelationCommandHandler(IGroupsRepository groupsRepo, IOUGroupRelationsRepository groupsRelationRepo)
    {
        _groupsRepo = groupsRepo;
        _groupsRelationRepo = groupsRelationRepo;
    }


    private async Task<bool> CheckGroupExistsAsync(UpdateOURelationCommand cmd, long id, CancellationToken ct)
    {
        var group = await _groupsRepo.GetByIdAsync(id, ct);
        return group != null;
    }

    public async Task<Response<long?>> Handle(UpdateOURelationCommand request, CancellationToken ct)
    {
        UoGroupRelation? relation = await _groupsRelationRepo.GetByIdAsync(request.Id, ct);

        if (relation == null)
        {
            return Response<long?>.Error(ResponseCode.BadRequest, "Bad request from UpdateOURelationCommand");
        }

        var group = await _groupsRepo.GetByIdAsync(request.GroupId, ct);
        if (group == null) return Response<long?>.Error(ResponseCode.BadRequest, "There is no Group with this id");

        relation.GroupId = request.GroupId;
        relation.GroupMail = request.GroupMail;
        relation.OldOU = request.OldOU;
        relation.ActiveOU = request.ActiveOU;
        relation.UpdatePassword = request.UpdatePassword;
        relation.ChangePasswordNextSignIn = request.ChangePasswordNextSignIn;

        await _groupsRelationRepo.UpdateAsync(relation, CancellationToken.None);

        return Response<long?>.Ok(relation.Id);
    }
}