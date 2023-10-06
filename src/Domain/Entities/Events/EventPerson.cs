using Domain.Entities.People;
using Domain.Entities.Orders;

namespace Domain.Entities.Events;

public class EventPerson : Entity
{
    public required uint Quantity { get; set; } = 1;
    public bool Paid { get; set; }
    public DateTimeOffset? DatePaid { get; set; }
    public bool PaidAsAmipa { get; set; } = false;

    public long PersonId { get; set; }
    public Person Person { get; set; } = default!;

    public long EventId { get; set; }
    public Event Event { get; set; } = default!;

    public long? OrderId { get; set; }
    public Order? Order { get; set; } = default!;

    public bool CanBePaid => Event.IsActive && !Paid;

    public decimal AmountPaid(Event e) => (PaidAsAmipa ? e.AmipaPrice : e.Price) * Quantity;
}