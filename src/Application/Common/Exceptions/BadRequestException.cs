using FluentValidation.Results;

namespace Application.Common.Exceptions;

public class BadRequestException : Exception
{
    public IDictionary<string, string[]> Failures { get; }

    public BadRequestException()
        : base("One or more validation failures have occurred.")
    {
        Failures = new Dictionary<string, string[]>();
    }

    public BadRequestException(string key, string value)
        : base("One or more validation failures have occurred.")
    {
        Failures = new Dictionary<string, string[]>()
        {
            { key, new string[] { value } }
        };
    }

    public BadRequestException(List<ValidationFailure> failures)
        : this()
    {
        var failureGroups = failures
            .GroupBy(e => e.PropertyName, e => e.ErrorMessage);

        foreach (var failureGroup in failureGroups)
        {
            var propertyName = failureGroup.Key;
            var propertyFailures = failureGroup.ToArray();

            Failures.Add(propertyName, propertyFailures);
        }
    }
}