using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NovaLearn.API.Common;
using NovaLearn.Application.Features.Security.Common;
using NovaLearn.Application.Features.Security.GetActiveSessions;
using NovaLearn.Application.Features.Security.GetLockedAccounts;
using NovaLearn.Application.Features.Security.GetSecurityOverview;
using NovaLearn.Application.Features.Security.RevokeAllSessionsForUser;
using NovaLearn.Application.Features.Security.RevokeSession;
using NovaLearn.Application.Features.Security.UnlockAccount;
using NovaLearn.Domain.Identity;
using NovaLearn.Shared.Common;

namespace NovaLearn.API.Features.Admin;

/// <summary>
/// The security center: who is currently signed in, who is locked out, and the levers to act on
/// both — force a session to sign out, sign an account out everywhere, or clear a lockout.
/// Administrator only, same scope as Finance, Reports and Audit Logs rather than a lecturer-scoped
/// view.
/// </summary>
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/admin/security")]
[Authorize(Roles = $"{Roles.SuperAdministrator},{Roles.Administrator}")]
public sealed class SecurityController(ISender sender) : ApiControllerBase
{
    [HttpGet("overview")]
    [ProducesResponseType(typeof(SecurityOverview), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetOverview(CancellationToken cancellationToken) =>
        HandleResult(await sender.Send(new GetSecurityOverviewQuery(), cancellationToken));

    [HttpGet("sessions")]
    [ProducesResponseType(typeof(PagedResult<SessionRow>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetSessions(
        [FromQuery] string? search,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default) =>
        HandleResult(await sender.Send(new GetActiveSessionsQuery(search, page, pageSize), cancellationToken));

    /// <summary>Forces one session to sign out.</summary>
    [HttpPost("sessions/{id:guid}/revoke")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> RevokeSession(Guid id, CancellationToken cancellationToken) =>
        HandleResult(await sender.Send(new RevokeSessionCommand(id), cancellationToken));

    /// <summary>Signs an account out everywhere by revoking every session it currently holds.</summary>
    [HttpPost("users/{userId:guid}/revoke-sessions")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RevokeAllSessions(Guid userId, CancellationToken cancellationToken) =>
        HandleResult(await sender.Send(new RevokeAllSessionsForUserCommand(userId), cancellationToken));

    [HttpGet("locked-accounts")]
    [ProducesResponseType(typeof(PagedResult<LockedAccountRow>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetLockedAccounts(
        [FromQuery] string? search,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default) =>
        HandleResult(await sender.Send(new GetLockedAccountsQuery(search, page, pageSize), cancellationToken));

    /// <summary>Clears a lockout imposed by repeated failed sign-ins.</summary>
    [HttpPost("users/{userId:guid}/unlock")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UnlockAccount(Guid userId, CancellationToken cancellationToken) =>
        HandleResult(await sender.Send(new UnlockAccountCommand(userId), cancellationToken));
}
