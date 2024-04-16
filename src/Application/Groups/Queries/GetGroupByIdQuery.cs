using Application.Common;
using Domain.Services;
using Domain.Entities.People;
using MediatR;

namespace Application.Groups.Queries;
# region ViewModels
public record GroupVm
{
    public long id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}

#endregion

#region Query
public record GetGroupByIdQuery(long Id) : IRequest<Response<GroupVm>>;
#endregion

public class GetGroupByIdQueryHandler : IRequestHandler<GetGroupByIdQuery, Response<GroupVm>>
{
    #region  IOC
    private readonly IGroupsRepository _groupsRepository;

        public GetGroupByIdQueryHandler(IGroupsRepository groupsRepository)
    {
        _groupsRepository = groupsRepository;
    }

    #endregion

    public async Task<Response<GroupVm>> Handle(GetGroupByIdQuery request, CancellationToken ct)
    {
        Group? group = await _groupsRepository.GetByIdAsync(request.Id, ct);
        if (group == null) return Response<GroupVm>.Error(ResponseCode.NotFound, "There is no group with this id");


        GroupVm GroupVm = new GroupVm();

        GroupVm.id = group.Id;
        GroupVm.Name = group.Name;
        GroupVm.Description = group.Description ?? "";

        return Response<GroupVm>.Ok(GroupVm);
    }
}
