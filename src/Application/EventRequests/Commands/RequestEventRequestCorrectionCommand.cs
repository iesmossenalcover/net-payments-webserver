using Application.Common;
using Domain.Services;
using MediatR;

namespace Application.EventRequests.Commands;

/// <summary>L'admin demana al professor que corregeixi la sol·licitud (motiu obligatori).</summary>
public record RequestEventRequestCorrectionCommand(long Id, string Reason) : IRequest<Response<long?>>;

public class RequestEventRequestCorrectionCommandHandler : IRequestHandler<RequestEventRequestCorrectionCommand, Response<long?>>
{
    #region IOC
    private readonly IEventRequestsRepository _eventRequestsRepository;
    private readonly ICurrentRequestService _currentRequestService;
    private readonly EventRequestNotifier _notifier;

    public RequestEventRequestCorrectionCommandHandler(IEventRequestsRepository eventRequestsRepository, ICurrentRequestService currentRequestService, EventRequestNotifier notifier)
    {
        _eventRequestsRepository = eventRequestsRepository;
        _currentRequestService = currentRequestService;
        _notifier = notifier;
    }
    #endregion

    public async Task<Response<long?>> Handle(RequestEventRequestCorrectionCommand request, CancellationToken ct)
    {
        if (_currentRequestService.UserId is not long userId)
        {
            return Response<long?>.Error(ResponseCode.BadRequest, EventRequestActions.NO_USER_MESSAGE);
        }

        return await EventRequestActions.RunAsync(_eventRequestsRepository, request.Id, null,
            (r, now) => r.RequestCorrection(userId, request.Reason, now),
            r => _notifier.NotifyCorrectionRequestedAsync(r, request.Reason, CancellationToken.None), ct);
    }
}
