using Application.Common;
using Domain.Services;
using Domain.Entities.Events;
using Domain.Entities.People;
using MediatR;

namespace Application.Events.Queries;

public record PaymentSummaryVm(int TotalCount, int AmipaCount, int NoAmipaCount, int TotalPaidCount, int AmipaPaidCount, int PaidCount, decimal Total, decimal Amipa, decimal NoAmipa, decimal TotalPaid, decimal AmipaPaid, decimal NoAmipaPaid);
public record EventPaymentVm(long Id, string FullName, string DocumentId, bool Amipa, decimal Price, bool Paid, string Group);
public record ListEventPaymentsVm(long Id, string Name, string Code, DateTimeOffset Date, PaymentSummaryVm Summary, IEnumerable<EventPaymentVm> PaidEvents, IEnumerable<EventPaymentVm> UnPaidEvents);

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
                ep.Id, person.FullName,
                person.DocumentId,
                ep.PaidAsAmipa,
                pgc.PriceForEvent(ep.Event),
                ep.Paid,
                pgc.Group.Name
            );
            payments.Add(epVm);
        }

        PaymentSummaryVm summaryVm = new PaymentSummaryVm(
            payments.Count(),
            payments.Where(x => x.Amipa).Count(),
            payments.Where(x => !x.Amipa).Count(),
            payments.Where(x => x.Paid).Count(),
            payments.Where(x => x.Amipa && x.Paid).Count(),
            payments.Where(x => !x.Amipa && x.Paid).Count(),
            payments.Sum(x => x.Price),
            payments.Where(x => x.Amipa).Sum(x => x.Price),
            payments.Where(x => !x.Amipa).Sum(x => x.Price),
            payments.Where(x => x.Paid).Sum(x => x.Price),
            payments.Where(x => x.Amipa && x.Paid).Sum(x => x.Price),
            payments.Where(x => !x.Amipa && x.Paid).Sum(x => x.Price)
        );

        var vm = new ListEventPaymentsVm(
            e.Id,
            e.Name,
            e.Code,
            e.Date,
            summaryVm,
            payments.Where(x => x.Paid).OrderBy(x => x.Group),
            payments.Where(x => !x.Paid).OrderBy(x => x.Group)
        );

        return Response<ListEventPaymentsVm>.Ok(vm);
    }
}