using System.Net.Http.Headers;
using System.Net.Mime;
using System.Text;
using Application.Common;
using Application.Events.Commands;
using Application.Events.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace WebServer.Handlers;

public class Events
{
    public static async Task<IEnumerable<EventVm>> ListCourseEvents(IMediator mediator)
    {
        return await mediator.Send(new ListEventsQuery());
    }

    public static async Task<Response<PersonActiveEventsVm>> ListActivePersonEvents(IMediator mediator, [FromBody] PersonActiveEventsQuery query)
    {
        return await mediator.Send(query);
    }

    public static async Task<Response<EventVm>> GetEvent(IMediator mediator, long id)
    {
        return await mediator.Send(new GetEventByIdQuery(id));
    }

    public static async Task<Response<string?>> CreateEvent(IMediator mediator, [FromBody] CreateEventCommand cmd)
    {
        return await mediator.Send(cmd);
    }

    public static async Task<Response<long?>> UpdateEvent(IMediator mediator, long id, [FromBody] UpdateEventCommand cmd)
    {
        cmd.SetId(id);
        return await mediator.Send(cmd);
    }

    public static async Task<Response<long?>> DeleteEvent(IMediator mediator, long id)
    {
        return await mediator.Send(new DeleteEventCommand(id));
    }

    public static async Task<Response<EventPeopleVm>> GetPeopleEvent(IMediator mediator, string eventCode)
    {
        return await mediator.Send(new EventPeopleQuery(eventCode));
    }

    public static async Task<Response<bool?>> SetPeopleToEvent(
        IMediator mediator,
        string eventCode,
        [FromBody] SetPeopleEventCommand cmd)
    {
        cmd.EventCode = eventCode;
        return await mediator.Send(cmd);
    }

    public static async Task<Response<ListEventPaymentsVm>> ListEventPayments(IMediator mediator, string eventCode)
    {
        return await mediator.Send(new ListEventPaymentsQuery(eventCode));
    }

    public static async Task<Response<ListEventSummaryVm>> ListEventSummary(IMediator mediator, string eventCode)
    {
        return await mediator.Send(new ListEventSummaryQuery(eventCode));
    }

    public static async Task<Response<bool>> SetPersonEventPaid(
        IMediator mediator,
        long eventPersonId,
        [FromBody] SetPersonEventPaidCommand cmd)
    {
        cmd.SetId(eventPersonId);
        return await mediator.Send(cmd);
    }

    public static async Task<IResult> ExportEvents(HttpContext ctx, IMediator mediator, [FromQuery] long? courseId)
    {
        var response = await mediator.Send(new ExportEventsInfoQuery(courseId));
        // Convert the CSV content to a byte array
        byte[] byteArray = response.Stream.ToArray();
        return Results.File(byteArray, response.FileType, response.FileName);
    }
}