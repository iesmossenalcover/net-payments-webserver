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

    public bool Enrolled { get; set; } = false;
    public long? EnrollmentEventId { get; set; }
    public Event? EnrollmentEvent { get; set; }
    public string? SubjectsInfo { get; set; } = default!;

    public decimal PriceForEvent(Event e)
    {
        return Amipa ? e.AmipaPrice : e.Price;
    }
}