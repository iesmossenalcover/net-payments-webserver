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
    private readonly Domain.Services.ICsvParser _csvParser;
    private readonly Domain.Services.IEventsRespository _eventsRepository;
    private readonly Domain.Services.IEventsPeopleRespository _eventsPeopleRespository;
    private readonly Domain.Services.ICoursesRepository _coursesRepository;
    private readonly Domain.Services.IPersonGroupCourseRepository _personGroupRespository;

    public ExportEventsInfoQueryHandler(ICsvParser csvParser, IEventsRespository eventsRepository, IEventsPeopleRespository eventsPeopleRespository, ICoursesRepository coursesRepository, IPersonGroupCourseRepository personGroupRespository)
    {
        _csvParser = csvParser;
        _eventsRepository = eventsRepository;
        _eventsPeopleRespository = eventsPeopleRespository;
        _coursesRepository = coursesRepository;
        _personGroupRespository = personGroupRespository;
    }
    #endregion

    public async Task<FileVm> Handle(ExportEventsInfoQuery request, CancellationToken ct)
    {
        Course c = request.CourseId.HasValue ?
                    await _coursesRepository.GetByIdAsync(request.CourseId.Value, ct) ?? throw new Exception("Course Not found") :
                    await _coursesRepository.GetCurrentCoursAsync(ct);

        IEnumerable<Event> events = await _eventsRepository.GetAllEventsByCourseIdAsync(c.Id, ct);
        IEnumerable<EventPerson> allEventsPeople = await _eventsPeopleRespository.GetAllByCourseId(c.Id, ct);
        IDictionary<long, PersonGroupCourse> pgcs = (await _personGroupRespository.GetCurrentCourseGroupByPeopleIdsAsync(allEventsPeople.Select(x => x.PersonId).Distinct(), ct)).ToDictionary(x => x.PersonId, x => x);

        List<EventRow> rows = new List<EventRow>(events.Count());
        foreach (var e in events)
        {
            IEnumerable<EventPerson> eventPeople = allEventsPeople.Where(x => x.EventId == e.Id);
            var amipaPayments = eventPeople.Where(x => x.Paid && pgcs[x.PersonId].Amipa);
            var noAmipaPayments = eventPeople.Where(x => x.Paid && !pgcs[x.PersonId].Amipa);
            var eventPgcs = eventPeople.Select(x => pgcs[x.PersonId]);
            rows.Add(new EventRow()
            {
                Title = e.Name,
                AmipaPeople = amipaPayments.Count(),
                Ammount = noAmipaPayments.Count() * e.Price + amipaPayments.Count() * e.AmipaPrice,
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
    public decimal Ammount { get; set; }
    public int AmipaPeople { get; set; }

}