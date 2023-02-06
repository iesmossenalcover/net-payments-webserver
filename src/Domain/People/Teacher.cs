namespace Domain.People;

public class Teacher
{
    public long Id { get; set; }
    public long PersonId { get; set; }
    public Person Person { get; set; } = default!;
}