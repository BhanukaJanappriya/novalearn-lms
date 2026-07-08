namespace NovaLearn.Domain.Common;

/// <summary>
/// Implemented by aggregate roots that raise domain events. The persistence layer collects
/// and dispatches these after <c>SaveChanges</c> succeeds, then clears them.
/// </summary>
public interface IHasDomainEvents
{
    IReadOnlyCollection<IDomainEvent> DomainEvents { get; }

    void RaiseDomainEvent(IDomainEvent domainEvent);

    void ClearDomainEvents();
}
