using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NovaLearn.API.Common;
using NovaLearn.Application.Features.AuditLogs.Common;
using NovaLearn.Application.Features.AuditLogs.GetAuditLogs;
using NovaLearn.Domain.Audit;
using NovaLearn.Domain.Identity;
using NovaLearn.Shared.Common;

namespace NovaLearn.API.Features.Admin;

/// <summary>
/// The platform's audit trail: a curated set of the most sensitive admin actions (role and status
/// changes, course and department deletion, settings edits, refunds), each logged as it happens.
/// Administrator only — same scope as Finance, Reports and Support, not a lecturer-scoped view.
/// </summary>
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/admin/audit-logs")]
[Authorize(Roles = $"{Roles.SuperAdministrator},{Roles.Administrator}")]
public sealed class AuditLogsController(ISender sender) : ApiControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<AuditLogRow>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Get(
        [FromQuery] AuditCategory? category,
        [FromQuery] Guid? actorId,
        [FromQuery] string? search,
        [FromQuery] DateTimeOffset? fromUtc,
        [FromQuery] DateTimeOffset? toUtc,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default) =>
        HandleResult(await sender.Send(
            new GetAuditLogsQuery(category, actorId, search, fromUtc, toUtc, page, pageSize),
            cancellationToken));
}
