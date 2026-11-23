using System.Text.Json.Serialization;
using Application.Common;
using Domain.Services;
using MediatR;

namespace Application.EventRequests.Commands;

/// <summary>L'admin edita la sol·licitud. Si estava pendent de creació, torna a revisió.</summary>
public record EditEventRequestCommand : EventRequestDataDto, IRequest<Response<long?>>
{
    [JsonIgnore]
    public long Id { get; private set; }
    public void SetId(long value) { Id = value; }
}

public class EditEventRequestCommandValidator : EventRequestDataValidator<EditEventRequestCommand> { }

public class EditEventRequestCommandHandler : IRequestHandler<EditEventRequestCommand, Response<long?>>
{
    #region IOC
    private readonly IEventRequestsRepository _eventRequestsRepository;
    private readonly ICurrentRequestService _currentRequestService;

    public EditEventRequestCommandHandler(IEventRequestsRepository eventRequestsRepository, ICurrentRequestService currentRequestService)
    {
        _eventRequestsRepository = eventRequestsRepository;
        _currentRequestService = currentRequestService;
    }
    #endregion

    public async Task<Response<long?>> Handle(EditEventRequestCommand request, CancellationToken ct)
    {
        if (_currentRequestService.UserId is not long userId)
        {
            return Response<long?>.Error(ResponseCode.BadRequest, EventRequestActions.NO_USER_MESSAGE);
        }

        return await EventRequestActions.RunAsync(_eventRequestsRepository, request.Id, null,
            (r, now) => r.EditByAdmin(userId, request.ToDomain(), now),
            null, ct);
    }
}
