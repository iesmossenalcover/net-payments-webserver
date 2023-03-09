using Domain.Entities.Orders;

namespace Application.Common.Services;

public interface IOrdersRepository : IRepository<Order>
{
    public Task<Order?> GetByCodeAsync(string code, CancellationToken ct);
}