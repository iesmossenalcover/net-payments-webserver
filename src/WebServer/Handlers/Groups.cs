using Application.Common;
using Application.Common.Models;
using Application.Groups.Commands;
using Application.Groups.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace WebServer.Handlers;

public class Groups
{
    public static async Task<SelectorVm> GetGroupsSelector(IMediator mediator)
    {
        return await mediator.Send(new GetAllGroupsSelectorQuery());
    }

    public static async Task<IEnumerable<GroupRowVm>> ListGroups(
        IMediator mediator)
    {
        return await mediator.Send(new ListGroupsQuery());
    }


    public static async Task<Response<GroupVm>> GetGroup(
        long id,
        IMediator mediator)
    {
        return await mediator.Send(new GetGroupByIdQuery(id));
    }

    public static async Task<Response<long?>> CreateGroup(
        IMediator mediator,
        [FromBody] CreateGroupCommand cmd)
    {
        return await mediator.Send(cmd);
    }

    public static async Task<Response<long?>> UpdateGroup(
    long id,
    IMediator mediator,
    [FromBody] UpdateGroupCommand cmd)
    {
        cmd.Id = id;
        return await mediator.Send(cmd);
    }

}