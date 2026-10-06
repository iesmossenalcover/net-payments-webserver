using Application.Common;
using Domain.Behaviours;
using Domain.Entities.Events;
using Domain.Entities.People;
using Domain.Services;
using MediatR;

namespace Application.Events.Commands;

/// <summary>
/// <paramref name="Paid"/> són els que s'han marcat com a pagats i
/// <paramref name="SkippedNotAuthorized"/> els que s'han deixat com estaven perquè
/// els falta l'autorització del curs.
/// </summary>
public record SetEventPeoplePaidVm(int Paid, int SkippedNotAuthorized);

/// <summary>
/// Marca com a pagats tots els alumnes pendents d'un esdeveniment que tenen
/// l'autorització del curs. Qui no la té es queda sense pagar.
/// </summary>
public record SetEventPeoplePaidCommand(string EventCode) : IRequest<Response<SetEventPeoplePaidVm>>;

public class SetEventPeoplePaidHandler : IRequestHandler<SetEventPeoplePaidCommand, Response<SetEventPeoplePaidVm>>
{
    #region IOC

    private readonly IEventsRespository _eventsRepository;
    private readonly IEventsPeopleRespository _eventsPeopleRepository;
    private readonly IPersonGroupCourseRepository _personGroupCourseRepository;
    private readonly ICoursesRepository _coursesRepository;
    private readonly EventPersonBehaviours _eventPersonBehaviours;
    private readonly IUnitOfWork _unitOfWork;

    public SetEventPeoplePaidHandler(IEventsRespository eventsRepository,
        IEventsPeopleRespository eventsPeopleRepository, IPersonGroupCourseRepository personGroupCourseRepository,
        ICoursesRepository coursesRepository, EventPersonBehaviours eventPersonBehaviours, IUnitOfWork unitOfWork)
    {
        _eventsRepository = eventsRepository;
        _eventsPeopleRepository = eventsPeopleRepository;
        _personGroupCourseRepository = personGroupCourseRepository;
        _coursesRepository = coursesRepository;
        _eventPersonBehaviours = eventPersonBehaviours;
        _unitOfWork = unitOfWork;
    }

    #endregion

    public async Task<Response<SetEventPeoplePaidVm>> Handle(SetEventPeoplePaidCommand request, CancellationToken ct)
    {
        Event? e = await _eventsRepository.GetEventByCodeAsync(request.EventCode, ct);
        if (e == null) return Response<SetEventPeoplePaidVm>.Error(ResponseCode.NotFound, "Esdeveniment no trobat");

        Course course = await _coursesRepository.GetCurrentCoursAsync(ct);
        if (course.Id != e.CourseId)
            return Response<SetEventPeoplePaidVm>.Error(ResponseCode.BadRequest,
                @"No es pot fer un pagament d'un curs no actiu.");

        IEnumerable<EventPerson> all = await _eventsPeopleRepository.GetAllByEventIdAsync(e.Id, ct);
        IEnumerable<EventPerson> unpaid = all.Where(x => !x.Paid);

        IDictionary<long, PersonGroupCourse> pgcs =
            (await _personGroupCourseRepository.GetPeopleGroupByPeopleIdsAndCourseIdAsync(e.CourseId,
                unpaid.Select(x => x.PersonId), ct))
            .ToDictionary(x => x.PersonId, x => x);

        List<EventPerson> toPay = unpaid
            .Where(x => pgcs.ContainsKey(x.PersonId) && pgcs[x.PersonId].IsAuthorizedFor(e))
            .ToList();

        int skipped = unpaid.Count(x => pgcs.ContainsKey(x.PersonId) && !pgcs[x.PersonId].IsAuthorizedFor(e));
        if (toPay.Count == 0) return Response<SetEventPeoplePaidVm>.Ok(new SetEventPeoplePaidVm(0, skipped));

        // Límit de places: o caben tots o no se'n marca cap, per no triar a l'atzar qui es queda fora.
        if (e.MaxCapacity.HasValue)
        {
            // Cada unitat pagada ocupa una plaça; el pagament massiu marca quantitat 1 per alumne.
            long paidPlaces = all.Where(x => x.Paid).Sum(x => (long)x.Quantity);
            long free = Math.Max(0, (long)e.MaxCapacity.Value - paidPlaces);
            if (toPay.Count > free)
            {
                return Response<SetEventPeoplePaidVm>.Error(ResponseCode.BadRequest,
                    $"No hi ha places per a tots: queden {free} places lliures ({paidPlaces}/{e.MaxCapacity.Value}) i hi ha {toPay.Count} alumnes pendents. Marca'ls un a un o augmenta el nombre de places.");
            }
        }

        IEnumerable<EventPerson> eventPeople =
            await _eventsPeopleRepository.GetWithRelationsByIdsAsync(toPay.Select(x => x.Id), ct);

        foreach (var ep in eventPeople)
        {
            // Com al pagament individual: pot haver-hi un intent de pagament per TPV a mitges,
            // i aquest pagament manual no té cap ordre associada.
            ep.PaidOrder = null;
            ep.PaidOrderId = null;
            ep.Quantity = 1;
        }

        // Dues escriptures com a màxim (event_person i, si és matrícula o AMIPA, person_group_course),
        // independentment de quants alumnes siguin. La transacció les manté juntes.
        await _unitOfWork.ExecuteInTransactionAsync(
            token => _eventPersonBehaviours.PayEventsForPeople(
                eventPeople.Select(x => (x, pgcs[x.PersonId])), token),
            ct);

        return Response<SetEventPeoplePaidVm>.Ok(new SetEventPeoplePaidVm(eventPeople.Count(), skipped));
    }
}
