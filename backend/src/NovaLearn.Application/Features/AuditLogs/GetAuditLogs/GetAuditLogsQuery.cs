using MediatR;
using NovaLearn.Application.Common.Errors;
using NovaLearn.Application.Common.Interfaces;
using NovaLearn.Application.Features.AuditLogs.Common;
using NovaLearn.Domain.Audit;
using NovaLearn.Shared.Common;
using NovaLearn.Shared.Results;

namespace NovaLearn.Application.Features.AuditLogs.GetAuditLogs;

/// <summary>The audit trail: every logged action, paged and optionally filtered. Staff only.</summary>
public sealed record GetAuditLogsQuery(
    AuditCategory? Category,
    Guid? ActorId,
    string? Search,
    DateTimeOffset? FromUtc,
    DateTimeOffset? ToUtc,
    int Page,
    int PageSize) : IRequest<Result<PagedResult<AuditLogRow>>>;

public sealed class GetAuditLogsQueryHandler(IAuditLogRepository auditLogs, ICurrentUser currentUser)
    : IRequestHandler<GetAuditLogsQuery, Result<PagedResult<AuditLogRow>>>
{
    public async Task<Result<PagedResult<AuditLogRow>>> Handle(
        GetAuditLogsQuery request, CancellationToken cancellationToken)
    {
        if (!AuditLogAuthority.IsStaff(currentUser))
        {
            return Result.Failure<PagedResult<AuditLogRow>>(AuditErrors.StaffOnly);
        }

        PagedResult<AuditLogRow> result = await auditLogs.SearchAsync(
            request.Category,
            request.ActorId,
            request.Search,
            request.FromUtc,
            request.ToUtc,
            request.Page,
            request.PageSize,
            cancellationToken);

        return Result.Success(result);
    }
}
