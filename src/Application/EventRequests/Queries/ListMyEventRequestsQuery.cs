using Domain.Services;
using MediatR;

namespace Application.EventRequests.Queries;

/// <summary>Sol·licituds de l'usuari actual (les eliminades no hi surten).</summary>
public record ListMyEventRequestsQuery : IRequest<IEnumerable<EventRequestSummaryVm>>;

public class ListMyEventRequestsQueryHandler : IRequestHandler<ListMyEventRequestsQuery, IEnumerable<EventRequestSummaryVm>>
{
    #region IOC
    private readonly IEventRequestsRepository _eventRequestsRepository;
    private readonly ICurrentRequestService _currentRequestService;

    public ListMyEventRequestsQueryHandler(IEventRequestsRepository eventRequestsRepository, ICurrentRequestService currentRequestService)
    {
        _eventRequestsRepository = eventRequestsRepository;
        _currentRequestService = currentRequestService;
    }
    #endregion

    public async Task<IEnumerable<EventRequestSummaryVm>> Handle(ListMyEventRequestsQuery request, CancellationToken ct)
    {
        if (_currentRequestService.UserId is not long userId) return Enumerable.Empty<EventRequestSummaryVm>();

        var requests = await _eventRequestsRepository.GetAllByRequesterAsync(userId, ct);
        return requests.Select(EventRequestMapper.ToSummaryVm);
    }
}
