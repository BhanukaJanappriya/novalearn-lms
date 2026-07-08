namespace NovaLearn.Domain.Common;

/// <summary>
/// Entities that carry creation/modification provenance. Populated automatically by the
/// persistence layer's auditing interceptor — never set these by hand in use cases.
/// </summary>
public interface IAuditable
{
    DateTimeOffset CreatedAtUtc { get; set; }

    string? CreatedBy { get; set; }

    DateTimeOffset? UpdatedAtUtc { get; set; }

    string? UpdatedBy { get; set; }
}

/// <summary>
/// Entities that are logically (not physically) removed. The persistence layer applies a
/// global query filter so soft-deleted rows are excluded from normal queries.
/// </summary>
public interface ISoftDeletable
{
    bool IsDeleted { get; set; }

    DateTimeOffset? DeletedAtUtc { get; set; }

    string? DeletedBy { get; set; }
}
