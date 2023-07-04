using Domain.Entities.Orders;

namespace Domain.Services;

public interface IOrdersRepository : IRepository<Order>
{
    public Task<Order?> GetByCodeAsync(string code, CancellationToken ct);

    public Task<IEnumerable<Order>> GetTodayPaidOrdersAsync(CancellationToken ct);
}