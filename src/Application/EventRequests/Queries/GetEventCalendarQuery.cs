using Domain.Entities.EventRequests;
using Domain.Services;
using MediatR;

namespace Application.EventRequests.Queries;

public enum CalendarItemKind
{
    Event = 0,
    Request = 1,
}

/// <summary>Un element del calendari: un esdeveniment o una sol·licitud en curs.</summary>
public record CalendarItemVm(
    CalendarItemKind Kind, long Id, string Title,
    DateTimeOffset Start, DateTimeOffset? End,
    EventRequestStatus? Status, string? EventCode);

/// <summary>
/// Esdeveniments i sol·licituds en curs d'un rang de dates, per ajudar a triar dates al formulari
/// de sol·licitud. Ho pot veure qualsevol professor (reader), per això només retorna títol i dates.
/// </summary>
public record GetEventCalendarQuery(DateTime From, DateTime To) : IRequest<IEnumerable<CalendarItemVm>>;

public class GetEventCalendarQueryHandler : IRequestHandler<GetEventCalendarQuery, IEnumerable<CalendarItemVm>>
{
    // Un mes de calendari són com a molt 6 setmanes: amb això n'hi ha prou i evita consultes enormes.
    private static readonly TimeSpan MAX_RANGE = TimeSpan.FromDays(62);

    #region IOC
    private readonly IEventsRespository _eventsRepository;
    private readonly IEventRequestsRepository _eventRequestsRepository;

    public GetEventCalendarQueryHandler(IEventsRespository eventsRepository, IEventRequestsRepository eventRequestsRepository)
    {
        _eventsRepository = eventsRepository;
        _eventRequestsRepository = eventRequestsRepository;
    }
    #endregion

    public async Task<IEnumerable<CalendarItemVm>> Handle(GetEventCalendarQuery request, CancellationToken ct)
    {
        var from = new DateTimeOffset(request.From.ToUniversalTime(), TimeSpan.Zero);
        var to = new DateTimeOffset(request.To.ToUniversalTime(), TimeSpan.Zero);
        if (to < from) return Enumerable.Empty<CalendarItemVm>();
        if (to - from > MAX_RANGE) to = from + MAX_RANGE;

        var events = await _eventsRepository.GetEventsOverlappingAsync(from, to, ct);
        var requests = await _eventRequestsRepository.GetOpenOverlappingAsync(from, to, ct);

        return events
            .Select(e => new CalendarItemVm(CalendarItemKind.Event, e.Id, e.Name, e.Date, e.EndDate, null, e.Code))
            .Concat(requests.Select(r => new CalendarItemVm(CalendarItemKind.Request, r.Id, r.Title, r.StartDate, r.EndDate, r.Status, null)))
            .OrderBy(x => x.Start)
            .ToList();
    }
}
