namespace Domain.Entities.People;

public class Course
{
    public long Id { get; set; }
    public string Name { get; set;} = default!;
    public DateTimeOffset StartDate { get; set; } = default!;
    public DateTimeOffset EndDate { get; set; } = default!;
    public bool Active { get; set; } = false;
}