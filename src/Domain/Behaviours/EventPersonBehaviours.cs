using Domain.Entities.Events;
using Domain.Entities.People;
using Domain.Services;

namespace Domain.Behaviours;

public class EventPersonProcessingService
{
    #region IOC

    private readonly IPersonGroupCourseRepository _personGroupCourseRepository;
    private readonly IEventsPeopleRespository _eventsPeopleRepository;

    public EventPersonProcessingService(IPersonGroupCourseRepository personGroupCourseRepository,
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
            ep.Paid = false;
            ep.DatePaid = null;
            ep.PaidAsAmipa = false;
        }

        await _eventsPeopleRepository.UpdateManyAsync(personEvents, ct);
        await ProcessPaidEvents(personEvents, false, ct);
    }

    public async Task PayEvents(IEnumerable<EventPerson> personEvents, bool paidAsAmipa, CancellationToken ct)
    {
        // Mark all event person as paid
        foreach (var ep in personEvents)
        {
            ep.Paid = true;
            ep.DatePaid = DateTimeOffset.UtcNow;
            ep.PaidAsAmipa = paidAsAmipa;
        }

        await _eventsPeopleRepository.UpdateManyAsync(personEvents, ct);
        await ProcessPaidEvents(personEvents, true, ct);
    }

    public async Task ProcessPaidEvents(IEnumerable<EventPerson> personEvents, bool paid, CancellationToken ct)
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

    public async Task ProcessPaidEvent(EventPerson ep, bool paid, CancellationToken ct)
    {
        if (!ep.Event.Enrollment && !ep.Event.Amipa) return;

        PersonGroupCourse? pgc =
            await _personGroupCourseRepository.GetCoursePersonGroupById(ep.PersonId, ep.Event.CourseId, ct);
        if (pgc == null) return;

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

        await _personGroupCourseRepository.UpdateAsync(pgc, ct);
    }
}