using Domain.Entities.Events;
using Domain.Entities.People;
using Domain.Services;

namespace Domain.Behaviours;

public class EventPersonBehaviours
{
    #region IOC

    private readonly IPersonGroupCourseRepository _personGroupCourseRepository;
    private readonly IEventsPeopleRespository _eventsPeopleRepository;

    public EventPersonBehaviours(IPersonGroupCourseRepository personGroupCourseRepository,
        IEventsPeopleRespository eventsPeopleRepository)
    {
        _personGroupCourseRepository = personGroupCourseRepository;
        _eventsPeopleRepository = eventsPeopleRepository;
    }

    #endregion

    public async Task UnPayEvents(IEnumerable<EventPerson> personEvents, CancellationToken ct)
    {
        // Mark all event person as unpaid
        foreach (var ep in personEvents)
        {
            SetPaid(ep, false, false);
        }

        await _eventsPeopleRepository.UpdateManyAsync(personEvents, ct);
        await ProcessPaidEvents(personEvents, false, ct);
    }

    public async Task PayEvents(IEnumerable<EventPerson> personEvents, bool paidAsAmipa, CancellationToken ct)
    {
        // Mark all event person as paid
        foreach (var ep in personEvents)
        {
            SetPaid(ep, true, paidAsAmipa);
        }

        await _eventsPeopleRepository.UpdateManyAsync(personEvents, ct);
        await ProcessPaidEvents(personEvents, true, ct);
    }

    /// <summary>
    /// Pagament de diverses persones a la vegada. A diferència de <see cref="PayEvents"/>, cada
    /// alumne ve amb el seu <see cref="PersonGroupCourse"/>: l'AMIPA i la matrícula són per persona,
    /// i així només cal una escriptura per taula en lloc d'una per alumne.
    /// </summary>
    public async Task PayEventsForPeople(IEnumerable<(EventPerson EventPerson, PersonGroupCourse Pgc)> people,
        CancellationToken ct)
    {
        List<EventPerson> personEvents = new();
        List<PersonGroupCourse> changedPgcs = new();

        foreach (var (ep, pgc) in people)
        {
            // L'ordre importa: es paga com a AMIPA si ja n'era, encara que l'esdeveniment que
            // s'està pagant sigui justament el que el farà soci.
            SetPaid(ep, true, pgc.Amipa);
            personEvents.Add(ep);

            if (ApplyEnrollmentAndAmipa(ep, pgc, true)) changedPgcs.Add(pgc);
        }

        await _eventsPeopleRepository.UpdateManyAsync(personEvents, ct);
        if (changedPgcs.Count > 0) await _personGroupCourseRepository.UpdateManyAsync(changedPgcs, ct);
    }

    private static void SetPaid(EventPerson ep, bool paid, bool paidAsAmipa)
    {
        ep.Paid = paid;
        ep.DatePaid = paid ? DateTimeOffset.UtcNow : null;
        ep.PaidAsAmipa = paid && paidAsAmipa;
    }

    private async Task ProcessPaidEvents(IEnumerable<EventPerson> personEvents, bool paid, CancellationToken ct)
    {
        EventPerson? e = personEvents.FirstOrDefault(x => x.Event.Enrollment);
        if (e != null)
        {
            await ProcessPaidEvent(e, paid, ct);
        }

        e = personEvents.FirstOrDefault(x => x.Event.Amipa);
        if (e != null)
        {
            await ProcessPaidEvent(e, paid, ct);
        }
    }

    private async Task ProcessPaidEvent(EventPerson ep, bool paid, CancellationToken ct)
    {
        if (!ep.Event.Enrollment && !ep.Event.Amipa) return;

        PersonGroupCourse? pgc =
            await _personGroupCourseRepository.GetCoursePersonGroupById(ep.PersonId, ep.Event.CourseId, ct);
        if (pgc == null) return;

        if (!ApplyEnrollmentAndAmipa(ep, pgc, paid)) return;

        await _personGroupCourseRepository.UpdateAsync(pgc, ct);
    }

    /// <summary>
    /// Trasllada el pagament d'un esdeveniment de matrícula o d'AMIPA al curs de la persona.
    /// Retorna si ha canviat res, per no desar de franc.
    /// </summary>
    private static bool ApplyEnrollmentAndAmipa(EventPerson ep, PersonGroupCourse pgc, bool paid)
    {
        if (!ep.Event.Enrollment && !ep.Event.Amipa) return false;

        // enrollment
        if (ep.Event.Enrollment)
        {
            pgc.EnrollmentEvent = paid ? ep.Event : null;
            pgc.EnrollmentEventId = paid ? ep.Event.Id : null;
            pgc.Enrolled = paid;
            pgc.EnrolledDate = paid ? DateTimeOffset.UtcNow : null;
        }

        // amipa
        if (ep.Event.Amipa)
        {
            pgc.Amipa = paid;
            pgc.AmipaDate = paid ? DateTimeOffset.UtcNow : null;
        }

        return true;
    }
}
