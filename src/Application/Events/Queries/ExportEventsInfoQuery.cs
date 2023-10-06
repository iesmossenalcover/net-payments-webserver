using Application.Common.Models;
using Domain.Services;
using Domain.Entities.Events;
using Domain.Entities.People;
using MediatR;

namespace Application.Events.Queries;

public record ExportEventsInfoQuery(long? CourseId) : IRequest<FileVm>;

public class ExportEventsInfoQueryHandler : IRequestHandler<ExportEventsInfoQuery, FileVm>
{
    #region IOC

    private readonly ICsvParser _csvParser;
    private readonly IEventsRespository _eventsRepository;
    private readonly IEventsPeopleRespository _eventsPeopleRepository;
    private readonly ICoursesRepository _coursesRepository;
    private readonly IPersonGroupCourseRepository _personGroupRepository;

    public ExportEventsInfoQueryHandler(ICsvParser csvParser,
        IEventsRespository eventsRepository,
        IEventsPeopleRespository eventsPeopleRepository,
        ICoursesRepository coursesRepository,
        IPersonGroupCourseRepository personGroupRepository)
    {
        _csvParser = csvParser;
        _eventsRepository = eventsRepository;
        _eventsPeopleRepository = eventsPeopleRepository;
        _coursesRepository = coursesRepository;
        _personGroupRepository = personGroupRepository;
    }

    #endregion

    public async Task<FileVm> Handle(ExportEventsInfoQuery request, CancellationToken ct)
    {
        Course c = request.CourseId.HasValue
            ? await _coursesRepository.GetByIdAsync(request.CourseId.Value, true, ct) ??
              throw new Exception("Course Not found")
            : await _coursesRepository.GetCurrentCoursAsync(ct);

        IEnumerable<Event> events = await _eventsRepository.GetAllEventsByCourseIdAsync(c.Id, ct);
        IEnumerable<EventPerson> allEventsPeople = await _eventsPeopleRepository.GetAllByCourseId(c.Id, ct);
        IDictionary<long, PersonGroupCourse> pgcs =
            (await _personGroupRepository.GetCurrentCourseGroupByPeopleIdsAsync(
                allEventsPeople.Select(x => x.PersonId).Distinct(), ct)).ToDictionary(x => x.PersonId, x => x);

        List<EventRow> rows = new List<EventRow>(events.Count());
        foreach (Event e in events)
        {
            IEnumerable<EventPerson> eventPeople = allEventsPeople.Where(x => x.EventId == e.Id);
            var amipaPayments = eventPeople.Where(x => x.Paid && x.PaidAsAmipa);
            var noAmipaPayments = eventPeople.Where(x => x.Paid && !x.PaidAsAmipa);
            var eventPgcs = eventPeople.Select(x => pgcs[x.PersonId]);
            rows.Add(new EventRow()
            {
                Title = e.Name,
                AmipaPeople = amipaPayments.Count(),
                Amount = noAmipaPayments.Count() * e.Price + amipaPayments.Count() * e.AmipaPrice,
                Date = e.Date.ToString("dd/MM/yyyy"),
                Group = string.Join(", ", eventPgcs.Select(x => x.Group.Name).Distinct()),
            });
        }

        var memStream = new MemoryStream();
        var streamWriter = new StreamWriter(memStream);
        await _csvParser.WriteToStreamAsync(streamWriter, rows);

        return new FileVm(memStream, "text/csv", "export.csv");
    }
}

public class EventRow
{
    public string Title { get; set; } = string.Empty;
    public string Group { get; set; } = string.Empty;
    public string Date { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public int AmipaPeople { get; set; }
}