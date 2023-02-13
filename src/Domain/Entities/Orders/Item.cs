using Domain.Entities.Orders;

namespace Domain.Entities.Orders;

public class Item : Entity
{
    public long OrderId { get; set; }
    public Order Order { get; set; } = default!;
}