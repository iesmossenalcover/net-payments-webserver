using Domain.Entities.EventRequests;
using Domain.Services;
using MediatR;

namespace Application.EventRequests.Queries;

/// <summary>
/// Totes les sol·licituds d'un curs (admin), opcionalment filtrades per estat. Sense curs, les del curs actual.
/// Les eliminades no hi surten.
/// </summary>
public record ListEventRequestsQuery(EventRequestStatus? Status, long? CourseId) : IRequest<IEnumerable<EventRequestSummaryVm>>;

public class ListEventRequestsQueryHandler : IRequestHandler<ListEventRequestsQuery, IEnumerable<EventRequestSummaryVm>>
{
    #region IOC
    private readonly IEventRequestsRepository _eventRequestsRepository;
    private readonly ICoursesRepository _coursesRepository;

    public ListEventRequestsQueryHandler(IEventRequestsRepository eventRequestsRepository, ICoursesRepository coursesRepository)
    {
        _eventRequestsRepository = eventRequestsRepository;
        _coursesRepository = coursesRepository;
    }
    #endregion

    public async Task<IEnumerable<EventRequestSummaryVm>> Handle(ListEventRequestsQuery request, CancellationToken ct)
    {
        long courseId = request.CourseId ?? (await _coursesRepository.GetCurrentCoursAsync(ct)).Id;
        var requests = await _eventRequestsRepository.GetAllAsync(request.Status, courseId, ct);
        return requests.Select(EventRequestMapper.ToSummaryVm);
    }
}
