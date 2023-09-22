using Application.Common;
using Domain.Services;
using FluentValidation;
using MediatR;
using Domain.Entities.GoogleApi;
using Domain.Entities.People;

namespace Application.OuRelations.Commands;

// Model we receive
public record UpdateOuRelationCommand : CreateOuRelationCommand, IRequest<Response<long?>>
{
    public long Id { get; set; }

    public UpdateOuRelationCommand(long id)
    {
        Id = id;
    }
}

// Validator
public class UpdateOuRelationCommandValidator : AbstractValidator<UpdateOuRelationCommand>
{
    private readonly IGroupsRepository _groupsRepository;

    public UpdateOuRelationCommandValidator(IGroupsRepository groupsRepository)

    {
        _groupsRepository = groupsRepository;
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithMessage(@"El camp no pot ser buid.");
        RuleFor(x => x.GroupMail)
            .NotEmpty().WithMessage(@"S'ha d'indicar un GroupMail.");
        RuleFor(x => x.OldOU)
            .NotEmpty().WithMessage(@"S'ha d'indicar un OldOU.");
        RuleFor(x => x.ActiveOU)
            .NotEmpty().WithMessage(@"S'ha d'indicar un ActiveOU.");
        RuleFor(x => x.GroupId)
            .NotEmpty().WithMessage(@"S'ha d'indicar un GroupId.")
            .MustAsync(CheckGroupExistsAsync).WithMessage(@"Ja existeix un grup amb aquest nom");
    }

    private async Task<bool> CheckGroupExistsAsync(CreateOuRelationCommand cmd, long id, CancellationToken ct)
    {
        Group? group = await _groupsRepository.GetByIdAsync(id, ct);
        return group != null;
    }
}

public class UpdateOuRelationCommandHandler : IRequestHandler<UpdateOuRelationCommand, Response<long?>>
{
    private readonly IOUGroupRelationsRepository _groupsRelationRepo;
    private readonly IGroupsRepository _groupsRepo;

    public UpdateOuRelationCommandHandler(IGroupsRepository groupsRepo, IOUGroupRelationsRepository groupsRelationRepo)
    {
        _groupsRepo = groupsRepo;
        _groupsRelationRepo = groupsRelationRepo;
    }


    private async Task<bool> CheckGroupExistsAsync(UpdateOuRelationCommand cmd, long id, CancellationToken ct)
    {
        var group = await _groupsRepo.GetByIdAsync(id, ct);
        return group != null;
    }

    public async Task<Response<long?>> Handle(UpdateOuRelationCommand request, CancellationToken ct)
    {
        OuGroupRelation? relation = await _groupsRelationRepo.GetByIdAsync(request.Id, ct);

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