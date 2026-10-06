using Application.Common;
using Application.Common.Helpers;
using Domain.Services;
using Domain.Entities.Events;
using Domain.Entities.People;
using FluentValidation;
using MediatR;
using Domain.Behaviours;

namespace Application.Events.Commands;

public record SetPersonEventPaidCommand : IRequest<Response<bool>>
{
    private long _Id;

    public long GetId => _Id;

    public void SetId(long value)
    {
        _Id = value;
    }

    public bool Paid { get; set; } = false;
    public uint? Quantity { get; set; }
}

public class SetPersonEventPaidValidator : AbstractValidator<SetPersonEventPaidCommand>
{
    public SetPersonEventPaidValidator()
    {
        RuleFor(x => x.GetId).NotEmpty().WithMessage("S'ha l'ID");
        RuleFor(x => x.Quantity).Must(x => x.HasValue ? x.Value > 0 : true).WithMessage("Quantitat invàlida");
    }
}

public class SetPersonEventPaidHandler : IRequestHandler<SetPersonEventPaidCommand, Response<bool>>
{
    #region IOC

    private readonly IEventsPeopleRespository _eventsPeopleRepository;
    private readonly IPersonGroupCourseRepository _personGroupCourseRepository;
    private readonly ICoursesRepository _coursesRepository;
    private readonly EventPersonBehaviours _eventPersonBehaviours;

    public SetPersonEventPaidHandler(IEventsPeopleRespository eventsPeopleRepository,
        IPersonGroupCourseRepository personGroupCourseRepository, ICoursesRepository coursesRepository,
        EventPersonBehaviours eventPersonBehaviours)
    {
        _eventsPeopleRepository = eventsPeopleRepository;
        _personGroupCourseRepository = personGroupCourseRepository;
        _coursesRepository = coursesRepository;
        _eventPersonBehaviours = eventPersonBehaviours;
    }

    #endregion

    public async Task<Response<bool>> Handle(SetPersonEventPaidCommand request, CancellationToken ct)
    {
        EventPerson? eventPerson = await _eventsPeopleRepository.GetWithRelationsByIdAsync(request.GetId, ct);
        if (eventPerson == null)
            return Response<bool>.Error(ResponseCode.NotFound, @"Aquesta persona no està  a l'esdeveniment.");

        Course course = await _coursesRepository.GetCurrentCoursAsync(ct);
        if (course.Id != eventPerson.Event.CourseId)
            return Response<bool>.Error(ResponseCode.BadRequest, @"No es pot fer un pagament d'un curs no actiu.");

        PersonGroupCourse? pgc =
            await _personGroupCourseRepository.GetCoursePersonGroupById(eventPerson.PersonId, course.Id, ct);
        if (pgc == null)
            return Response<bool>.Error(ResponseCode.BadRequest, "Error, la persona no està asociada al curs");

        // Sense l'autorització del curs no es pot marcar com a pagat. Desmarcar sí que es permet,
        // perquè l'autorització es pot haver revocat després d'un pagament ja fet.
        if (request.Paid && !pgc.IsAuthorizedFor(eventPerson.Event))
        {
            return Response<bool>.Error(ResponseCode.BadRequest,
                $"No es pot marcar com a pagat. {AuthorizationMessages.Missing(eventPerson.Event)}.");
        }

        // Límit de places: només s'aplica als pagaments manuals. Els pagaments online no es limiten
        // (un pagament de Redsys que arriba tard ja està cobrat). Cada unitat de la quantitat ocupa una plaça,
        // i si ja estava pagat només compten les unitats noves.
        uint quantity = Math.Min(request.Quantity ?? 1, eventPerson.Event.MaxQuantity);
        long extraPlaces = (long)quantity - (eventPerson.Paid ? eventPerson.Quantity : 0);
        if (request.Paid && extraPlaces > 0 && eventPerson.Event.MaxCapacity.HasValue)
        {
            long paidPlaces = await _eventsPeopleRepository.GetPaidPlacesByEventIdAsync(eventPerson.EventId, ct);
            long maxCapacity = eventPerson.Event.MaxCapacity.Value;
            if (paidPlaces + extraPlaces > maxCapacity)
            {
                long free = Math.Max(0, maxCapacity - paidPlaces);
                return Response<bool>.Error(ResponseCode.BadRequest,
                    $"No es pot marcar com a pagat. Queden {free} places lliures ({paidPlaces}/{maxCapacity}) i se'n demanen {extraPlaces}. Si cal, augmenta el nombre de places.");
            }
        }

        // Ensure this because they may be an attempt to pay using tpv, so it is related to an unpaid order.
        eventPerson.PaidOrder = null;
        eventPerson.PaidOrderId = null;

        eventPerson.Quantity = quantity;

        if (request.Paid)
        {
            await _eventPersonBehaviours.PayEvents(new[] { eventPerson }, pgc.Amipa, ct);
        }
        else
        {
            await _eventPersonBehaviours.UnPayEvents(new[] { eventPerson }, ct);
        }

        return Response<bool>.Ok(true);
    }
}