using Application.Common;
using Application.Common.Services;
using FluentValidation;
using MediatR;
using Domain.Entities.Orders;
using Application.Common.Models;
using Domain.Entities.Events;
using Domain.Entities.People;

namespace Application.Orders.Commands;

public record ConfirmOrderCommandVm();

public record ConfirmOrderCommand : IRequest<Response<ConfirmOrderCommandVm?>>
{
    public string MerchantParamenters { get; set; } = string.Empty;
    public string Signature { get; set; } = string.Empty;
}

public class ConfirmOrderCommandValidator : AbstractValidator<CreateOrderCommand>
{
    public ConfirmOrderCommandValidator()
    {
        RuleFor(x => x.EventCodes).NotEmpty().WithMessage("Com a mínim s'ha de seleccionar un event.");
    }
}

public class ConfirmOrderCommandHandler : IRequestHandler<ConfirmOrderCommand, Response<ConfirmOrderCommandVm?>>
{
    #region IOC
    private readonly IEventsPeopleRespository _eventsPeopleRespository;
    private readonly IPersonGroupCourseRepository _personGroupCourseRepository;
    private readonly IOrdersRepository _ordersRepository;
    private readonly IRedsys _redsys;

    public ConfirmOrderCommandHandler(IEventsPeopleRespository eventsPeopleRespository, IPersonGroupCourseRepository personGroupCourseRepository, IOrdersRepository ordersRepository, IRedsys redsys)
    {
        _eventsPeopleRespository = eventsPeopleRespository;
        _personGroupCourseRepository = personGroupCourseRepository;
        _ordersRepository = ordersRepository;
        _redsys = redsys;
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
            return Response<ConfirmOrderCommandVm?>.Error(ResponseCode.BadRequest, "Error, cap event amb aquest ordre");
        }

        if (!result.Success)
        {
            order.Status = OrderStatus.Error;
            await _ordersRepository.UpdateAsync(order, CancellationToken.None);
            return Response<ConfirmOrderCommandVm?>.Error(ResponseCode.BadRequest, result.ErrorMessage ?? string.Empty);
        }

        order.Status = OrderStatus.Paid;
        foreach (var ep in personEvents)
        {
            ep.Paid = true;
        }

        await _eventsPeopleRespository.UpdateManyAsync(personEvents, CancellationToken.None);

        // buissness logic: todo move
        
        // enrollment
        EventPerson? enrollmentEvent = personEvents.FirstOrDefault(x => x.Event.Enrollment);
        if (enrollmentEvent != null)
        {
            PersonGroupCourse? pgc = await _personGroupCourseRepository.GetCoursePersonGroupById(enrollmentEvent.PersonId, enrollmentEvent.Event.CourseId, ct);
            if (pgc != null)
            {
                pgc.EnrollmentEvent = enrollmentEvent.Event;
                await _personGroupCourseRepository.UpdateAsync(pgc, CancellationToken.None);
            }
        }

        // amipa
        EventPerson? amipaEvent = personEvents.FirstOrDefault(x => x.Event.Amipa);
        if (amipaEvent != null)
        {
            PersonGroupCourse? pgc = await _personGroupCourseRepository.GetCoursePersonGroupById(amipaEvent.PersonId, amipaEvent.Event.CourseId, ct);
            if (pgc != null)
            {
                pgc.Amipa = true;
                await _personGroupCourseRepository.UpdateAsync(pgc, CancellationToken.None);
            }
        }

        return Response<ConfirmOrderCommandVm?>.Ok(new ConfirmOrderCommandVm());
    }
}