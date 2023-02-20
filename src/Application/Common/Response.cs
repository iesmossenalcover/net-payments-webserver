namespace Application.Common;

public enum ResponseCode
{
    Success = 0,
    BadRequest = 1,
    NotFound = 2,
}

public interface IResponse
{
    public void SetErrors(ResponseCode code, IDictionary<string, string[]> errors);
}

public class Response<T> : IResponse
{
    public ResponseCode Code { get; set; }
    public T? Data { get; set; }
    public IDictionary<string, string[]>? Errors { get; set; }

    public Response()
    {
    }

    public Response(T data)
    {
        Code = ResponseCode.Success;
        Data = data;
    }

    public Response(ResponseCode code, IDictionary<string, string[]> errors)
    {
        Code = code;
        Errors = errors;
    }

    public void SetErrors(ResponseCode code, IDictionary<string, string[]> errors)
    {
        Code = code;
        Errors = errors;
    }

    public static Response<T> Ok(T data)
    {
        return new Response<T>(data);
    }

    public static Response<T> Error(ResponseCode code, string message)
    {
        return new Response<T>(
            code,
            new Dictionary<string, string[]>() { { "", new string[] { message } } }
        );
    }

    public static Response<T> Error(ResponseCode code, string key, string message)
    {
        return new Response<T>(
            code,
            new Dictionary<string, string[]>() { { key, new string[] { message } } }
        );
    }
}