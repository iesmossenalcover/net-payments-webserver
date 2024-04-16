using Domain.Entities.Logs;
using Domain.ValueObjects;

namespace Domain.Services;

public interface ILogStore
{
    Task<LogStoreInfo> Save(Log log);
    Task<Log?> Read(LogStoreInfo logData);
}