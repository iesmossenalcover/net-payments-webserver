namespace Domain.Entities.Orders;

public class Order
{
    public long Id { get; set; }
    public DateTimeOffset Created { get; set; } = default!;
    public OrderStatus Status { get; set; } = OrderStatus.Pending;

}