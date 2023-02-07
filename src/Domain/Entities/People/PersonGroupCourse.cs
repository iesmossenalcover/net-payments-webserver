namespace Domain.Entities.People;

public class PersonGroupCourse
{
    public long Id { get; set; }

    public long PersonId { get; set; }
    public Person Person { get; set; } = default!;
    
    public long CourseId { get; set; }
    public Course Course { get; set; } = default!;

    public long GroupId { get; set; }
    public Group Group { get; set; } = default!;
}