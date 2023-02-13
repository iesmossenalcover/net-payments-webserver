using Domain.Entities.People;
using Domain.Entities.Orders;

namespace Domain.Entities.Events;

public class EventPerson : Entity
{
    public bool Paid { get; set; }

    public long PersonId { get; set; }
    public Person Person { get; set; } = default!;
    
    public long EventId { get; set; }
    public Event Event { get; set; } = default!;

    public long ItemId { get; set; }
    public Item Item { get; set; } = default!;

}