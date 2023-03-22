using Domain.Entities.Configuration;

namespace Application.Common.Services;

public interface IAppConfigRepository : IRepository<AppConfig>
{
    Task<AppConfig> GetAsync(CancellationToken ct);
}