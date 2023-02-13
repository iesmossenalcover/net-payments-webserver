namespace Domain.Entities.Events;

public class Event : Entity
{
    public string Code { get; set; }  = default!;
    public decimal NormalPrice { get; set; }
    public decimal AmipaPrice { get; set; }
    public DateTimeOffset CreationDate { get; set; } = default!;
    public DateTimeOffset PublishDate { get; set; } = default!;
    public DateTimeOffset UnpublishDate { get; set; } = default!;
    public bool IsAmipa { get; set; } = false;

}