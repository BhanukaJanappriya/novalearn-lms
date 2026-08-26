using NovaLearn.Application.Features.AuditLogs.Common;
using NovaLearn.Domain.Audit;
using NovaLearn.Shared.Common;

namespace NovaLearn.Application.Common.Interfaces;

/// <summary>Read side of the audit trail.</summary>
public interface IAuditLogRepository
{
    /// <summary>Paged, filtered search over the audit trail, newest first. All filters are optional.</summary>
    Task<PagedResult<AuditLogRow>> SearchAsync(
        AuditCategory? category,
        Guid? actorId,
        string? search,
        DateTimeOffset? fromUtc,
        DateTimeOffset? toUtc,
        int page,
        int pageSize,
        CancellationToken cancellationToken);
}
