using Domain.Entities.Events;
using Domain.Entities.People;
using Domain.Services;

namespace Domain.Behaviours;

public class EventPersonProcessingService
{
    #region IOC
    private readonly Domain.Services.IPersonGroupCourseRepository _personGroupCourseRepository;

    public EventPersonProcessingService(IPersonGroupCourseRepository personGroupCourseRepository)
    {
        _personGroupCourseRepository = personGroupCourseRepository;
    }

    #endregion

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

        PersonGroupCourse? pgc = await _personGroupCourseRepository.GetCoursePersonGroupById(ep.PersonId, ep.Event.CourseId, ct);
        if (pgc == null) return;

        // enrollment
        if (ep.Event.Enrollment)
        {
            pgc.EnrollmentEvent = paid ? ep.Event : null;
            pgc.Enrolled = paid;
        }

        // amipa
        if (ep.Event.Amipa)
        {
            pgc.Amipa = paid;
        }
        await _personGroupCourseRepository.UpdateAsync(pgc, ct);
    }
}