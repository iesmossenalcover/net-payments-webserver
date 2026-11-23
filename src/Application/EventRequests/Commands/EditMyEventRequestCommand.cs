using System.Text.Json.Serialization;
using Application.Common;
using Domain.Entities.EventRequests;
using Domain.Services;
using MediatR;

namespace Application.EventRequests.Commands;

/// <summary>
/// El professor propietari edita la seva sol·licitud: la corregeix quan l'admin ho ha demanat, o hi fa
/// canvis quan està aprovada i encara és dins del termini. En tots dos casos torna a revisió.
/// </summary>
public record EditMyEventRequestCommand : EventRequestDataDto, IRequest<Response<long?>>
{
    [JsonIgnore]
    public long Id { get; private set; }
    public void SetId(long value) { Id = value; }
}

public class EditMyEventRequestCommandValidator : EventRequestDataValidator<EditMyEventRequestCommand> { }

public class EditMyEventRequestCommandHandler : IRequestHandler<EditMyEventRequestCommand, Response<long?>>
{
    #region IOC
    private readonly IEventRequestsRepository _eventRequestsRepository;
    private readonly ICurrentRequestService _currentRequestService;
    private readonly EventRequestNotifier _notifier;

    public EditMyEventRequestCommandHandler(IEventRequestsRepository eventRequestsRepository, ICurrentRequestService currentRequestService, EventRequestNotifier notifier)
    {
        _eventRequestsRepository = eventRequestsRepository;
        _currentRequestService = currentRequestService;
        _notifier = notifier;
    }
    #endregion

    public async Task<Response<long?>> Handle(EditMyEventRequestCommand request, CancellationToken ct)
    {
        if (_currentRequestService.UserId is not long userId)
        {
            return Response<long?>.Error(ResponseCode.BadRequest, EventRequestActions.NO_USER_MESSAGE);
        }

        EventRequestStatus previous = default;
        return await EventRequestActions.RunAsync(_eventRequestsRepository, request.Id, userId,
            (r, now) =>
            {
                previous = r.Status;
                r.EditByRequester(userId, request.ToDomain(), now);
            },
            r => previous == EventRequestStatus.PendingCreation
                ? _notifier.NotifyEditedAfterApprovalAsync(r, CancellationToken.None)
                : _notifier.NotifyResubmittedAsync(r, CancellationToken.None),
            ct);
    }
}
