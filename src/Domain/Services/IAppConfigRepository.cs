using Domain.Entities.Configuration;

namespace Domain.Services;

public interface IAppConfigRepository : IRepository<AppConfig>
{
    Task<AppConfig> GetAsync(CancellationToken ct);
}