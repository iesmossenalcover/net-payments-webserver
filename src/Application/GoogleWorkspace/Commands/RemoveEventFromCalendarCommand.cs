using Application.Common;
using Domain.Services;
using MediatR;
using Domain.Entities.Events;

namespace Application.GoogleWorkspace.Commands;

// Model we receive
public record RemoveEventFromCalendarCommand(long Id) : IRequest<Response<string?>>;

// Handler
public class RemoveEventFromCalendarCommandHandler : IRequestHandler<RemoveEventFromCalendarCommand, Response<string?>>
{
    #region props

    private readonly IGoogleAdminApi _googleAdminApi;
    private readonly IEventsRespository _eventsRepository;
    private readonly string calendarId;

    public RemoveEventFromCalendarCommandHandler(IGoogleAdminApi googleAdminApi, IEventsRespository eventsRespository, IConfiguration configuration)
    {
        _googleAdminApi = googleAdminApi;
        _eventsRepository = eventsRespository;
        calendarId = configuration.GetValue<string>("GoogleApiCalendarId") ?? throw new Exception("GoogleApiCalendarId");
    }
    #endregion

    public async Task<Response<string?>> Handle(RemoveEventFromCalendarCommand request, CancellationToken ct)
    {
        Event? e = await _eventsRepository.GetByIdAsync(request.Id, ct);
        if (e == null) return Response<string?>.Error(ResponseCode.NotFound, "No s'ha trobat l'event");

        if (e.CalendarEventId == null) return Response<string?>.Ok(null);

        string calendarEventId = e.CalendarEventId;

        var result = await _googleAdminApi.DeleteCalendarEvent(calendarId, e.CalendarEventId);
        if (!result.Success)
        {
            return Response<string?>.Error(ResponseCode.BadRequest, result.ErrorMessage ?? "No es pot eliminar l'event del calendari de Google");
        }

        e.CalendarEventId = null;
        await _eventsRepository.UpdateAsync(e, ct);

        return Response<string?>.Ok(calendarEventId);
    }
}
