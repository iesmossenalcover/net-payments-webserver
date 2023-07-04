using Domain.Entities.Configuration;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repos;

public class AppConfigRepository : Repository<Domain.Entities.Configuration.AppConfig>, Domain.Services.IAppConfigRepository
{
    public AppConfigRepository(AppDbContext dbContext) : base(dbContext, dbContext.AppConfigs) {}

    public Task<AppConfig> GetAsync(CancellationToken ct)
    {
        return _dbSet.FirstAsync();
    }
}