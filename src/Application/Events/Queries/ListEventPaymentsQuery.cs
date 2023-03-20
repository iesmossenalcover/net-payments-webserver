using Application.Common;
using Application.Common.Services;
using Domain.Entities.Events;
using Domain.Entities.People;
using MediatR;

namespace Application.Events.Queries;

public record EventPaymentVm(long Id, string FullName, bool Amipa, decimal Price, bool Paid);
public record ListEventPaymentsVm(long Id, string Code, decimal TotalPrice, decimal AmipaTotalPrice, int Count, int PaidCount, IEnumerable<EventPaymentVm> PaidEvents,IEnumerable<EventPaymentVm> UnPaidEvents );

public record ListEventPaymentsQuery(string Code) : IRequest<Response<ListEventPaymentsVm>>;

public class ListEventPaymentsQueryHandler : IRequestHandler<ListEventPaymentsQuery, Response<ListEventPaymentsVm>>
{
    #region IOC

    private readonly IPersonGroupCourseRepository _personGroupCourseRepository;
    private readonly IEventsRespository _eventsRepository;
    private readonly IEventsPeopleRespository _eventsPeopleRepository;

    public ListEventPaymentsQueryHandler(IPersonGroupCourseRepository personGroupCourseRepository, IEventsRespository eventsRepository, IEventsPeopleRespository eventsPeopleRepository)
    {
        _personGroupCourseRepository = personGroupCourseRepository;
        _eventsRepository = eventsRepository;
        _eventsPeopleRepository = eventsPeopleRepository;
    }

    #endregion

    public async Task<Response<ListEventPaymentsVm>> Handle(ListEventPaymentsQuery request, CancellationToken ct)
    {
        Event? e = await _eventsRepository.GetEventByCodeAsync(request.Code, ct);
        if (e == null) return Response<ListEventPaymentsVm>.Error(ResponseCode.NotFound, "Event no trobat");

        IEnumerable<EventPerson> eventPeople = await _eventsPeopleRepository.GetAllByEventIdAsync(e.Id, ct);
        IDictionary<long, PersonGroupCourse> pgcs =
                    (await _personGroupCourseRepository.GetCurrentCourseGroupByPeopleIdsAsync(eventPeople.Select(x => x.PersonId), ct))
                    .ToDictionary(x => x.PersonId, x => x);

        var payments = new List<EventPaymentVm>(eventPeople.Count());
        foreach (var ep in eventPeople)
        {
            Person person = ep.Person;
            if (!pgcs.ContainsKey(person.Id)) continue;
            PersonGroupCourse pgc = pgcs[person.Id];

            var epVm = new EventPaymentVm(
                ep.Id, $"{person.Name} {person.Surname1} {person.Surname2}",
                pgc.Amipa,
                pgc.PriceForEvent(ep.Event),
                ep.Paid
            );
            payments.Add(epVm);
        }

        decimal totalPrice = payments.Sum(x => x.Price);
        decimal amiaPrice = payments.Where(x => x.Amipa).Sum(x => x.Price);
        int paidCount = payments.Count(x => x.Paid);

        var vm = new ListEventPaymentsVm(e.Id, e.Code, totalPrice, amiaPrice, eventPeople.Count(), paidCount, payments.Where(x => x.Paid), payments.Where(x => !x.Paid));
        return Response<ListEventPaymentsVm>.Ok(vm);
    }
}