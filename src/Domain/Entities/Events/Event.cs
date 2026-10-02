using Domain.Entities.People;

namespace Domain.Entities.Events;

public enum EventType
{
    Other = 0,
    Walking = 1,
    Transport = 2,
    Trip = 3,
}

public class Event : Entity
{
    public string Code { get; set; }  = default!;
    public string Name { get; set; } = default!;
    public string Description { get; set; } = default!;
    public string? Location { get; set; }
    
    public decimal Price { get; set; }
    public decimal AmipaPrice { get; set; }

    public required uint MaxQuantity { get; set; } = 1;

    public bool Enrollment { get; set; } = false;
    public bool Amipa { get; set; } = false;
    public EventType Type { get; set; } = EventType.Other;

    public DateTimeOffset Date { get; set; } = default!;
    public DateTimeOffset? EndDate { get; set; } = default!;

    public DateTimeOffset CreationDate { get; set; } = default!;
    public DateTimeOffset PublishDate { get; set; } = default!;
    public DateTimeOffset? UnpublishDate { get; set; } = default!;

    public long CourseId { get; set; }
    public Course Course { get; set; } = default!;

    public string? CalendarEventId { get; set; } = default!;

    public bool IsActive => (PublishDate <= DateTimeOffset.UtcNow) && (UnpublishDate.HasValue ? DateTimeOffset.UtcNow <= UnpublishDate.Value : true);
}