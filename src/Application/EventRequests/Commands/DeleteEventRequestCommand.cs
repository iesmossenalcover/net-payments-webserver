using Application.Common;
using Domain.Services;
using MediatR;

namespace Application.EventRequests.Commands;

/// <summary>L'admin elimina la sol·licitud (esborrat lògic).</summary>
public record DeleteEventRequestCommand(long Id) : IRequest<Response<long?>>;

public class DeleteEventRequestCommandHandler : IRequestHandler<DeleteEventRequestCommand, Response<long?>>
{
    #region IOC
    private readonly IEventRequestsRepository _eventRequestsRepository;
    private readonly ICurrentRequestService _currentRequestService;

    public DeleteEventRequestCommandHandler(IEventRequestsRepository eventRequestsRepository, ICurrentRequestService currentRequestService)
    {
        _eventRequestsRepository = eventRequestsRepository;
        _currentRequestService = currentRequestService;
    }
    #endregion

    public async Task<Response<long?>> Handle(DeleteEventRequestCommand request, CancellationToken ct)
    {
        if (_currentRequestService.UserId is not long userId)
        {
            return Response<long?>.Error(ResponseCode.BadRequest, EventRequestActions.NO_USER_MESSAGE);
        }

        return await EventRequestActions.RunAsync(_eventRequestsRepository, request.Id, null,
            (r, now) => r.Delete(userId, now),
            null, ct);
    }
}
