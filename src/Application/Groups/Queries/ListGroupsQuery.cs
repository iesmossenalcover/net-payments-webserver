using Domain.Services;
using Domain.Entities.People;
using MediatR;

namespace Application.Groups.Queries;

# region ViewModels
public record GroupRowVm(long Id, string Name, string? Description);
public record ListGroupsVm(IEnumerable<GroupRowVm> Groups);
#endregion

public record ListGroupsQuery() : IRequest<IEnumerable<GroupRowVm>>;

public class ListGroupsQueryHandler : IRequestHandler<ListGroupsQuery, IEnumerable<GroupRowVm>>
{
    # region IOC
    private readonly IGroupsRepository _groupsRepository;

    public ListGroupsQueryHandler(IGroupsRepository groupsRepository)
    {
        _groupsRepository = groupsRepository;
    }

    #endregion

    public async Task<IEnumerable<GroupRowVm>> Handle(ListGroupsQuery request, CancellationToken ct)
    {
        IEnumerable<Group> groups =  await _groupsRepository.GetAllAsync(ct);
        return groups.Select(x => ToGroupRowVm(x)).OrderBy(x => x.Name);
    }

    public static GroupRowVm ToGroupRowVm(Group g)
    {        
        return new GroupRowVm(
           g.Id,
           g.Name,
           g.Description
        );
    }

}
