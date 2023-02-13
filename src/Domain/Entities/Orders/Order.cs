namespace Domain.Entities.Orders;

public class Order : Entity
{
    public DateTimeOffset Created { get; set; } = default!;
    public OrderStatus Status { get; set; } = OrderStatus.Pending;

}