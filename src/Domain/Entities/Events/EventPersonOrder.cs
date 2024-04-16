using Domain.Entities.Orders;

namespace Domain.Entities.Events;

public class EventPersonOrder : Entity
{
    public required long EventPersonId { get; set; }
    public required EventPerson EventPerson { get; set; }

    public required long OrderId { get; set; }
    public required Order Order { get; set; }

    public required uint Quantity { get; set; }
}