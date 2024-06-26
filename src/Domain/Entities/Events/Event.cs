using Domain.Entities.People;

namespace Domain.Entities.Events;

public class Event : Entity
{
    public string Code { get; set; }  = default!;
    public string Name { get; set; } = default!;
    public string Description { get; set; } = default!;
    
    public decimal Price { get; set; }
    public decimal AmipaPrice { get; set; }

    public required uint MaxQuantity { get; set; } = 1;

    public bool Enrollment { get; set; } = false;
    public bool Amipa { get; set; } = false;

    public DateTimeOffset Date { get; set; } = default!;

    public DateTimeOffset CreationDate { get; set; } = default!;
    public DateTimeOffset PublishDate { get; set; } = default!;
    public DateTimeOffset? UnpublishDate { get; set; } = default!;

    public long CourseId { get; set; }
    public Course Course { get; set; } = default!;

    public bool IsActive => (PublishDate <= DateTimeOffset.UtcNow) && (UnpublishDate.HasValue ? DateTimeOffset.UtcNow <= UnpublishDate.Value : true);
}