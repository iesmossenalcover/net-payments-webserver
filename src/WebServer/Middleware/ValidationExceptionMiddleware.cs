using FluentValidation;

namespace WebServer.Middleware;

public class ValidationExceptionHandlerMiddleware
{
    private readonly RequestDelegate _next;

    public ValidationExceptionHandlerMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task Invoke(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (ValidationException ex)
        {
            await context.Response.WriteAsJsonAsync(
                new Error(true, ex.Errors.Select(x => new ValidationError(x.PropertyName, x.ErrorMessage, x.AttemptedValue)))
            );
        }
    }
}

record Error(bool error, IEnumerable<ValidationError> errors);
record ValidationError(string propertyName, string message, object attemptedValue);