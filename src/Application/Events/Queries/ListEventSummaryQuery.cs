using Application.Common;
using Application.Common.Services;
using Domain.Entities.Events;
using Domain.Entities.People;
using MediatR;

namespace Application.Events.Queries;

public record EventSummaryVm(long Id, string FullName, string DocumentId, bool Paid, string Group);
public record ListEventSummaryVm(long Id,string Name, string Code, IEnumerable<EventSummaryVm> PaidEvents,IEnumerable<EventSummaryVm> UnPaidEvents );

public record ListEventSummaryQuery(string Code) : IRequest<Response<ListEventSummaryVm>>;

public class ListEventSummarysQueryHandler : IRequestHandler<ListEventSummaryQuery, Response<ListEventSummaryVm>>
{
    #region IOC

    private readonly IPersonGroupCourseRepository _personGroupCourseRepository;
    private readonly IEventsRespository _eventsRepository;
    private readonly IEventsPeopleRespository _eventsPeopleRepository;

    public ListEventSummarysQueryHandler(IPersonGroupCourseRepository personGroupCourseRepository, IEventsRespository eventsRepository, IEventsPeopleRespository eventsPeopleRepository)
    {
        _personGroupCourseRepository = personGroupCourseRepository;
        _eventsRepository = eventsRepository;
        _eventsPeopleRepository = eventsPeopleRepository;
    }

    #endregion

    public async Task<Response<ListEventSummaryVm>> Handle(ListEventSummaryQuery request, CancellationToken ct)
    {
        Event? e = await _eventsRepository.GetEventByCodeAsync(request.Code, ct);
        if (e == null) return Response<ListEventSummaryVm>.Error(ResponseCode.NotFound, "Event no trobat");

        IEnumerable<EventPerson> eventPeople = await _eventsPeopleRepository.GetAllByEventIdAsync(e.Id, ct);
        IDictionary<long, PersonGroupCourse> pgcs =
                    (await _personGroupCourseRepository.GetCurrentCourseGroupByPeopleIdsAsync(eventPeople.Select(x => x.PersonId), ct))
                    .ToDictionary(x => x.PersonId, x => x);

        var payments = new List<EventSummaryVm>(eventPeople.Count());
        foreach (var ep in eventPeople)
        {
            Person person = ep.Person;
            if (!pgcs.ContainsKey(person.Id)) continue;
            PersonGroupCourse pgc = pgcs[person.Id];

            var epVm = new EventSummaryVm(
                ep.Id, $"{person.Name} {person.Surname1} {person.Surname2}",
                person.DocumentId,
                ep.Paid,
                pgc.Group.Name
            );
            payments.Add(epVm);
        }


        var vm = new ListEventSummaryVm(e.Id, e.Name, e.Code, payments.Where(x => x.Paid).OrderBy(x => x.Group), payments.Where(x => !x.Paid).OrderBy(x => x.Group));
        return Response<ListEventSummaryVm>.Ok(vm);
    }
}