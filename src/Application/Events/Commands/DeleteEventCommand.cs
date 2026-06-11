using Application.Common;
using Domain.Services;
using Domain.Entities.Events;
using MediatR;

namespace Application.Events.Commands;

public record DeleteEventCommand(long Id) : IRequest<Response<long?>>;
public class DeleteEventCommandHandler : IRequestHandler<DeleteEventCommand, Response<long?>>
{
    #region IOC
    private readonly IEventsRespository _eventsRespository;
    private readonly IGoogleAdminApi _googleAdminApi;
    private readonly string calendarId;

    public DeleteEventCommandHandler(IEventsRespository eventsRespository, IGoogleAdminApi googleAdminApi, IConfiguration configuration)
    {
        _eventsRespository = eventsRespository;
        _googleAdminApi = googleAdminApi;
        calendarId = configuration.GetValue<string>("GoogleApiCalendarId") ?? throw new Exception("GoogleApiCalendarId");
    }
    #endregion

    public async Task<Response<long?>> Handle(DeleteEventCommand request, CancellationToken ct)
    {
        Event? e = await _eventsRespository.GetByIdAsync(request.Id, ct);
        if (e == null) return Response<long?>.Error(ResponseCode.BadRequest, "L'esdeveniment no existeix");

        if (e.CalendarEventId != null)
        {
            var calendarResult = await _googleAdminApi.DeleteCalendarEvent(calendarId, e.CalendarEventId);
            if (!calendarResult.Success)
            {
                e.CalendarEventId = null;
                await _eventsRespository.UpdateAsync(e, ct);
                return Response<long?>.Error(ResponseCode.BadRequest, calendarResult.ErrorMessage ?? "No es pot eliminar l'event del calendari de Google");
            }
        }

        try
        {
            await _eventsRespository.DeleteAsync(e, CancellationToken.None);

        }
        catch (Exception)
        {
            return Response<long?>.Error(ResponseCode.BadRequest, "No es pot eliminar l'esdeveniment.");
        }

        return Response<long?>.Ok(e.Id);
    }


}