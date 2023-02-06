using Domain.People;
using Domain.Orders;

namespace Domain.Events;

public class EventPerson
{
    public long Id { get; set; }
    public bool Paid { get; set; }

    public long PersonId { get; set; }
    public Person Person { get; set; } = default!;
    
    public long EventId { get; set; }
    public Event Event { get; set; } = default!;

    public long ItemId { get; set; }
    public Item Item { get; set; } = default!;

}