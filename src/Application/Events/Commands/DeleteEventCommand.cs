using Application.Common;
using Application.Common.Services;
using Domain.Entities.Events;
using MediatR;

namespace Application.Events.Commands;

public record DeleteEventCommand(long Id) : IRequest<Unit>;
public class DeleteEventCommandHandler : IRequestHandler<DeleteEventCommand, Unit>
{
    #region IOC
    private readonly IEventsRespository _eventsRespository;

    public DeleteEventCommandHandler(IEventsRespository eventsRespository)
    {
        _eventsRespository = eventsRespository;
    }
    #endregion

    public async Task<Unit> Handle(DeleteEventCommand request, CancellationToken ct)
    {
        Event? e = await _eventsRespository.GetByIdAsync(request.Id, ct);
        if (e == null) return Unit.Value;

        await _eventsRespository.DeleteAsync(e, CancellationToken.None);

        return Unit.Value;
    }


}