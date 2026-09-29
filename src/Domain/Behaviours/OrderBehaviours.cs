using Domain.Entities.Events;
using Domain.Entities.Orders;
using Domain.Entities.People;
using Domain.Services;

namespace Domain.Behaviours;

public class OrderBehaviours
{
    #region IOC

    private readonly IEventsPeopleRespository _eventsPeopleRepository;
    private readonly IPersonGroupCourseRepository _personGroupCourseRepository;
    private readonly IEventPersonOrderRepository _eventPersonOrderRepository;
    private readonly IOrdersRepository _ordersRepository;
    private readonly EventPersonBehaviours _eventPersonBehaviours;

    public OrderBehaviours(IEventsPeopleRespository eventsPeopleRepository,
        IPersonGroupCourseRepository personGroupCourseRepository, IEventPersonOrderRepository eventPersonOrderRepository,
        IOrdersRepository ordersRepository, EventPersonBehaviours eventPersonBehaviours)
    {
        _eventsPeopleRepository = eventsPeopleRepository;
        _personGroupCourseRepository = personGroupCourseRepository;
        _eventPersonOrderRepository = eventPersonOrderRepository;
        _ordersRepository = ordersRepository;
        _eventPersonBehaviours = eventPersonBehaviours;
    }

    #endregion

    /// <summary>
    /// Marca l'ordre i tots els seus esdeveniments com a pagats.
    /// Retorna un missatge d'error o null si tot ha anat bé.
    /// </summary>
    public async Task<string?> PayOrder(Order order, CancellationToken ct)
    {
        // Get all PersonEventOrder paid by this order
        IEnumerable<EventPersonOrder> personEventOrders =
            await _eventPersonOrderRepository.GetAllByOrderIdAsync(order.Id, ct);
        if (!personEventOrders.Any())
        {
            return "Error, cap esdeveniment amb aquest ordre";
        }

        IEnumerable<long> eventPersonIds = personEventOrders.Select(x => x.EventPersonId);

        IEnumerable<EventPerson> personEvents =
            await _eventsPeopleRepository.GetWithRelationsByIdsAsync(eventPersonIds, ct);

        long courseId = personEvents.First().Event.CourseId;
        Person p = personEvents.First().Person;
        PersonGroupCourse? pgc = await _personGroupCourseRepository.GetCoursePersonGroupById(p.Id, courseId, ct);
        if (pgc == null)
        {
            return "Error, la persona no està asociada al curs";
        }

        // Update quantities on person_events based on person_event_order
        // IMPORTANT to avoid fraud. Always set paid order quantity.
        foreach (var epo in personEventOrders)
        {
            EventPerson ep = personEvents.First(x => x.Id == epo.EventPersonId);
            ep.Quantity = epo.Quantity;
        }

        // Update order from all personEvents
        foreach (var pe in personEvents)
        {
            pe.PaidOrderId = order.Id;
            pe.PaidOrder = order;
        }

        order.Status = OrderStatus.Paid;
        order.PaidDate = DateTimeOffset.UtcNow;
        await _ordersRepository.UpdateAsync(order, ct);

        await _eventPersonBehaviours.PayEvents(personEvents, pgc.Amipa, ct);

        return null;
    }
}
