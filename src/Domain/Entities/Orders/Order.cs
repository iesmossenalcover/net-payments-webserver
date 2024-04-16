using Domain.Entities.People;

namespace Domain.Entities.Orders;

public class Order : Entity
{
    public string Code { get; set;} = default!;
    public decimal Amount { get; set;}
    public DateTimeOffset Created { get; set; } = default!;
    public DateTimeOffset PaidDate { get; set; } = default!;
    public OrderStatus Status { get; set; } = OrderStatus.Pending;

    public long PersonId { get; set;}
    public Person Person {get;set;} = default!;
}