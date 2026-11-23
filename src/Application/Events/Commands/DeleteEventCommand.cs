using Application.Common;
using Domain.Services;
using Domain.Entities.Events;
using Domain.Entities.EventRequests;
using MediatR;

namespace Application.Events.Commands;

public record DeleteEventCommand(long Id) : IRequest<Response<long?>>;
public class DeleteEventCommandHandler : IRequestHandler<DeleteEventCommand, Response<long?>>
{
    #region IOC
    private readonly IEventsRespository _eventsRespository;
    private readonly IEventsPeopleRespository _eventsPeopleRespository;
    private readonly IEventRequestsRepository _eventRequestsRepository;
    private readonly ICurrentRequestService _currentRequestService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IGoogleAdminApi _googleAdminApi;
    private readonly string calendarId;

    public DeleteEventCommandHandler(IEventsRespository eventsRespository, IEventsPeopleRespository eventsPeopleRespository, IEventRequestsRepository eventRequestsRepository, ICurrentRequestService currentRequestService, IUnitOfWork unitOfWork, IGoogleAdminApi googleAdminApi, IConfiguration configuration)
    {
        _eventsRespository = eventsRespository;
        _eventsPeopleRespository = eventsPeopleRespository;
        _eventRequestsRepository = eventRequestsRepository;
        _currentRequestService = currentRequestService;
        _unitOfWork = unitOfWork;
        _googleAdminApi = googleAdminApi;
        calendarId = configuration.GetValue<string>("GoogleApiCalendarId") ?? throw new Exception("GoogleApiCalendarId");
    }
    #endregion

    public async Task<Response<long?>> Handle(DeleteEventCommand request, CancellationToken ct)
    {
        Event? e = await _eventsRespository.GetByIdAsync(request.Id, ct);
        if (e == null) return Response<long?>.Error(ResponseCode.BadRequest, "L'esdeveniment no existeix");

        // Els EventPerson s'esborren en cascada: si n'hi ha de pagats es perdria el registre del pagament.
        // L'admin ha de retornar els pagaments (desmarcar-los) abans de poder eliminar l'esdeveniment.
        if (await _eventsPeopleRespository.GetPaidPlacesByEventIdAsync(e.Id, ct) > 0)
        {
            return Response<long?>.Error(ResponseCode.BadRequest, "No es pot eliminar un esdeveniment amb pagaments. Primer s'han de retornar els pagaments.");
        }

        if (e.CalendarEventId != null)
        {
            var calendarResult = await _googleAdminApi.DeleteCalendarEvent(calendarId, e.CalendarEventId);
            if (!calendarResult.Success)
            {
                e.CalendarEventId = null;
                await _eventsRespository.UpdateAsync(e, ct);
                return Response<long?>.Error(ResponseCode.BadRequest, calendarResult.ErrorMessage ?? "No es pot eliminar l'esdeveniment del calendari de Google");
            }
        }

        try
        {
            await _unitOfWork.ExecuteInTransactionAsync(async _ =>
            {
                // Si l'esdeveniment venia d'una sol·licitud, aquesta torna a revisió (transició automàtica).
                EventRequest? eventRequest = await _eventRequestsRepository.GetByEventIdAsync(e.Id, CancellationToken.None);
                if (eventRequest != null)
                {
                    eventRequest.OnEventDeleted(_currentRequestService.UserId, DateTimeOffset.UtcNow);
                    if (!await _eventRequestsRepository.SaveAsync(CancellationToken.None))
                    {
                        throw new Exception("La sol·licitud de l'esdeveniment ha canviat d'estat mentre s'eliminava");
                    }
                }

                await _eventsRespository.DeleteAsync(e, CancellationToken.None);
            }, CancellationToken.None);
        }
        catch (Exception)
        {
            return Response<long?>.Error(ResponseCode.BadRequest, "No es pot eliminar l'esdeveniment.");
        }

        return Response<long?>.Ok(e.Id);
    }


}