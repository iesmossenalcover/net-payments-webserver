using Domain.Entities.Orders;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repos;

public class OrdersRepository : Repository<Order>, Domain.Services.IOrdersRepository
{
    public OrdersRepository(AppDbContext dbContext) : base(dbContext, dbContext.Orders) {}

    public async Task<Order?> GetByCodeAsync(string code, CancellationToken ct)
    {
        return await _dbSet.FirstOrDefaultAsync(x => x.Code == code, ct);
    }

    public async Task<IEnumerable<Order>> GetTodayPaidOrdersAsync(CancellationToken ct)
    {
        return await _dbSet.Where(x => x.Status == OrderStatus.Paid && x.PaidDate.Date == DateTimeOffset.UtcNow.Date).ToListAsync(ct);

    }
}