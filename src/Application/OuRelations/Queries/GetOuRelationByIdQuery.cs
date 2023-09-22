using Application.Common;
using Domain.Services;
using MediatR;
using Domain.Entities.GoogleApi;
using Domain.Entities.People;

namespace Application.OuRelations.Queries;
# region ViewModels
public record OuRelationVm
{
    public long Id { get; set; }
    public long GroupId { get; set; }
    public string GroupMail { get; set; } = string.Empty;
    public string OldOU { get; set; } = string.Empty;
    public string ActiveOU { get; set; } = string.Empty;
    public bool UpdatePassword { get; set; }
    public bool ChangePasswordNextSignIn { get; set; }
}

#endregion

#region Query
public record GetOuRelationByIdQuery(long Id) : IRequest<Response<OuRelationVm>>;
#endregion

public class GetOuRelationByIdQueryHandler : IRequestHandler<GetOuRelationByIdQuery, Response<OuRelationVm>>
{
    #region  IOC
    private readonly IOUGroupRelationsRepository _groupsRelationRepo;
            private readonly IGroupsRepository _groupsRepository;


    public GetOuRelationByIdQueryHandler(
        IOUGroupRelationsRepository groupsRelationRepo,
        IGroupsRepository groupsRepository
    )
    {
        _groupsRelationRepo = groupsRelationRepo;
        _groupsRepository = groupsRepository;

    }
    #endregion
    
    public async Task<Response<OuRelationVm>> Handle(GetOuRelationByIdQuery request, CancellationToken ct)
    {
        OuGroupRelation? relation = await _groupsRelationRepo.GetByIdAsync(request.Id, ct);
        if (relation == null) return Response<OuRelationVm>.Error(ResponseCode.NotFound, @"There is no OU Relational Group with this id");

        Group? group = await _groupsRepository.GetByIdAsync(relation.GroupId, ct);
        if (group == null) return Response<OuRelationVm>.Error(ResponseCode.NotFound, @"There is no Group with this id");

        var ouRelationVm = new OuRelationVm();

        ouRelationVm.Id = relation.Id;
        ouRelationVm.GroupId = relation.GroupId;
        ouRelationVm.GroupMail = relation.GroupMail;
        ouRelationVm.OldOU = relation.OldOU;
        ouRelationVm.ActiveOU = relation.ActiveOU;
        ouRelationVm.UpdatePassword = relation.UpdatePassword;
        ouRelationVm.ChangePasswordNextSignIn = relation.ChangePasswordNextSignIn;

        return Response<OuRelationVm>.Ok(ouRelationVm);
    }
}
