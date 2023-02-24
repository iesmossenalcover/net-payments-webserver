namespace Domain.Entities.Orders;

public class Order : Entity
{
    public string Code { get; set;} = default!;
    public decimal Price { get; set;}
    public DateTimeOffset Created { get; set; } = default!;
    public OrderStatus Status { get; set; } = OrderStatus.Pending;
}