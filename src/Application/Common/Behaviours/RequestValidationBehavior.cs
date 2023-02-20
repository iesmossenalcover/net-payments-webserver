using FluentValidation;
using FluentValidation.Results;
using MediatR;

namespace Application.Common.Behaviors;

public class RequestValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
        where TRequest : IRequest<TResponse>
        where TResponse : IResponse, new()
{
    private readonly IEnumerable<IValidator<TRequest>> _validators;

    public RequestValidationBehavior(IEnumerable<IValidator<TRequest>> validators)
    {
        _validators = validators;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var context = new ValidationContext<TRequest>(request);
        
        Dictionary<string, string[]> errors = new Dictionary<string, string[]>();
        foreach (var validator in _validators)
        {
            ValidationResult validationResult = await validator.ValidateAsync(context);
            var validationErrors = validationResult.Errors;
            var groupedErrors = validationResult.Errors.GroupBy(x => x.PropertyName);
            foreach (var e in groupedErrors)
            {
                errors.TryAdd(e.Key, e.Select(x => x.ErrorMessage).ToArray());
            }
        }

        if (errors.Any())
        {
            var r = new TResponse();
            r.SetErrors(ResponseCode.BadRequest, errors);
            return r;
        }

        return await next();
    }
}