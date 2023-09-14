namespace Infrastructure;

public class IntoInfoLogStore : Domain.Services.ILogStore
{
    public Task<Domain.Entities.Tasks.Log?> Read(Domain.Entities.Tasks.LogStoreInfo logData)
    {
        if (logData.Type != Domain.Entities.Tasks.StoreType.INTO_INFO)
        {
            throw new ArgumentException("LogStoreInfo is not of type INTO_INFO");
        }

        var log = new Domain.Entities.Tasks.Log(logData.Info);

        return Task.FromResult<Domain.Entities.Tasks.Log?>(log);
    }

    public Task<Domain.Entities.Tasks.LogStoreInfo?> Save(Domain.Entities.Tasks.Log log)
    {
        var logStoreInfo = new Domain.Entities.Tasks.LogStoreInfo()
        {
            Type = Domain.Entities.Tasks.StoreType.INTO_INFO,
            Info = log.Data,
        };
        return Task.FromResult<Domain.Entities.Tasks.LogStoreInfo?>(logStoreInfo);
    }
}