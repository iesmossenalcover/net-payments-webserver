using Application.Common;
using Application.EventRequests;
using Application.EventRequests.Commands;
using Application.EventRequests.Queries;
using Application.Events.Commands;
using Domain.Entities.EventRequests;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace WebServer.Handlers;

public class EventRequests
{
    #region Professor (només les seves)

    public static async Task<IEnumerable<EventRequestSummaryVm>> ListMine(IMediator mediator)
    {
        return await mediator.Send(new ListMyEventRequestsQuery());
    }

    public static async Task<Response<EventRequestVm>> GetMine(IMediator mediator, long id)
    {
        return await mediator.Send(new GetEventRequestQuery(id, OnlyMine: true));
    }

    public static async Task<Response<long?>> Submit(IMediator mediator, [FromBody] SubmitEventRequestCommand cmd)
    {
        return await mediator.Send(cmd);
    }

    public static async Task<IEnumerable<CalendarItemVm>> Calendar(IMediator mediator, [FromQuery] DateTime from, [FromQuery] DateTime to)
    {
        return await mediator.Send(new GetEventCalendarQuery(from, to));
    }

    public static async Task<IEnumerable<GroupPersonVm>> GroupPeople(IMediator mediator, long groupId)
    {
        return await mediator.Send(new GetGroupPeopleQuery(groupId));
    }

    public static async Task<Response<long?>> EditMine(IMediator mediator, long id, [FromBody] EditMyEventRequestCommand cmd)
    {
        cmd.SetId(id);
        return await mediator.Send(cmd);
    }

    #endregion

    #region Admin

    public static async Task<IEnumerable<EventRequestSummaryVm>> List(IMediator mediator, [FromQuery] EventRequestStatus? status = null)
    {
        return await mediator.Send(new ListEventRequestsQuery(status));
    }

    public static async Task<Response<EventRequestVm>> Get(IMediator mediator, long id)
    {
        return await mediator.Send(new GetEventRequestQuery(id, OnlyMine: false));
    }

    public static async Task<Response<long?>> Edit(IMediator mediator, long id, [FromBody] EditEventRequestCommand cmd)
    {
        cmd.SetId(id);
        return await mediator.Send(cmd);
    }

    public static async Task<Response<long?>> Approve(IMediator mediator, long id, [FromBody] ApproveBody body)
    {
        return await mediator.Send(new ApproveEventRequestCommand(id, body.ChangesDeadline, body.SyncToCalendar));
    }

    public static async Task<Response<long?>> RequestCorrection(IMediator mediator, long id, [FromBody] ReasonBody body)
    {
        return await mediator.Send(new RequestEventRequestCorrectionCommand(id, body.Reason));
    }

    public static async Task<Response<long?>> Reject(IMediator mediator, long id, [FromBody] ReasonBody body)
    {
        return await mediator.Send(new RejectEventRequestCommand(id, body.Reason));
    }

    public static async Task<Response<long?>> Delete(IMediator mediator, long id)
    {
        return await mediator.Send(new DeleteEventRequestCommand(id));
    }

    public static async Task<Response<CreateEventFromRequestVm?>> CreateEvent(IMediator mediator, long id, [FromBody] CreateEventCommand cmd)
    {
        return await mediator.Send(new CreateEventFromRequestCommand(id, cmd));
    }

    #endregion
}
