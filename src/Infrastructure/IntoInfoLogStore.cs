using Domain.Entities.Logs;
using Domain.ValueObjects;

namespace Infrastructure;

public class IntoInfoLogStore : Domain.Services.ILogStore
{
    public Task<Log?> Read(LogStoreInfo logData)
    {
        if (logData.Type != StoreType.INTO_INFO)
        {
            throw new ArgumentException("LogStoreInfo is not of type INTO_INFO");
        }

        var log = new Log(logData.Info);

        return Task.FromResult<Log?>(log);
    }

    public Task<LogStoreInfo> Save(Log log)
    {
        var logStoreInfo = new LogStoreInfo()
        {
            Type = StoreType.INTO_INFO,
            Info = log.Data,
        };
        return Task.FromResult(logStoreInfo);
    }
}