namespace Domain.ValueObjects;

public class Log
{
    // El lock ha de ser un objecte estable: Data es reassigna a cada Add, de manera que
    // cada fil bloquejaria una instància de string diferent i el lock no protegiria res.
    private readonly object _lock = new();

    public string Data { get; private set; } = string.Empty;

    public Log() { }

    public Log(string data)
    {
        Data = data;
    }

    public void Add(string data)
    {
        lock (_lock)
        {
            Data += $"[{DateTimeOffset.UtcNow}] - {data}\n";
        }
    }
}