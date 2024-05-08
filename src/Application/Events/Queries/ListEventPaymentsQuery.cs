using Application.Common;
using Domain.Services;
using Domain.Entities.Events;
using Domain.Entities.People;
using MediatR;
using Application.Common.Helpers;

namespace Application.Events.Queries;

public record PaymentSummaryVm(int TotalCount, int AmipaCount, int NoAmipaCount, int TotalPaidCount, int AmipaPaidCount, int PaidCount, decimal TotalPaid, decimal AmipaPaid, decimal NoAmipaPaid);
public record EventPaymentVm(long Id, string FullName, string DocumentId, bool Amipa, decimal Price, bool Paid, string Group, uint Quantity, DateTimeOffset? DatePaid);

public record PaymentsEvent(
    long Id, string Name, string Code,
    string Description,
    decimal Price,
    decimal AmipaPrice,
    DateTimeOffset Date,
    DateTimeOffset PublishDate,
    DateTimeOffset? UnpublishDate,
    bool IsActive,
    bool IsAmpia,
    bool IsEnrollment,
    bool QuantitySelector, uint? MaxQuantity = null
);

public record ListEventPaymentsVm(
    PaymentsEvent Event,
    PaymentSummaryVm Summary, IEnumerable<EventPaymentVm> PaidEvents, IEnumerable<EventPaymentVm> UnPaidEvents,
    bool QuantitySelector, uint? MaxQuantity = null
);

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

        var quantitySelector = e.MaxQuantity > 1;

        var payments = new List<EventPaymentVm>(eventPeople.Count());
        foreach (var ep in eventPeople)
        {
            Person person = ep.Person;
            if (!pgcs.ContainsKey(person.Id)) continue;
            PersonGroupCourse pgc = pgcs[person.Id];

            var epVm = new EventPaymentVm(
                ep.Id, person.FullName,
                person.DocumentId,
                ep.Paid ? ep.PaidAsAmipa : pgc.Amipa,
                ep.Paid ? ep.AmountPaid(ep.Event) : pgc.PriceForEvent(ep.Event),
                ep.Paid,
                pgc.Group.Name,
                quantitySelector && ep.Paid ? ep.Quantity : 1,
                ep.DatePaid
            );
            payments.Add(epVm);
        }

        PaymentSummaryVm summaryVm = new PaymentSummaryVm(
            payments.Count,
            payments.Count(x => x.Amipa),
            payments.Count(x => !x.Amipa),
            payments.Count(x => x.Paid),
            payments.Count(x => x.Amipa && x.Paid),
            payments.Count(x => !x.Amipa && x.Paid),
            payments.Where(x => x.Paid).Sum(x => x.Price),
            payments.Where(x => x.Amipa && x.Paid).Sum(x => x.Price),
            payments.Where(x => !x.Amipa && x.Paid).Sum(x => x.Price)
        );

        var vm = new ListEventPaymentsVm(
            new PaymentsEvent(
                e.Id,
                e.Name,
                e.Code,
                e.Description,
                e.Price,
                e.AmipaPrice,
                e.Date,
                e.PublishDate,
                e.UnpublishDate,
                e.IsActive,
                e.Amipa,
                e.Enrollment,
                quantitySelector,
                quantitySelector ? e.MaxQuantity : null
            ),
            summaryVm,
            payments.Where(x => x.Paid).OrderByDescending(x => x.DatePaid).ThenBy(x => x.Group).ThenBy(x => x.FullName),
            payments.Where(x => !x.Paid).OrderBy(x => x.Group).ThenBy(x => x.FullName),
            quantitySelector,
            quantitySelector ? e.MaxQuantity : null
        );

        return Response<ListEventPaymentsVm>.Ok(vm);
    }
}