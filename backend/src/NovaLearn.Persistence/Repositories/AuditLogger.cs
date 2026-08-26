using NovaLearn.Application.Common.Interfaces;
using NovaLearn.Domain.Audit;

namespace NovaLearn.Persistence.Repositories;

/// <summary>
/// EF Core implementation of the audit trail's write side. Self-contained by design — see the
/// remarks on <see cref="IAuditLogger"/> — so it saves the entry itself rather than trusting the
/// caller's own unit of work to still be open by the time it is called.
/// </summary>
internal sealed class AuditLogger(ApplicationDbContext context, IDateTimeProvider dateTimeProvider)
    : IAuditLogger
{
    public async Task RecordAsync(
        Guid actorId,
        AuditCategory category,
        string action,
        string? details = null,
        string? entityType = null,
        Guid? entityId = null,
        CancellationToken cancellationToken = default)
    {
        AuditLog log = AuditLog.Create(
            actorId, category, action, details, entityType, entityId, dateTimeProvider.UtcNow);

        await context.Set<AuditLog>().AddAsync(log, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
    }
}
