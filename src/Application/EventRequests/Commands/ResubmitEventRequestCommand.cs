using System.Text.Json.Serialization;
using Application.Common;
using Domain.Services;
using MediatR;

namespace Application.EventRequests.Commands;

/// <summary>El professor propietari corregeix la sol·licitud i la torna a enviar a revisió.</summary>
public record ResubmitEventRequestCommand : EventRequestDataDto, IRequest<Response<long?>>
{
    [JsonIgnore]
    public long Id { get; private set; }
    public void SetId(long value) { Id = value; }
}

public class ResubmitEventRequestCommandValidator : EventRequestDataValidator<ResubmitEventRequestCommand> { }

public class ResubmitEventRequestCommandHandler : IRequestHandler<ResubmitEventRequestCommand, Response<long?>>
{
    #region IOC
    private readonly IEventRequestsRepository _eventRequestsRepository;
    private readonly ICurrentRequestService _currentRequestService;

    public ResubmitEventRequestCommandHandler(IEventRequestsRepository eventRequestsRepository, ICurrentRequestService currentRequestService)
    {
        _eventRequestsRepository = eventRequestsRepository;
        _currentRequestService = currentRequestService;
    }
    #endregion

    public async Task<Response<long?>> Handle(ResubmitEventRequestCommand request, CancellationToken ct)
    {
        if (_currentRequestService.UserId is not long userId)
        {
            return Response<long?>.Error(ResponseCode.BadRequest, EventRequestActions.NO_USER_MESSAGE);
        }

        return await EventRequestActions.RunAsync(_eventRequestsRepository, request.Id, userId,
            (r, now) => r.Resubmit(userId, request.ToDomain(), now),
            null, ct);
    }
}
