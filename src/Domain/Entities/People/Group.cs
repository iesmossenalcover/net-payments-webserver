namespace Domain.Entities.People;

public class Group
{
    public long Id { get; set; }
    public string Name { get; set; } = default!;
    public string? Description { get; set; }
    public DateTime Created { get; set; } = default!;

    public long? ParentId { get; set; }
    public Group? Parent { get; set; }
}