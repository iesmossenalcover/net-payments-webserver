using Application.Common;
using Domain.Services;
using Domain.Entities.People;
using FluentValidation;
using MediatR;
using Domain.Entities.GoogleApi;

namespace Application.OuRelations.Commands;

// Model we receive
public record CreateOuRelationCommand : IRequest<Response<long?>>
{
    public long GroupId { get; set; }
    public string GroupMail { get; set; } = string.Empty;
    public string OldOu { get; set; } = string.Empty;
    public string ActiveOu { get; set; } = string.Empty;
    public bool UpdatePassword { get; set; }
    public bool ChangePasswordNextSignIn { get; set; }

}

// Validator
public class CreateOURelationCommandValidator : AbstractValidator<CreateOuRelationCommand>
{
    private readonly IGroupsRepository _groupsRepository;

    public CreateOURelationCommandValidator(IGroupsRepository groupsRepository)
    {
        _groupsRepository = groupsRepository;

        RuleFor(x => x.GroupMail)
        .NotEmpty().WithMessage(@"S'ha d'indicar un GroupMail.");
        RuleFor(x => x.OldOu)
        .NotEmpty().WithMessage(@"S'ha d'indicar un OldOU.");
        RuleFor(x => x.ActiveOu)
        .NotEmpty().WithMessage(@"S'ha d'indicar un ActiveOU.");
        RuleFor(x => x.GroupId)
        .NotEmpty().WithMessage(@"S'ha d'indicar un Group.")
        .MustAsync(CheckGroupExistsAsync).WithMessage(@"El grup seleccionat no existeix.");
    }
    private async Task<bool> CheckGroupExistsAsync(CreateOuRelationCommand cmd, long id, CancellationToken ct)
    {
        if (id == 0) return true;
        Group? group = await _groupsRepository.GetByIdAsync(id, ct);
        return group != null;
    }
}

// Handler
public class CreateOURelationCommandHandler : IRequestHandler<CreateOuRelationCommand, Response<long?>>
{

    private readonly IOUGroupRelationsRepository _groupsRelationRepo;

    public CreateOURelationCommandHandler(
        IOUGroupRelationsRepository groupsRelationRepo
    )
    {
        _groupsRelationRepo = groupsRelationRepo;
    }

    public async Task<Response<long?>> Handle(CreateOuRelationCommand request, CancellationToken ct)
    {

        OuGroupRelation uorelation = new OuGroupRelation()
        {
            GroupId = request.GroupId,
            GroupMail = request.GroupMail,
            OldOU = request.OldOu,
            ActiveOU = request.ActiveOu,
            UpdatePassword = request.UpdatePassword,
            ChangePasswordNextSignIn = request.ChangePasswordNextSignIn,

        };

        await _groupsRelationRepo.InsertAsync(uorelation, CancellationToken.None);

        return Response<long?>.Ok(uorelation.Id);
    }
}