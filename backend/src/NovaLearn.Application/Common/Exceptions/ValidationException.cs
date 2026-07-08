using FluentValidation.Results;

namespace NovaLearn.Application.Common.Exceptions;

/// <summary>
/// Thrown by <c>ValidationBehaviour</c> when one or more validators fail. Carries a
/// field → messages map that the API's exception handler renders as an RFC 7807 problem.
/// </summary>
public sealed class ValidationException : Exception
{
    public ValidationException(IEnumerable<ValidationFailure> failures)
        : base("One or more validation failures occurred.")
    {
        Errors = failures
            .GroupBy(f => f.PropertyName, f => f.ErrorMessage)
            .ToDictionary(g => g.Key, g => g.ToArray());
    }

    public IReadOnlyDictionary<string, string[]> Errors { get; }
}
