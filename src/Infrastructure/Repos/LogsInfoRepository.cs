using Domain.Entities.Logs;

namespace Infrastructure.Repos;

public class LogsInfoRepository : Repository<LogStoreInfo>, Domain.Services.ILogsInfoRespository
{
    public LogsInfoRepository(AppDbContext dbContext) : base(dbContext, dbContext.LogStoreInfos) { }
}