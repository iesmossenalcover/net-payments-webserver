using Application.Common;
using Domain.Services;
using MediatR;
using Domain.Entities.GoogleApi;

namespace Application.OURelations.Queries;
# region ViewModels
public record OURelationVm
{
    public long id { get; set; }
    public long GroupId { get; set; }
    public string GroupMail { get; set; } = string.Empty;
    public string OldOU { get; set; } = string.Empty;
    public string ActiveOU { get; set; } = string.Empty;
    public bool UpdatePassword { get; set; }
    public bool ChangePasswordNextSignIn { get; set; }
}

#endregion

#region Query
public record GetOURelationByIdQuery(long Id) : IRequest<Response<OURelationVm>>;
#endregion

public class GetOURelationByIdQueryHandler : IRequestHandler<GetOURelationByIdQuery, Response<OURelationVm>>
{
    #region  IOC
    private readonly IOUGroupRelationsRepository _groupsRelationRepo;
            private readonly IGroupsRepository _groupsRepository;


    public GetOURelationByIdQueryHandler(
        IOUGroupRelationsRepository groupsRelationRepo,
        IGroupsRepository groupsRepository
    )
    {
        _groupsRelationRepo = groupsRelationRepo;
        _groupsRepository = groupsRepository;

    }
    #endregion
    
    public async Task<Response<OURelationVm>> Handle(GetOURelationByIdQuery request, CancellationToken ct)
    {
        UoGroupRelation? relation = await _groupsRelationRepo.GetByIdAsync(request.Id, ct);
        if (relation == null) return Response<OURelationVm>.Error(ResponseCode.NotFound, "There is no OU Relational Group with this id");

        var group = await _groupsRepository.GetByIdAsync(relation.GroupId, ct);
        if (group == null) return Response<OURelationVm>.Error(ResponseCode.NotFound, "There is no Group with this id");

        OURelationVm OURelationVm = new OURelationVm();

        OURelationVm.id = relation.Id;
        OURelationVm.GroupId = relation.GroupId;
        OURelationVm.GroupMail = relation.GroupMail;
        OURelationVm.OldOU = relation.OldOU;
        OURelationVm.ActiveOU = relation.ActiveOU;
        OURelationVm.UpdatePassword = relation.UpdatePassword;
        OURelationVm.ChangePasswordNextSignIn = relation.ChangePasswordNextSignIn;

        return Response<OURelationVm>.Ok(OURelationVm);
    }

    Task<Response<OURelationVm>> IRequestHandler<GetOURelationByIdQuery, Response<OURelationVm>>.Handle(GetOURelationByIdQuery request, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}
