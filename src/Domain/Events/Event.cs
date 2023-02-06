namespace Domain.Events;

public class Event
{
    public long Id { get; set; }
    public string Code { get; set; }  = default!;
    public decimal NormalPrice { get; set; }
    public decimal AmipaPrice { get; set; }
    public DateTime CreationDate { get; set; } = default!;
    public DateTime PublishDate { get; set; } = default!;
    public DateTime UnpublishDate { get; set; } = default!;

}