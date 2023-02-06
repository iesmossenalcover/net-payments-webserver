namespace Domain.People;

public class Course
{
    public long Id { get; set; }
    public string Name { get; set;} = default!;
    public DateTime StartDate { get; set; } = default!;
    public DateTime EndDate { get; set; } = default!;
    public bool Active { get; set; } = false;
}