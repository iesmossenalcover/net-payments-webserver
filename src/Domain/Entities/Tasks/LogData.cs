namespace Domain.Entities.Tasks;

public enum StoreType
{
    INTO_INFO,
}

public class LogStoreInfo : Entity
{
    public required StoreType Type { get; set; }
    public required string Info { get; set; }
}

public class Log
{
    public string Data { get; private set; } = string.Empty;

    public Log() {}

    public Log(string data)
    {
        Data = data;
    }

    public void Add(string data)
    {
        Data += $"[{new DateTimeOffset()}] - {data}\n";
    }
}