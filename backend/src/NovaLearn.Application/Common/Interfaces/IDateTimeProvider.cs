namespace NovaLearn.Application.Common.Interfaces;

/// <summary>Abstracts the system clock so time-dependent use cases are deterministically testable.</summary>
public interface IDateTimeProvider
{
    DateTimeOffset UtcNow { get; }
}
