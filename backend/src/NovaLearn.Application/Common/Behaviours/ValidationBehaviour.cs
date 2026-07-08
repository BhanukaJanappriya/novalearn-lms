using FluentValidation;
using MediatR;
using ValidationException = NovaLearn.Application.Common.Exceptions.ValidationException;

namespace NovaLearn.Application.Common.Behaviours;

/// <summary>
/// MediatR pipeline step that runs every registered <see cref="IValidator{T}"/> for the request
/// before the handler executes. Aggregates failures and throws a single
/// <see cref="ValidationException"/> so handlers can assume valid input.
/// </summary>
public sealed class ValidationBehaviour<TRequest, TResponse>(IEnumerable<IValidator<TRequest>> validators)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    public async Task<TResponse> Handle(
        TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        if (!validators.Any())
        {
            return await next();
        }

        var context = new ValidationContext<TRequest>(request);

        var results = await Task.WhenAll(
            validators.Select(v => v.ValidateAsync(context, cancellationToken)));

        var failures = results
            .SelectMany(r => r.Errors)
            .Where(f => f is not null)
            .ToList();

        if (failures.Count != 0)
        {
            throw new ValidationException(failures);
        }

        return await next();
    }
}
