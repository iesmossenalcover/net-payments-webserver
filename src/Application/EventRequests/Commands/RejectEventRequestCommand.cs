using Application.Common;
using Domain.Services;
using MediatR;

namespace Application.EventRequests.Commands;

/// <summary>L'admin rebutja la sol·licitud (motiu obligatori). És un esborrat lògic.</summary>
public record RejectEventRequestCommand(long Id, string Reason) : IRequest<Response<long?>>;

public class RejectEventRequestCommandHandler : IRequestHandler<RejectEventRequestCommand, Response<long?>>
{
    #region IOC
    private readonly IEventRequestsRepository _eventRequestsRepository;
    private readonly ICurrentRequestService _currentRequestService;
    private readonly EventRequestNotifier _notifier;

    public RejectEventRequestCommandHandler(IEventRequestsRepository eventRequestsRepository, ICurrentRequestService currentRequestService, EventRequestNotifier notifier)
    {
        _eventRequestsRepository = eventRequestsRepository;
        _currentRequestService = currentRequestService;
        _notifier = notifier;
    }
    #endregion

    public async Task<Response<long?>> Handle(RejectEventRequestCommand request, CancellationToken ct)
    {
        if (_currentRequestService.UserId is not long userId)
        {
            return Response<long?>.Error(ResponseCode.BadRequest, EventRequestActions.NO_USER_MESSAGE);
        }

        return await EventRequestActions.RunAsync(_eventRequestsRepository, request.Id, null,
            (r, now) => r.Reject(userId, request.Reason, now),
            r => _notifier.NotifyRejectedAsync(r, request.Reason, CancellationToken.None), ct);
    }
}
