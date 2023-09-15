namespace Domain.ValueObjects;

public class Log
{
    public string Data { get; private set; } = string.Empty;

    public Log() { }

    public Log(string data)
    {
        Data = data;
    }

    public void Add(string data)
    {
        lock (Data)
        {
            Data += $"[{DateTimeOffset.UtcNow}] - {data}\n";
        }
    }
}