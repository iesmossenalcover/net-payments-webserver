using Application.Common;
using Domain.Services;
using Domain.Entities.People;
using FluentValidation;
using MediatR;
using Domain.Entities.GoogleApi;

namespace Application.OURelations.Commands;

// Model we receive
public record CreateOURelationCommand : IRequest<Response<long?>>
{
    public long GroupId { get; set; }
    public string GroupMail { get; set; } = string.Empty;
    public string OldOU { get; set; } = string.Empty;
    public string ActiveOU { get; set; } = string.Empty;
    public bool UpdatePassword { get; set; }
    public bool ChangePasswordNextSignIn { get; set; }

}

// Validator
public class CreateOURelationCommandValidator : AbstractValidator<CreateOURelationCommand>
{
    private readonly IGroupsRepository _groupsRepository;

    public CreateOURelationCommandValidator(IGroupsRepository groupsRepository)
    {
        _groupsRepository = groupsRepository;

        RuleFor(x => x.GroupMail)
        .NotEmpty().WithMessage("S'ha d'indicar un GroupMail.");
        RuleFor(x => x.OldOU)
        .NotEmpty().WithMessage("S'ha d'indicar un OldOU.");
        RuleFor(x => x.ActiveOU)
        .NotEmpty().WithMessage("S'ha d'indicar un ActiveOU.");
        RuleFor(x => x.GroupId)
        .NotEmpty().WithMessage("S'ha d'indicar un GroupId.")
        .MustAsync(CheckGroupExistsAsync).WithMessage("Ja existeix un grup amb aquest nom");
    }
    private async Task<bool> CheckGroupExistsAsync(CreateOURelationCommand cmd, long id, CancellationToken ct)
    {
        var group = await _groupsRepository.GetByIdAsync(id, ct);
        return group != null;
    }
}

// Handler
public class CreateOURelationCommandHandler : IRequestHandler<CreateOURelationCommand, Response<long?>>
{

    private readonly IOUGroupRelationsRepository _groupsRelationRepo;

    public CreateOURelationCommandHandler(
        IOUGroupRelationsRepository groupsRelationRepo
    )
    {
        _groupsRelationRepo = groupsRelationRepo;
    }

    public async Task<Response<long?>> Handle(CreateOURelationCommand request, CancellationToken ct)
    {

        UoGroupRelation uorelation = new UoGroupRelation()
        {
            GroupId = request.GroupId,
            GroupMail = request.GroupMail,
            OldOU = request.OldOU,
            ActiveOU = request.ActiveOU,
            UpdatePassword = request.UpdatePassword,
            ChangePasswordNextSignIn = request.ChangePasswordNextSignIn,

        };

        await _groupsRelationRepo.InsertAsync(uorelation, CancellationToken.None);

        return Response<long?>.Ok(uorelation.Id);
    }
}