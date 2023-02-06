namespace Domain.Orders;

public class Item
{
    public long Id { get; set; }
    public long OrderId { get; set; }
    public Order Order { get; set; } = default!;
}