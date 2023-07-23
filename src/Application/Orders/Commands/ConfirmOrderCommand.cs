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
    private readonly IEventsPeopleRespository _eventsPeopleRespository;
    private readonly IPersonGroupCourseRepository _personGroupCourseRepository;
    private readonly IOrdersRepository _ordersRepository;
    private readonly IRedsys _redsys;
    private readonly Domain.Behaviours.EventPersonProcessingService _eventPersonProcessingService;

    public ConfirmOrderCommandHandler(IEventsPeopleRespository eventsPeopleRespository, IPersonGroupCourseRepository personGroupCourseRepository, IOrdersRepository ordersRepository, IRedsys redsys, EventPersonProcessingService eventPersonProcessingService)
    {
        _eventsPeopleRespository = eventsPeopleRespository;
        _personGroupCourseRepository = personGroupCourseRepository;
        _ordersRepository = ordersRepository;
        _redsys = redsys;
        _eventPersonProcessingService = eventPersonProcessingService;
    }
    #endregion

    public async Task<Response<ConfirmOrderCommandVm?>> Handle(ConfirmOrderCommand request, CancellationToken ct)
    {
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

        IEnumerable<EventPerson> personEvents = await _eventsPeopleRespository.GetAllByOrderId(order.Id, ct);
        if (!personEvents.Any())
        {
            return Response<ConfirmOrderCommandVm?>.Error(ResponseCode.BadRequest, "Error, cap esdeveniment amb aquest ordre");
        }

        if (!result.Success)
        {
            order.Status = OrderStatus.Error;
            await _ordersRepository.UpdateAsync(order, CancellationToken.None);
            return Response<ConfirmOrderCommandVm?>.Error(ResponseCode.BadRequest, result.ErrorMessage ?? string.Empty);
        }

        long courseId = personEvents.First().Event.CourseId;
        Person p = personEvents.First().Person;
        PersonGroupCourse? pgc = await _personGroupCourseRepository.GetCoursePersonGroupById(p.Id, courseId, ct);
        if (pgc == null)
        {
            return Response<ConfirmOrderCommandVm?>.Error(ResponseCode.BadRequest, "Error, la persona no està asociada al curs");
        }

        order.Status = OrderStatus.Paid;
        order.PaidDate = DateTimeOffset.UtcNow;
        foreach (var ep in personEvents)
        {
            ep.Paid = true;
            ep.PaidAsAmipa = pgc.Amipa;
        }

        await _eventsPeopleRespository.UpdateManyAsync(personEvents, CancellationToken.None);

        // Bussiness logic when an event is paid
        await _eventPersonProcessingService.ProcessPaidEvents(personEvents, true, CancellationToken.None);

        return Response<ConfirmOrderCommandVm?>.Ok(new ConfirmOrderCommandVm());
    }
}