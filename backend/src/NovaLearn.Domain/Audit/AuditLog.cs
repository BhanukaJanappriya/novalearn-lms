using NovaLearn.Domain.Common;
using NovaLearn.Domain.Identity;

namespace NovaLearn.Domain.Audit;

/// <summary>
/// One entry in the platform's audit trail: who did what, to which record, and when. Written as a
/// side effect of a curated set of the platform's most sensitive commands (role changes, account
/// activation, course and department deletion, settings edits, refunds) rather than every command
/// that exists — an audit trail an admin can actually read end to end, not a firehose of every
/// click.
/// </summary>
public sealed class AuditLog : BaseEntity
{
    public const int ActionMaxLength = 200;
    public const int DetailsMaxLength = 1000;

    private AuditLog() { } // EF Core

    public Guid ActorId { get; private set; }

    /// <summary>Optional navigation to the account that performed the action (for read projections).</summary>
    public ApplicationUser? Actor { get; private set; }

    public AuditCategory Category { get; private set; }

    /// <summary>Short human label, e.g. "Deactivated account".</summary>
    public string Action { get; private set; } = null!;

    /// <summary>What made this instance specific, e.g. the account or amount involved.</summary>
    public string? Details { get; private set; }

    /// <summary>The kind of record this action was about, e.g. "User", "Course". Optional.</summary>
    public string? EntityType { get; private set; }

    public Guid? EntityId { get; private set; }

    public static AuditLog Create(
        Guid actorId,
        AuditCategory category,
        string action,
        string? details,
        string? entityType,
        Guid? entityId,
        DateTimeOffset now)
    {
        if (string.IsNullOrWhiteSpace(action))
        {
            throw new ArgumentException("An audit entry needs an action.", nameof(action));
        }

        return new AuditLog
        {
            ActorId = actorId,
            Category = category,
            Action = action.Trim(),
            Details = string.IsNullOrWhiteSpace(details) ? null : details.Trim(),
            EntityType = string.IsNullOrWhiteSpace(entityType) ? null : entityType.Trim(),
            EntityId = entityId,
            CreatedAtUtc = now
        };
    }
}
