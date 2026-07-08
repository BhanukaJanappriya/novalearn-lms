using NovaLearn.Application.Common.Interfaces;

namespace NovaLearn.Infrastructure.Time;

/// <summary>System-clock implementation of <see cref="IDateTimeProvider"/>.</summary>
public sealed class DateTimeProvider : IDateTimeProvider
{
    public DateTimeOffset UtcNow => DateTimeOffset.UtcNow;
}
