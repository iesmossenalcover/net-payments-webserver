using Application.Common.Models;
using Domain.Services;
using Domain.Entities.People;
using MediatR;

namespace Application.Groups.Queries;

public record GetAllGroupsSelectorQuery() : IRequest<SelectorVm>;

public class GetAllGroupsSelectorQueryHandler : IRequestHandler<GetAllGroupsSelectorQuery, SelectorVm>
{
    private readonly IGroupsRepository _groupsRepository;

    public GetAllGroupsSelectorQueryHandler(IGroupsRepository groupsRepository)
    {
        _groupsRepository = groupsRepository;
    }

    public async Task<SelectorVm> Handle(GetAllGroupsSelectorQuery request, CancellationToken ct)
    {
        IEnumerable<Group> groups = (await _groupsRepository.GetAllAsync(ct)).OrderBy(x => x.Name);
        long activeGroupId = groups.Any() ? groups.First().Id : 0;

        List<SelectOptionVm> options = new List<SelectOptionVm>(groups.Count());

        foreach (var c in groups)
        {
            options.Add(new SelectOptionVm(c.Id.ToString(), c.Name));
        }
        return new SelectorVm(options);
    }
}