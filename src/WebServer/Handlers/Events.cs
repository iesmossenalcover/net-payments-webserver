using Application.Common;
using Application.Events.Commands;
using Application.Events.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace WebServer.Handlers;

public class Events
{
    public static async Task<Response<EventVm>> GetEvent(IMediator mediator, long id)
    {
        return await mediator.Send(new GetEventByIdQuery(id));
    }

    public static async Task<Response<long?>> CreateEvent(IMediator mediator, [FromBody]CreateEventCommand cmd)
    {
        return await mediator.Send(cmd);
    }

    public static async Task<Response<long?>> UpdateEvent(IMediator mediator, long id, [FromBody] UpdateEventCommand cmd)
    {
        cmd.Id = id;
        return await mediator.Send(cmd);
    }
}