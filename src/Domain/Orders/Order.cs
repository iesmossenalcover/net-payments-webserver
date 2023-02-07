namespace Domain.Orders;

public class Order
{
    public long Id { get; set; }
    public DateTime Created { get; set; } = default!;
    public OrderStatus Status { get; set; } = OrderStatus.Pending;

}