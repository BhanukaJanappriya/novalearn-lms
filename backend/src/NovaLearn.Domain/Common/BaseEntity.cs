namespace NovaLearn.Domain.Common;

/// <summary>
/// Base for non-Identity domain entities. Provides a surrogate key, auditing and soft-delete
/// fields, an EF Core optimistic-concurrency token, and domain-event bookkeeping.
/// </summary>
public abstract class BaseEntity : IAuditable, ISoftDeletable, IHasDomainEvents
{
    private readonly List<IDomainEvent> _domainEvents = [];

    public Guid Id { get; protected set; } = Guid.NewGuid();

    // --- Auditing (IAuditable) ---
    public DateTimeOffset CreatedAtUtc { get; set; }
    public string? CreatedBy { get; set; }
    public DateTimeOffset? UpdatedAtUtc { get; set; }
    public string? UpdatedBy { get; set; }

    // --- Soft delete (ISoftDeletable) ---
    public bool IsDeleted { get; set; }
    public DateTimeOffset? DeletedAtUtc { get; set; }
    public string? DeletedBy { get; set; }

    /// <summary>Optimistic concurrency token; mapped to a PostgreSQL <c>xmin</c> system column.</summary>
    public uint Version { get; set; }

    // --- Domain events (IHasDomainEvents) ---
    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    public void RaiseDomainEvent(IDomainEvent domainEvent) => _domainEvents.Add(domainEvent);

    public void ClearDomainEvents() => _domainEvents.Clear();
}
