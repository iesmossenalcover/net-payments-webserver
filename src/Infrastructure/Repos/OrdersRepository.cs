using Domain.Entities.Orders;
using Domain.Entities.People;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repos;

public class OrdersRepository : Repository<Order>, Application.Common.Services.IOrdersRepository
{
    public OrdersRepository(AppDbContext dbContext) : base(dbContext, dbContext.Orders) {}

    public async Task<Order?> GetByCodeAsync(string code, CancellationToken ct)
    {
        return await _dbSet.FirstOrDefaultAsync(x => x.Code == code, ct);
    }
}