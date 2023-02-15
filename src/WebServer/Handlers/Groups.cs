using Application.Common.Models;
using Application.Groups.Queries;
using MediatR;

namespace WebServer.Handlers;

public class Groups
{
    public static async Task<SelectorVm> GetGroupsSelector(IMediator mediator)
    {
        return await mediator.Send(new GetAllGroupsSelectorQuery());
    }
}