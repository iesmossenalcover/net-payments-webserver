using Domain.Entities.EventRequests;
using Domain.Services;
using MediatR;

namespace Application.EventRequests.Queries;

/// <summary>Totes les sol·licituds (admin), opcionalment filtrades per estat. Les eliminades no hi surten.</summary>
public record ListEventRequestsQuery(EventRequestStatus? Status) : IRequest<IEnumerable<EventRequestSummaryVm>>;

public class ListEventRequestsQueryHandler : IRequestHandler<ListEventRequestsQuery, IEnumerable<EventRequestSummaryVm>>
{
    #region IOC
    private readonly IEventRequestsRepository _eventRequestsRepository;

    public ListEventRequestsQueryHandler(IEventRequestsRepository eventRequestsRepository)
    {
        _eventRequestsRepository = eventRequestsRepository;
    }
    #endregion

    public async Task<IEnumerable<EventRequestSummaryVm>> Handle(ListEventRequestsQuery request, CancellationToken ct)
    {
        var requests = await _eventRequestsRepository.GetAllAsync(request.Status, ct);
        return requests.Select(EventRequestMapper.ToSummaryVm);
    }
}
