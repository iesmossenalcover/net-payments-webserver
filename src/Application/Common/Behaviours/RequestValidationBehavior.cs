using FluentValidation;
using FluentValidation.Results;
using MediatR;

namespace Application.Common.Behaviors;

public class RequestValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
        where TRequest : IRequest<TResponse>
{
    private readonly IEnumerable<IValidator<TRequest>> _validators;

    public RequestValidationBehavior(IEnumerable<IValidator<TRequest>> validators)
    {
        _validators = validators;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var context = new ValidationContext<TRequest>(request);
        var errors = new List<ValidationFailure>();
        foreach (var v in _validators)
        {
            var validatoin = await v.ValidateAsync(context);
            errors.AddRange(validatoin.Errors);
        }
        
        if (errors.Count != 0)
        {
            throw new Exceptions.BadRequestException(errors);
        }

        return await next();
    }
}