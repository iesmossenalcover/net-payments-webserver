using Application.Common;
using Application.Common.Services;
using Domain.Entities.Events;
using MediatR;

namespace Application.Events.Commands;

public record DeleteEventCommand(long Id) : IRequest<Response<long?>>;
public class DeleteEventCommandHandler : IRequestHandler<DeleteEventCommand, Response<long?>>
{
    #region IOC
    private readonly IEventsRespository _eventsRespository;

    public DeleteEventCommandHandler(IEventsRespository eventsRespository)
    {
        _eventsRespository = eventsRespository;
    }
    #endregion

    public async Task<Response<long?>> Handle(DeleteEventCommand request, CancellationToken ct)
    {
        Event? e = await _eventsRespository.GetByIdAsync(request.Id, ct);
        if (e == null) return Response<long?>.Error(ResponseCode.BadRequest, "L'esdeveniment no existeix");

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