namespace Application.Common.Models;

public class GoogleApiResult<T>
    {
        public bool Success { get; set; }
        public string? ErrorMessage { get; set; } = null;
        public T? Data { get; set; }

        public GoogleApiResult(string message)
        {
            Success = false;
            ErrorMessage = message;
        }

        public GoogleApiResult(T data)
        {
            Success = true;
            Data = data;
        }
    }