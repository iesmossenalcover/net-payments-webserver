using Domain.Entities.Events;

namespace Domain.Entities.People;

public class PersonGroupCourse : Entity
{

    public long PersonId { get; set; }
    public Person Person { get; set; } = default!;

    public long CourseId { get; set; }
    public Course Course { get; set; } = default!;

    public long GroupId { get; set; }
    public Group Group { get; set; } = default!;

    public bool Amipa { get; set; } = false;
    public DateTimeOffset? AmipaDate { get; set; }

    public bool Enrolled { get; set; } = false;
    public DateTimeOffset? EnrolledDate { get; set; }
    public long? EnrollmentEventId { get; set; }
    public Event? EnrollmentEvent { get; set; }
    public string? SubjectsInfo { get; set; } = default!;

    public bool WalkingAuthorization { get; set; } = false;
    public DateTimeOffset? WalkingAuthorizationDate { get; set; }
    public bool TransportAuthorization { get; set; } = false;
    public DateTimeOffset? TransportAuthorizationDate { get; set; }

    public decimal PriceForEvent(Event e)
    {
        return Amipa ? e.AmipaPrice : e.Price;
    }

    public bool IsAuthorizedFor(Event e) => e.Type switch
    {
        EventType.Walking => WalkingAuthorization,
        EventType.Transport => TransportAuthorization,
        _ => true
    };

    public static string MissingAuthorizationMessage(Event e) => e.Type == EventType.Walking
        ? "Falta l'autorització de sortides a peu"
        : "Falta l'autorització de sortides amb transport";

    // The date is only set when the authorization is granted, and cleared when revoked.
    public void SetWalkingAuthorization(bool value)
    {
        if (value && !WalkingAuthorization) WalkingAuthorizationDate = DateTimeOffset.UtcNow;
        if (!value) WalkingAuthorizationDate = null;
        WalkingAuthorization = value;
    }

    public void SetTransportAuthorization(bool value)
    {
        if (value && !TransportAuthorization) TransportAuthorizationDate = DateTimeOffset.UtcNow;
        if (!value) TransportAuthorizationDate = null;
        TransportAuthorization = value;
    }
}