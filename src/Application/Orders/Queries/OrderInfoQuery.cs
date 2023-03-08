using Application.Common;
using Application.Common.Models;
using Application.Common.Services;
using Domain.Entities.Events;
using Domain.Entities.Orders;
using Domain.Entities.People;
using MediatR;

namespace Application.Orders.Queries;

public record EventInfo(string Code, string Name, decimal Price, string Currency);
public record OrderInfoVm(IEnumerable<EventInfo> events);

public record OrderInfoQuery(string Signature, string MerchantParamenters, string SignatureVersion) : IRequest<Response<OrderInfoVm>>;

public class OrderInfoQueryHandler : IRequestHandler<OrderInfoQuery, Response<OrderInfoVm>>
{
    private readonly IRedsys _redsys;
    private readonly IOrdersRepository _ordersRepository;
    private readonly IPersonGroupCourseRepository _personGroupsCourseRepository;
    private readonly IEventsPeopleRespository _eventsPeopleRespository;
    private readonly ICoursesRepository _coursesRepository;

    public OrderInfoQueryHandler(IRedsys redsys, IOrdersRepository ordersRepository, IPersonGroupCourseRepository personGroupsCourseRepository, IEventsPeopleRespository eventsPeopleRespository, ICoursesRepository coursesRepository)
    {
        _redsys = redsys;
        _ordersRepository = ordersRepository;
        _personGroupsCourseRepository = personGroupsCourseRepository;
        _eventsPeopleRespository = eventsPeopleRespository;
        _coursesRepository = coursesRepository;
    }

    public async Task<Response<OrderInfoVm>> Handle(OrderInfoQuery request, CancellationToken ct)
    {
        bool isValid = _redsys.Validate(request.MerchantParamenters, request.Signature);
        if (!isValid) return Response<OrderInfoVm>.Error(ResponseCode.BadRequest, "Firma no vàlida");

        RedsysResult data = _redsys.GetResult(request.Signature);
        Order? order = await _ordersRepository.GetByCodeAsync(data.OrderCode, ct);
        if (order == null) return Response<OrderInfoVm>.Error(ResponseCode.BadRequest, "No s'ha trobat l'ordre.");

        Course course = await _coursesRepository.GetCurrentCoursAsync(ct);
        PersonGroupCourse? pgc = await _personGroupsCourseRepository.GetCoursePersonGroupById(order.PersonId, course.Id, ct);
        if (pgc == null) return Response<OrderInfoVm>.Error(ResponseCode.BadRequest, "Persona no matriculada al curs actual.");

        IEnumerable<EventPerson> orderEvents = await _eventsPeopleRespository.GetAllByOrderId(order.Id, ct);
        IEnumerable<EventInfo> eventsInfo = orderEvents.Select(x => new EventInfo(x.Event.Code, x.Event.Name, pgc.PriceForEvent(x.Event), "€"));
        return Response<OrderInfoVm>.Ok(new OrderInfoVm(eventsInfo));
    }
}