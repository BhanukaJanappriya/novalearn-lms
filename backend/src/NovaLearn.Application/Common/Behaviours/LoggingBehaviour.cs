using System.Diagnostics;
using MediatR;
using Microsoft.Extensions.Logging;
using NovaLearn.Application.Common.Interfaces;

namespace NovaLearn.Application.Common.Behaviours;

/// <summary>
/// Structured request/response logging with elapsed time and the acting user. Emits a warning
/// for requests slower than <see cref="LongRunningThresholdMs"/> to surface performance issues.
/// </summary>
public sealed class LoggingBehaviour<TRequest, TResponse>(
    ILogger<LoggingBehaviour<TRequest, TResponse>> logger,
    ICurrentUser currentUser)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private const long LongRunningThresholdMs = 500;

    public async Task<TResponse> Handle(
        TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        string requestName = typeof(TRequest).Name;
        Guid? userId = currentUser.UserId;

        logger.LogInformation("Handling {RequestName} for user {UserId}", requestName, userId);

        var stopwatch = Stopwatch.StartNew();
        TResponse response = await next();
        stopwatch.Stop();

        if (stopwatch.ElapsedMilliseconds > LongRunningThresholdMs)
        {
            logger.LogWarning(
                "Long-running request {RequestName} took {ElapsedMilliseconds}ms",
                requestName, stopwatch.ElapsedMilliseconds);
        }
        else
        {
            logger.LogInformation(
                "Handled {RequestName} in {ElapsedMilliseconds}ms", requestName, stopwatch.ElapsedMilliseconds);
        }

        return response;
    }
}
