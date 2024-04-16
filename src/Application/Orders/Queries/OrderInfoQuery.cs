using Application.Common;
using Application.Common.Models;
using Domain.Services;
using Domain.Entities.Configuration;
using Domain.Entities.Events;
using Domain.Entities.Orders;
using Domain.Entities.People;
using MediatR;

namespace Application.Orders.Queries;

public record EventInfo(string Code, string Name, uint Quantity, decimal Price, string Currency);

public record OrderInfoVm(string PersonName, string PersonDocumentId, IEnumerable<EventInfo> Events,
    bool DisplayEnrollment, string? EnrollmentSubjectsInfo, string? GroupDescription);

public record OrderInfoQuery
    (string Signature, string MerchantParameters, string SignatureVersion) : IRequest<Response<OrderInfoVm>>;

public class OrderInfoQueryHandler : IRequestHandler<OrderInfoQuery, Response<OrderInfoVm>>
{
    private readonly IRedsys _redsys;
    private readonly IOrdersRepository _ordersRepository;
    private readonly IPersonGroupCourseRepository _personGroupsCourseRepository;
    private readonly IEventsPeopleRespository _eventsPeopleRepository;
    private readonly ICoursesRepository _coursesRepository;
    private readonly IAppConfigRepository _appConfigRepository;

    public OrderInfoQueryHandler(IRedsys redsys, IOrdersRepository ordersRepository,
        IPersonGroupCourseRepository personGroupsCourseRepository, IEventsPeopleRespository eventsPeopleRepository,
        ICoursesRepository coursesRepository, IAppConfigRepository appConfigRepository)
    {
        _redsys = redsys;
        _ordersRepository = ordersRepository;
        _personGroupsCourseRepository = personGroupsCourseRepository;
        _eventsPeopleRepository = eventsPeopleRepository;
        _coursesRepository = coursesRepository;
        _appConfigRepository = appConfigRepository;
    }

    public async Task<Response<OrderInfoVm>> Handle(OrderInfoQuery request, CancellationToken ct)
    {
        bool isValid = _redsys.Validate(request.MerchantParameters, request.Signature);
        if (!isValid) return Response<OrderInfoVm>.Error(ResponseCode.BadRequest, "Firma no vàlida");

        RedsysResult data = _redsys.GetResult(request.MerchantParameters);
        Order? order = await _ordersRepository.GetByCodeAsync(data.OrderCode, ct);
        if (order == null) return Response<OrderInfoVm>.Error(ResponseCode.BadRequest, "No s'ha trobat l'ordre.");

        Course course = await _coursesRepository.GetCurrentCoursAsync(ct);
        PersonGroupCourse? pgc =
            await _personGroupsCourseRepository.GetCoursePersonGroupById(order.PersonId, course.Id, ct);
        if (pgc == null)
            return Response<OrderInfoVm>.Error(ResponseCode.BadRequest, "Persona no matriculada al curs actual.");

        IEnumerable<EventPerson> orderEvents = await _eventsPeopleRepository.GetAllByOrderId(order.Id, ct);

        AppConfig config = await _appConfigRepository.GetAsync(ct);

        bool enrollmentEvent = orderEvents.Any(x => x.Event.Enrollment);

        IEnumerable<EventInfo> eventsInfo =
            orderEvents.Select(x => new EventInfo(x.Event.Code, x.Event.Name,
                x.Quantity,
                pgc.PriceForEvent(x.Event) * x.Quantity, "€"));
        return Response<OrderInfoVm>.Ok(new OrderInfoVm(
            pgc.Person.FullName, pgc.Person.DocumentId,
            eventsInfo, enrollmentEvent && config.DisplayEnrollment, pgc.SubjectsInfo, pgc.Group.Description));
    }
}