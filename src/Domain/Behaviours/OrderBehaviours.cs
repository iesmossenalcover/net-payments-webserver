using Domain.Entities.Events;
using Domain.Entities.Orders;
using Domain.Entities.People;
using Domain.Services;

namespace Domain.Behaviours;

public enum PayOrderResult
{
    Ok = 0,
    AlreadyPaid = 1,
    NoEventsForOrder = 2,
    PersonNotInCourse = 3,
}

public class OrderBehaviours
{
    #region IOC

    private readonly IEventsPeopleRespository _eventsPeopleRepository;
    private readonly IPersonGroupCourseRepository _personGroupCourseRepository;
    private readonly IEventPersonOrderRepository _eventPersonOrderRepository;
    private readonly IOrdersRepository _ordersRepository;
    private readonly EventPersonBehaviours _eventPersonBehaviours;
    private readonly IUnitOfWork _unitOfWork;

    public OrderBehaviours(IEventsPeopleRespository eventsPeopleRepository,
        IPersonGroupCourseRepository personGroupCourseRepository, IEventPersonOrderRepository eventPersonOrderRepository,
        IOrdersRepository ordersRepository, EventPersonBehaviours eventPersonBehaviours, IUnitOfWork unitOfWork)
    {
        _eventsPeopleRepository = eventsPeopleRepository;
        _personGroupCourseRepository = personGroupCourseRepository;
        _eventPersonOrderRepository = eventPersonOrderRepository;
        _ordersRepository = ordersRepository;
        _eventPersonBehaviours = eventPersonBehaviours;
        _unitOfWork = unitOfWork;
    }

    #endregion

    /// <summary>
    /// Marca l'ordre i tots els seus esdeveniments com a pagats.
    /// És idempotent: Redsys pot repetir la notificació i el camí gratuït es pot reintentar.
    /// </summary>
    public async Task<PayOrderResult> PayOrder(Order order, CancellationToken ct)
    {
        if (order.Status == OrderStatus.Paid) return PayOrderResult.AlreadyPaid;

        // Get all PersonEventOrder paid by this order
        IEnumerable<EventPersonOrder> personEventOrders =
            await _eventPersonOrderRepository.GetAllByOrderIdAsync(order.Id, ct);
        if (!personEventOrders.Any())
        {
            return PayOrderResult.NoEventsForOrder;
        }

        IEnumerable<long> eventPersonIds = personEventOrders.Select(x => x.EventPersonId);

        IEnumerable<EventPerson> personEvents =
            await _eventsPeopleRepository.GetWithRelationsByIdsAsync(eventPersonIds, ct);

        long courseId = personEvents.First().Event.CourseId;
        Person p = personEvents.First().Person;
        PersonGroupCourse? pgc = await _personGroupCourseRepository.GetCoursePersonGroupById(p.Id, courseId, ct);
        if (pgc == null)
        {
            return PayOrderResult.PersonNotInCourse;
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

        // L'ordre, els esdeveniments i la matrícula/AMIPA s'han de moure junts: si només se'n
        // desés una part l'ordre quedaria pagada amb esdeveniments sense marcar.
        await _unitOfWork.ExecuteInTransactionAsync(async token =>
        {
            await _ordersRepository.UpdateAsync(order, token);
            await _eventPersonBehaviours.PayEvents(personEvents, pgc.Amipa, token);
        }, ct);

        return PayOrderResult.Ok;
    }
}
