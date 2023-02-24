using Application.Common;
using Application.Common.Services;
using Domain.Entities.Events;
using FluentValidation;
using MediatR;
using System.Text;
using Domain.Entities.People;
using Domain.Entities.Orders;
using Application.Common.Helpers;

namespace Application.Orders.Commands;

public record CreateOrderCommand : IRequest<Response<long?>>
{
    public string DocumentId { get; set; } = string.Empty;
    public IEnumerable<string> EventCodes { get; set; } = Enumerable.Empty<string>();
}

public class CreateEventCommandValidator : AbstractValidator<CreateOrderCommand>
{
	public CreateEventCommandValidator()
	{
        RuleFor(x => x.EventCodes).NotEmpty().WithMessage("Com a mínim s'ha de seleccionar un event.");
    }
}

public class CreateOrderCommandHandler : IRequestHandler<CreateOrderCommand, Response<long?>>
{
    #region IOC
    private readonly int MAX_TRIES = 10;
    private readonly IEventsPeopleRespository _eventsPeopleRepository;
    private readonly IPersonGroupCourseRepository _peopleGroupCourseRepository;
    private readonly ICoursesRepository _coursesRepository;
    private readonly IOrdersRepository _ordersRepository;

    public CreateOrderCommandHandler(IEventsPeopleRespository eventsPeopleRepository, IPersonGroupCourseRepository peopleGroupCourseRepository, ICoursesRepository coursesRepository, IOrdersRepository ordersRepository)
    {
        _eventsPeopleRepository = eventsPeopleRepository;
        _peopleGroupCourseRepository = peopleGroupCourseRepository;
        _coursesRepository = coursesRepository;
        _ordersRepository = ordersRepository;
    }

    #endregion

    public async Task<Response<long?>> Handle(CreateOrderCommand request, CancellationToken ct)
    {
        Course course = await _coursesRepository.GetCurrentCoursAsync(ct);
        PersonGroupCourse? pgc = await _peopleGroupCourseRepository.GetCoursePersonGroupBy(request.DocumentId, course.Id, ct);
        
        if (pgc == null)
        {
            return Response<long?>.Error(ResponseCode.NotFound, "No s'ha trobat cap persona amb aquest document al curs actual.");
        }
        Person person = pgc.Person;
        IEnumerable<EventPerson> personEvents = await _eventsPeopleRepository.GetAllByPersonAndCourse(person.Id, course.Id, ct);
        personEvents = personEvents.Where(x => x.Event.IsActive && !x.Paid);
        IEnumerable<string> activeEventCodes = personEvents.Select(x => x.Event.Code);
        if (!request.EventCodes.All(x => activeEventCodes.Contains(x)))
        {
            return Response<long?>.Error(ResponseCode.BadRequest, "S'han especificat esdeveniments que no es poden pagar.");
        }
        
        // Create order
        bool foundFreeCode = false;
        string code = string.Empty;
        for (int i = 0; i < MAX_TRIES && !foundFreeCode; i++)
        {
            code = $"{GenerateString.RandomNumeric(4)}{GenerateString.RandomAlphanumeric(8)}";
            Order? existingOrder = await _ordersRepository.GetByCodeAsync(code, ct);
            if (existingOrder == null) foundFreeCode = true;
        }

        if (!foundFreeCode) return Response<long?>.Error(ResponseCode.InternalError, "S'ha porduït un error. Torna a inciar el procés.");

        Order order = new Order()
        {
            Code = code,
            Status = OrderStatus.Pending,
            Created = DateTimeOffset.UtcNow,
            Price = personEvents.Sum(x => pgc.PriceForEvent(x.Event)),
        };
        await _ordersRepository.InsertAsync(order, CancellationToken.None);

        foreach (var pe in personEvents)
        {
            pe.Order = order;
        }
        await _eventsPeopleRepository.UpdateManyAsync(personEvents, CancellationToken.None);

        // Generar dades tpv

        // Retornar dades tpv

        return Response<long?>.Ok(1);
    }
}