namespace Domain.Entities.Events;

public class Event : Entity
{
    public string Code { get; set; }  = default!;
    public string Name { get; set; } = default!;
    public decimal Price { get; set; }
    public decimal AmipaPrice { get; set; }
    public DateTimeOffset CreationDate { get; set; } = default!;
    public DateTimeOffset PublishDate { get; set; } = default!;
    public DateTimeOffset? UnpublishDate { get; set; } = default!;

}