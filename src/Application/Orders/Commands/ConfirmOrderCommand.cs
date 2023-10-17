using Application.Common;
using Domain.Services;
using FluentValidation;
using MediatR;
using Domain.Entities.Orders;
using Application.Common.Models;
using Domain.Entities.Events;
using Domain.Entities.People;
using Domain.Behaviours;

namespace Application.Orders.Commands;

public record ConfirmOrderCommandVm();

public record ConfirmOrderCommand : IRequest<Response<ConfirmOrderCommandVm?>>
{
    public string MerchantParamenters { get; set; } = string.Empty;
    public string Signature { get; set; } = string.Empty;
}

public class ConfirmOrderCommandHandler : IRequestHandler<ConfirmOrderCommand, Response<ConfirmOrderCommandVm?>>
{
    #region IOC

    private readonly IEventsPeopleRespository _eventsPeopleRepository;
    private readonly IPersonGroupCourseRepository _personGroupCourseRepository;
    private readonly IEventPersonOrderRepository _eventPersonOrderRepository;
    private readonly IOrdersRepository _ordersRepository;
    private readonly IRedsys _redsys;
    private readonly Domain.Behaviours.EventPersonBehaviours _eventPersonBehaviours;

    public ConfirmOrderCommandHandler(IEventsPeopleRespository eventsPeopleRepository,
        IPersonGroupCourseRepository personGroupCourseRepository, IOrdersRepository ordersRepository, IRedsys redsys,
        EventPersonBehaviours eventPersonBehaviours, IEventPersonOrderRepository eventPersonOrderRepository)
    {
        _eventsPeopleRepository = eventsPeopleRepository;
        _personGroupCourseRepository = personGroupCourseRepository;
        _ordersRepository = ordersRepository;
        _redsys = redsys;
        _eventPersonBehaviours = eventPersonBehaviours;
        _eventPersonOrderRepository = eventPersonOrderRepository;
    }

    #endregion

    public async Task<Response<ConfirmOrderCommandVm?>> Handle(ConfirmOrderCommand request, CancellationToken ct)
    {
        ct = CancellationToken.None;

        bool isValid = _redsys.Validate(request.MerchantParamenters, request.Signature);
        if (!isValid)
        {
            return Response<ConfirmOrderCommandVm?>.Error(ResponseCode.BadRequest, "Firma invàlida");
        }

        RedsysResult result = _redsys.GetResult(request.MerchantParamenters);
        Order? order = await _ordersRepository.GetByCodeAsync(result.OrderCode, ct);
        if (order == null)
        {
            return Response<ConfirmOrderCommandVm?>.Error(ResponseCode.BadRequest, "Error, l'ordre no existeix");
        }

        if (!result.Success)
        {
            order.Status = OrderStatus.Error;
            await _ordersRepository.UpdateAsync(order, ct);
            return Response<ConfirmOrderCommandVm?>.Error(ResponseCode.BadRequest, result.ErrorMessage ?? string.Empty);
        }

        // Get all PersonEventOrder paid by this order
        IEnumerable<EventPersonOrder> personEventOrders =
            await _eventPersonOrderRepository.GetAllByOrderIdAsync(order.Id, ct);
        if (!personEventOrders.Any())
        {
            return Response<ConfirmOrderCommandVm?>.Error(ResponseCode.BadRequest,
                "Error, cap esdeveniment amb aquest ordre");
        }

        IEnumerable<EventPerson> personEvents = personEventOrders.Select(x => x.EventPerson);
        
        long courseId = personEvents.First().Event.CourseId;
        Person p = personEvents.First().Person;
        PersonGroupCourse? pgc = await _personGroupCourseRepository.GetCoursePersonGroupById(p.Id, courseId, ct);
        if (pgc == null)
        {
            return Response<ConfirmOrderCommandVm?>.Error(ResponseCode.BadRequest,
                "Error, la persona no està asociada al curs");
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

        return Response<ConfirmOrderCommandVm?>.Ok(new ConfirmOrderCommandVm());
    }
}