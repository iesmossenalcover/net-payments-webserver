using Application.Common.Exceptions;

namespace WebServer.Middleware;

public class ValidationExceptionHandlerMiddleware
{
    private readonly RequestDelegate _next;
    private readonly System.Text.Json.JsonSerializerOptions serializeOptions;

    public ValidationExceptionHandlerMiddleware(RequestDelegate next)
    {
        _next = next;
        serializeOptions = new System.Text.Json.JsonSerializerOptions
        {
            PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase,
            DictionaryKeyPolicy = System.Text.Json.JsonNamingPolicy.CamelCase,
            WriteIndented = true
        };
    }

    public async Task Invoke(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (BadRequestException ex)
        {

            await context.Response.WriteAsJsonAsync(new Error(true, ex.Failures), serializeOptions);
        }
    }
}

record Error(bool error, IDictionary<string, string[]> errors);