using NovaLearn.Domain.Audit;

namespace NovaLearn.Application.Features.AuditLogs.Common;

/// <summary>One audit entry as an administrator sees it on the log.</summary>
public sealed record AuditLogRow(
    Guid Id,
    AuditCategory Category,
    string Action,
    string? Details,
    string? EntityType,
    Guid? EntityId,
    Guid ActorId,
    string ActorName,
    string ActorEmail,
    DateTimeOffset CreatedAtUtc);

public static class AuditLogMapper
{
    public static AuditLogRow ToRow(AuditLog log) =>
        new(
            log.Id,
            log.Category,
            log.Action,
            log.Details,
            log.EntityType,
            log.EntityId,
            log.ActorId,
            log.Actor is { } actor ? $"{actor.FirstName} {actor.LastName}".Trim() : "Unknown",
            log.Actor?.Email ?? string.Empty,
            log.CreatedAtUtc);
}
