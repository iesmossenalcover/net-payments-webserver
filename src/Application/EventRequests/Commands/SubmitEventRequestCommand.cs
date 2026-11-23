using Application.Common;
using Domain.Entities.EventRequests;
using Domain.Entities.People;
using Domain.Services;
using MediatR;

namespace Application.EventRequests.Commands;

/// <summary>El professor envia una sol·licitud nova: neix directament en PendingReview.</summary>
public record SubmitEventRequestCommand : EventRequestDataDto, IRequest<Response<long?>>;

public class SubmitEventRequestCommandValidator : EventRequestDataValidator<SubmitEventRequestCommand> { }

public class SubmitEventRequestCommandHandler : IRequestHandler<SubmitEventRequestCommand, Response<long?>>
{
    #region IOC
    private readonly IEventRequestsRepository _eventRequestsRepository;
    private readonly ICoursesRepository _coursesRepository;
    private readonly ICurrentRequestService _currentRequestService;
    private readonly EventRequestNotifier _notifier;

    public SubmitEventRequestCommandHandler(IEventRequestsRepository eventRequestsRepository, ICoursesRepository coursesRepository, ICurrentRequestService currentRequestService, EventRequestNotifier notifier)
    {
        _eventRequestsRepository = eventRequestsRepository;
        _coursesRepository = coursesRepository;
        _currentRequestService = currentRequestService;
        _notifier = notifier;
    }
    #endregion

    public async Task<Response<long?>> Handle(SubmitEventRequestCommand request, CancellationToken ct)
    {
        if (_currentRequestService.UserId is not long userId)
        {
            return Response<long?>.Error(ResponseCode.BadRequest, EventRequestActions.NO_USER_MESSAGE);
        }

        Course course = await _coursesRepository.GetCurrentCoursAsync(ct);
        var eventRequest = EventRequest.Submit(userId, course.Id, request.ToDomain(), DateTimeOffset.UtcNow);
        await _eventRequestsRepository.InsertAsync(eventRequest, CancellationToken.None);

        // Es recarrega per tenir el sol·licitant (nom i correu) per al correu.
        EventRequest? saved = await _eventRequestsRepository.GetWithDetailsAsync(eventRequest.Id, CancellationToken.None);
        if (saved != null) await _notifier.NotifySubmittedAsync(saved, CancellationToken.None);

        return Response<long?>.Ok(eventRequest.Id);
    }
}
