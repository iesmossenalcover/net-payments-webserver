namespace Domain.Orders;

public class Order
{
    public long Id { get; set; }
    public DateTime CreationDate { get; set; } = default!;
    public OrderStatus Status { get; set; } = OrderStatus.Pending;

}