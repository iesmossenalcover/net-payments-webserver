using Application.Common;
using Application.Common.Models;
using Domain.Services;
using MediatR;
using Domain.Entities.GoogleApi;
using Domain.Entities.People;

namespace Application.OuRelations.Queries;

# region ViewModels

public record OuRelationVm(
    long Id,
    long GroupId,
    string GroupMail,
    string OldOu,
    string ActiveOu,
    bool UpdatePassword,
    bool ChangePasswordNextSignIn);

public record OuRelationPageVm(OuRelationVm OuGroupRelation, SelectorVm Groups);

#endregion

#region Query

public record GetOuRelationByIdQuery(long Id) : IRequest<Response<OuRelationPageVm>>;

#endregion

public class GetOuRelationByIdQueryHandler : IRequestHandler<GetOuRelationByIdQuery, Response<OuRelationPageVm>>
{
    #region IOC

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

    public async Task<Response<OuRelationPageVm>> Handle(GetOuRelationByIdQuery request, CancellationToken ct)
    {
        OuGroupRelation? relation = await _groupsRelationRepo.GetByIdAsync(request.Id, ct);
        if (relation == null)
            return Response<OuRelationPageVm>.Error(ResponseCode.NotFound,
                @"There is no OU Relational Group with this id");

        IEnumerable<Group> groups = await _groupsRepository.GetAllAsync(ct);

        var ouRelationVm = new OuRelationVm(
            relation.Id,
            relation.GroupId,
            relation.GroupMail,
            relation.OldOU,
            relation.ActiveOU,
            relation.UpdatePassword,
            relation.ChangePasswordNextSignIn
        );

        SelectorVm selectorVm = new SelectorVm(
            groups
                .Select(x => new SelectOptionVm($"{x.Id}", x.Name))
                .OrderBy(x => x.Value)
        );

        return Response<OuRelationPageVm>.Ok(new OuRelationPageVm(ouRelationVm, selectorVm));
    }
}