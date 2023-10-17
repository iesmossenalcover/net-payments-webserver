using Application.Common;
using Domain.Services;
using Domain.Entities.Events;
using FluentValidation;
using MediatR;
using Domain.Entities.People;
using Domain.Entities.Orders;
using Application.Common.Helpers;
using Application.Common.Models;
using System.Text.RegularExpressions;

namespace Application.Orders.Commands;

public record CreateOrderCommandVm(string Url, string MerchantParameters, string SignatureVersion, string Signature);

public record SelectedEvent(string Code, uint? Quantity);

public record CreateOrderCommand : IRequest<Response<CreateOrderCommandVm?>>
{
    public string DocumentId { get; set; } = string.Empty;
    public IEnumerable<SelectedEvent> Events { get; set; } = Enumerable.Empty<SelectedEvent>();
}

public class CreateEventCommandValidator : AbstractValidator<CreateOrderCommand>
{
    public CreateEventCommandValidator()
    {
        RuleFor(x => x.Events)
            .NotEmpty().WithMessage("Com a mínim s'ha de seleccionar un event.");
    }
}

public class CreateOrderCommandHandler : IRequestHandler<CreateOrderCommand, Response<CreateOrderCommandVm?>>
{
    #region IOC

    private readonly int MAX_TRIES = 10;
    private readonly IEventsPeopleRespository _eventsPeopleRepository;
    private readonly IEventPersonOrderRepository _eventPersonOrderRepository;
    private readonly IPersonGroupCourseRepository _peopleGroupCourseRepository;
    private readonly ICoursesRepository _coursesRepository;
    private readonly IOrdersRepository _ordersRepository;
    private readonly IRedsys _redsys;

    public CreateOrderCommandHandler(IEventsPeopleRespository eventsPeopleRepository,
        IPersonGroupCourseRepository peopleGroupCourseRepository, ICoursesRepository coursesRepository,
        IOrdersRepository ordersRepository, IRedsys redsys, IEventPersonOrderRepository eventPersonOrderRepository)
    {
        _eventsPeopleRepository = eventsPeopleRepository;
        _peopleGroupCourseRepository = peopleGroupCourseRepository;
        _coursesRepository = coursesRepository;
        _ordersRepository = ordersRepository;
        _redsys = redsys;
        _eventPersonOrderRepository = eventPersonOrderRepository;
    }

    #endregion

    public async Task<Response<CreateOrderCommandVm?>> Handle(CreateOrderCommand request, CancellationToken ct)
    {
        ct = CancellationToken.None;

        Course course = await _coursesRepository.GetCurrentCoursAsync(ct);
        PersonGroupCourse? pgc =
            await _peopleGroupCourseRepository.GetCoursePersonGroupByDocumentId(request.DocumentId, course.Id, ct);
        if (pgc == null)
        {
            return Response<CreateOrderCommandVm?>.Error(ResponseCode.NotFound,
                "No s'ha trobat cap persona amb aquest document al curs actual.");
        }

        Person person = pgc.Person;
        IEnumerable<EventPerson> personEvents =
            await _eventsPeopleRepository.GetAllByPersonAndCourse(person.Id, course.Id, ct);

        personEvents = personEvents.Where(x => x.Event.IsActive && !x.Paid);
        IEnumerable<string> activeEventCodes = personEvents.Select(x => x.Event.Code);

        if (!request.Events.All(x => activeEventCodes.Contains(x.Code)))
        {
            return Response<CreateOrderCommandVm?>.Error(ResponseCode.BadRequest,
                "S'han especificat esdeveniments que no es poden pagar.");
        }

        // Set quantities
        personEvents = personEvents.Where(x => request.Events.Select(y => y.Code).Contains(x.Event.Code));
        foreach (var pe in personEvents)
        {
            var r = request.Events.First(x => x.Code == pe.Event.Code);
            // If quantity must be set, then
            if (pe.Event.MaxQuantity > 1)
            {
                if (r.Quantity.HasValue && r.Quantity.Value > pe.Event.MaxQuantity)
                {
                    return Response<CreateOrderCommandVm?>.Error(ResponseCode.BadRequest,
                        $"L'esdeveniment {pe.Event.Name} permet com a màxim {pe.Event.MaxQuantity} quantitats");
                }
                else if (r.Quantity.HasValue && r.Quantity.Value <= 0)
                {
                    return Response<CreateOrderCommandVm?>.Error(ResponseCode.BadRequest,
                        $"Quantitat no vàlid per l'esdeveniment {pe.Event.Name}");
                }
            }

            pe.Quantity = pe.Event.MaxQuantity > 1 && r.Quantity.HasValue ? r.Quantity.Value : 1;
        }

        // Create order
        bool foundFreeCode = false;
        string code = string.Empty;
        for (int i = 0; i < MAX_TRIES && !foundFreeCode; i++)
        {
            code = $"{GenerateString.CurrentDateAsCode()}{GenerateString.RandomAlphanumeric(4)}";
            Order? existingOrder = await _ordersRepository.GetByCodeAsync(code, ct);
            if (existingOrder == null) foundFreeCode = true;
        }

        if (!foundFreeCode)
            return Response<CreateOrderCommandVm?>.Error(ResponseCode.InternalError,
                "S'ha porduït un error. Torna a inciar el procés.");

        // Create order
        Order order = new()
        {
            Code = code,
            Status = OrderStatus.Pending,
            Created = DateTimeOffset.UtcNow,
            Amount = personEvents.Sum(x => pgc.PriceForEvent(x.Event) * x.Quantity),
            Person = pgc.Person,
        };
        await _ordersRepository.InsertAsync(order, ct);

        // Create person event orders
        IEnumerable<EventPersonOrder> eventPersonOrders = personEvents.Select(x => new EventPersonOrder()
        {
            OrderId = order.Id,
            Order = order,
            EventPersonId = x.Id,
            EventPerson = x,
        });
        await _eventPersonOrderRepository.InsertManyAsync(eventPersonOrders, ct);

        RedsysRequest redsysRequest = _redsys.CreateRedsysRequest(order);
        var vm = new CreateOrderCommandVm(redsysRequest.Url, redsysRequest.MerchantParamenters,
            redsysRequest.SignatureVersion, redsysRequest.Signature);
        return Response<CreateOrderCommandVm?>.Ok(vm);
    }
}