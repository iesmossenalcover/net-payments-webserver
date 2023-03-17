namespace Domain.Entities.People;

public class Group : Entity
{
    public string Name { get; set; } = default!;
    public string? Description { get; set; }
    public DateTimeOffset Created { get; set; } = default!;

    public int Order;

    public long? ParentId { get; set; }
    public Group? Parent { get; set; }
}