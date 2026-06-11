namespace Application.Common.Models;

public class GoogleApiResult<T>
{
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; } = null;
    public T? Data { get; set; }

    public static GoogleApiResult<T> Fail(string message) => new()
    {
        Success = false,
        ErrorMessage = message,
    };

    public static GoogleApiResult<T> Ok(T data) => new()
    {
        Success = true,
        Data = data,
    };
}
