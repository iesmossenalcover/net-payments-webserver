using Application.Common;
using Domain.Services;
using FluentValidation;
using MediatR;

namespace Application.EventRequests.Commands;

/// <summary>
/// L'admin aprova la sol·licitud: queda pendent de crear l'esdeveniment. Indica fins quan el professor
/// hi pot fer canvis (si en fa, torna a revisió) i el professor en rep un correu.
/// </summary>
public record ApproveEventRequestCommand(long Id, DateTime ChangesDeadline) : IRequest<Response<long?>>;

public class ApproveEventRequestCommandValidator : AbstractValidator<ApproveEventRequestCommand>
{
    public ApproveEventRequestCommandValidator()
    {
        RuleFor(x => x.ChangesDeadline).NotEmpty().WithMessage("S'ha d'indicar fins quan el professor pot fer canvis.");
    }
}

public class ApproveEventRequestCommandHandler : IRequestHandler<ApproveEventRequestCommand, Response<long?>>
{
    #region IOC
    private readonly IEventRequestsRepository _eventRequestsRepository;
    private readonly ICurrentRequestService _currentRequestService;
    private readonly EventRequestNotifier _notifier;

    public ApproveEventRequestCommandHandler(IEventRequestsRepository eventRequestsRepository, ICurrentRequestService currentRequestService, EventRequestNotifier notifier)
    {
        _eventRequestsRepository = eventRequestsRepository;
        _currentRequestService = currentRequestService;
        _notifier = notifier;
    }
    #endregion

    public async Task<Response<long?>> Handle(ApproveEventRequestCommand request, CancellationToken ct)
    {
        if (_currentRequestService.UserId is not long userId)
        {
            return Response<long?>.Error(ResponseCode.BadRequest, EventRequestActions.NO_USER_MESSAGE);
        }

        var deadline = new DateTimeOffset(request.ChangesDeadline.ToUniversalTime(), TimeSpan.Zero);
        return await EventRequestActions.RunAsync(_eventRequestsRepository, request.Id, null,
            (r, now) => r.Approve(userId, deadline, now),
            r => _notifier.NotifyApprovedAsync(r, CancellationToken.None), ct);
    }
}
