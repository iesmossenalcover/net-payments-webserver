namespace Domain.Entities.People;

public class Course : Entity
{
    public string Name { get; set;} = default!;
    public DateTimeOffset StartDate { get; set; } = default!;
    public DateTimeOffset EndDate { get; set; } = default!;
    public bool Active { get; set; } = false;
}