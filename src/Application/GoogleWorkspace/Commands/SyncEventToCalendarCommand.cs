using Application.Common;
using Application.Common.Models;
using Domain.Services;
using Domain.Entities.GoogleApi;
using Domain.Entities.People;
using MediatR;
using Domain.Entities.Events;

namespace Application.GoogleWorkspace.Commands;

// Model we receive
public record SyncEventToCalendarCommand(long Id) : IRequest<Response<SyncEventToCalendarCommandVm>>;

// Validator for the model

// Optionally define a view model
public record SyncEventToCalendarCommandVm(string EventId);

// Handler
public class SyncEventToCalendarCommandHandler : IRequestHandler<SyncEventToCalendarCommand, Response<SyncEventToCalendarCommandVm>>
{
    #region props

    private readonly IGoogleAdminApi _googleAdminApi;
    private readonly IEventsRespository _eventsRepository;
    private readonly string calendarId;
    private readonly string frontEventSummaryUrl;

    public SyncEventToCalendarCommandHandler(IGoogleAdminApi googleAdminApi, IEventsRespository eventsRespository, IConfiguration configuration)
    {
        _googleAdminApi = googleAdminApi;
        _eventsRepository = eventsRespository;
        calendarId = configuration.GetValue<string>("GoogleApiCalendarId") ?? throw new Exception("GoogleApiCalendarId");
        frontEventSummaryUrl = configuration.GetValue<string>("FrontEventSummaryUrl") ?? throw new Exception("FrontEventSummaryUrl");
    }
    #endregion

    public async Task<Response<SyncEventToCalendarCommandVm>> Handle(SyncEventToCalendarCommand request, CancellationToken ct)
    {

        Event? e = await _eventsRepository.GetByIdAsync(request.Id, ct);
        if (e == null) return Response<SyncEventToCalendarCommandVm>.Error(ResponseCode.NotFound, "No s'ha trobat l'event");

        if (e.CalendarEventId == null)
        {
            // Create
            var summaryUrl = frontEventSummaryUrl.Replace("{code}", e.Code);
            var result = await _googleAdminApi.CreateCalendarEvent(
                calendarId,
                e.Name,
                summaryUrl,
                e.Date,
                e.EndDate ?? e.Date
            );
            if (!result.Success || result.Data == null) return Response<SyncEventToCalendarCommandVm>.Error(ResponseCode.BadRequest, result.ErrorMessage ?? "Error creant l'event al calendari");

            e.CalendarEventId = result.Data;
            await _eventsRepository.UpdateAsync(e, ct);
        }
        else
        {
            // Update
            var summaryUrl = frontEventSummaryUrl.Replace("{code}", e.Code);
            var result = await _googleAdminApi.UpdateCalendarEvent(
                calendarId,
                e.CalendarEventId,
                e.Name,
                summaryUrl,
                e.Date,
                e.EndDate ?? e.Date
            );
            if (!result.Success)
            {
                // TODO: Return enum from the API to know if the error is because the event was not found, and in that case, remove the calendarEventId from the event
                if (result.ErrorMessage == "not_found")
                {
                    Console.WriteLine($"Event with id {e.CalendarEventId} not found in calendar, removing calendarEventId from event");
                    e.CalendarEventId = null;
                    await _eventsRepository.UpdateAsync(e, ct);
                }
                return Response<SyncEventToCalendarCommandVm>.Error(ResponseCode.BadRequest, result.ErrorMessage ?? "Error creant l'event al calendari");
            }
        }
        return Response<SyncEventToCalendarCommandVm>.Ok(new SyncEventToCalendarCommandVm(e.CalendarEventId));
    }
}
