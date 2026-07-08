namespace NovaLearn.Domain.Common;

/// <summary>Convenience base that stamps every domain event with an id and timestamp.</summary>
public abstract record DomainEvent : IDomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();

    public DateTimeOffset OccurredOnUtc { get; } = DateTimeOffset.UtcNow;
}
