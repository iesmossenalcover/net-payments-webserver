using Domain.Services;
using MediatR;
using Domain.Entities.GoogleApi;

namespace Application.OURelations.Queries;

# region ViewModels
public record OURelationRowVm(long Id, string GroupMail, string OldOU, string ActiveOU);
public record ListOURelationsVm(IEnumerable<OURelationRowVm> OURelations);
#endregion

public record ListOURelationsQuery() : IRequest<IEnumerable<OURelationRowVm>>;

public class ListOURelationsQueryHandler : IRequestHandler<ListOURelationsQuery, IEnumerable<OURelationRowVm>>
{
    # region IOC
    private readonly IOUGroupRelationsRepository _groupsRelationRepo;

    public ListOURelationsQueryHandler(IOUGroupRelationsRepository groupsRelationRepo)
    {
        _groupsRelationRepo = groupsRelationRepo;
    }

    #endregion

    public async Task<IEnumerable<OURelationRowVm>> Handle(ListOURelationsQuery request, CancellationToken ct)
    {
        IEnumerable<UoGroupRelation> uogroups =  await _groupsRelationRepo.GetAllAsync(ct);
        return uogroups.Select(x => ToOURelationRowVm(x)).OrderBy(x => x.GroupMail);
    }

    public static OURelationRowVm ToOURelationRowVm(UoGroupRelation g)
    {        
        return new OURelationRowVm(
           g.Id,
           g.GroupMail,
           g.OldOU,
           g.ActiveOU
        );
    }

}
