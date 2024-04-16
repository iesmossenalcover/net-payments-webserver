using Domain.Services;
using MediatR;
using Domain.Entities.GoogleApi;

namespace Application.OuRelations.Queries;

# region ViewModels

public record OuRelationRowVm(long Id, string GroupName, string GroupMail, string OldOu, string ActiveOu);

public record ListOuRelationsVm(IEnumerable<OuRelationRowVm> OuGroupRelations);

#endregion

public record ListOuRelationsQuery() : IRequest<IEnumerable<OuRelationRowVm>>;

public class ListOuRelationsQueryHandler : IRequestHandler<ListOuRelationsQuery, IEnumerable<OuRelationRowVm>>
{
    # region IOC

    private readonly IOUGroupRelationsRepository _groupsRelationRepo;

    public ListOuRelationsQueryHandler(IOUGroupRelationsRepository groupsRelationRepo)
    {
        _groupsRelationRepo = groupsRelationRepo;
    }

    #endregion

    public async Task<IEnumerable<OuRelationRowVm>> Handle(ListOuRelationsQuery request, CancellationToken ct)
    {
        var uoGroups = await _groupsRelationRepo.GetAllWithRelationsAsync(ct);
        return uoGroups.Select(ToOuRelationRowVm).OrderBy(x => x.GroupMail);
    }

    private static OuRelationRowVm ToOuRelationRowVm(OuGroupRelation g)
    {
        return new OuRelationRowVm(
            g.Id,
            g.Group.Name,
            g.GroupMail,
            g.OldOU,
            g.ActiveOU
        );
    }
}