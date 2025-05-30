using Application.Common;
using Application.Common.Models;
using Domain.Services;
using Domain.Entities.Events;
using Domain.Entities.People;
using MediatR;

namespace Application.Events.Queries;

public record EventSummaryVm(long Id, string FullName, string DocumentId, bool Paid, long GroupId, string GroupName, uint? Quantity);
public record ListEventSummaryVm(long Id, string Name, DateTimeOffset Date, DateTimeOffset PublishDate, DateTimeOffset? UnpublishDate, string Code, IEnumerable<SelectOptionVm> Groups, IEnumerable<EventSummaryVm> Events);

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
                    (await _personGroupCourseRepository.GetPeopleGroupByPeopleIdsAndCourseIdAsync(e.CourseId, eventPeople.Select(x => x.PersonId), ct))
                    .ToDictionary(x => x.PersonId, x => x);

        var payments = new List<EventSummaryVm>(eventPeople.Count());
        foreach (var ep in eventPeople)
        {
            Person person = ep.Person;
            if (!pgcs.ContainsKey(person.Id)) continue;
            PersonGroupCourse pgc = pgcs[person.Id];

            var epVm = new EventSummaryVm(
                ep.Id, person.FormalFullName,
                person.DocumentId,
                ep.Paid,
                pgc.Group.Id,
                pgc.Group.Name,
                ep.Event.MaxQuantity > 1 && ep.Paid ? ep.Quantity : null
            );
            payments.Add(epVm);
        }

        var vm = new ListEventSummaryVm(
            e.Id, e.Name, e.Date, e.PublishDate, e.UnpublishDate, e.Code,
            pgcs.Values.DistinctBy(x => x.GroupId).Select(x => new SelectOptionVm(x.Group.Id.ToString(), x.Group.Name)).OrderBy(x => x.Value),
            payments.OrderBy(x => x.GroupName).ThenBy(x => x.FullName)
        );
        return Response<ListEventSummaryVm>.Ok(vm);
    }
}