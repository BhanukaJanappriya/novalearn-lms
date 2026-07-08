using MediatR;

namespace NovaLearn.Domain.Common;

/// <summary>
/// Marker for something that has happened in the domain and may trigger side effects.
/// Extends <see cref="INotification"/> so events can be dispatched via MediatR after
/// the owning aggregate is persisted.
/// </summary>
public interface IDomainEvent : INotification
{
    Guid EventId { get; }

    DateTimeOffset OccurredOnUtc { get; }
}
