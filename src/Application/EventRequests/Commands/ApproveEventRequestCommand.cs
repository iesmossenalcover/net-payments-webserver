using Application.Common;
using Domain.Services;
using MediatR;

namespace Application.EventRequests.Commands;

/// <summary>L'admin aprova la sol·licitud: queda pendent de crear l'esdeveniment.</summary>
public record ApproveEventRequestCommand(long Id) : IRequest<Response<long?>>;

public class ApproveEventRequestCommandHandler : IRequestHandler<ApproveEventRequestCommand, Response<long?>>
{
    #region IOC
    private readonly IEventRequestsRepository _eventRequestsRepository;
    private readonly ICurrentRequestService _currentRequestService;

    public ApproveEventRequestCommandHandler(IEventRequestsRepository eventRequestsRepository, ICurrentRequestService currentRequestService)
    {
        _eventRequestsRepository = eventRequestsRepository;
        _currentRequestService = currentRequestService;
    }
    #endregion

    public async Task<Response<long?>> Handle(ApproveEventRequestCommand request, CancellationToken ct)
    {
        if (_currentRequestService.UserId is not long userId)
        {
            return Response<long?>.Error(ResponseCode.BadRequest, EventRequestActions.NO_USER_MESSAGE);
        }

        return await EventRequestActions.RunAsync(_eventRequestsRepository, request.Id, null,
            (r, now) => r.Approve(userId, now),
            null, ct);
    }
}
