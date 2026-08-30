using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using NovaLearn.API.Common;
using NovaLearn.Application.Common.Models;
using NovaLearn.Application.Features.Admin.Analytics;
using NovaLearn.Application.Features.Admin.Dashboard;
using NovaLearn.Application.Features.Admin.Users.Common;
using NovaLearn.Application.Features.Admin.Users.GetUsers;
using NovaLearn.Application.Features.Admin.Users.SetUserStatus;
using NovaLearn.Application.Features.Admin.Users.UpdateUserRoles;
using NovaLearn.Application.Features.Admin.Users.VerifyUserEmail;
using NovaLearn.Domain.Identity;
using NovaLearn.Shared.Common;
using NovaLearn.Shared.Results;

namespace NovaLearn.API.Features.Admin;

/// <summary>Administrative endpoints. Restricted to administrator roles.</summary>
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/admin")]
[Authorize(Roles = $"{Roles.SuperAdministrator},{Roles.Administrator}")]
public sealed class AdminController(ISender sender) : ApiControllerBase
{
    /// <summary>
    /// Returns the aggregate dashboard payload (KPIs, analytics, feeds, health, security).
    /// <paramref name="days"/> controls only the enrollment/completion trend charts.
    /// </summary>
    [HttpGet("dashboard")]
    [ProducesResponseType(typeof(AdminDashboardResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetDashboard(
        [FromQuery] int days = 365, CancellationToken cancellationToken = default)
    {
        Result<AdminDashboardResponse> result =
            await sender.Send(new GetAdminDashboardQuery(days), cancellationToken);

        return HandleResult(result);
    }

    /// <summary>
    /// Platform analytics for a window: trends against the previous period, course and
    /// department performance, and how marks were distributed.
    /// </summary>
    [HttpGet("analytics")]
    [ProducesResponseType(typeof(PlatformAnalytics), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetAnalytics(
        [FromQuery] int days = 30, CancellationToken cancellationToken = default) =>
        HandleResult(await sender.Send(new GetPlatformAnalyticsQuery(days), cancellationToken));

    /// <summary>Lists accounts, newest first, with optional search, role and state filters.</summary>
    [HttpGet("users")]
    [ProducesResponseType(typeof(PagedResult<AdminUserDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetUsers(
        [FromQuery] AdminUserSearchRequest request, CancellationToken cancellationToken)
    {
        var query = new GetUsersQuery(
            request.Search, request.Role, request.IsActive, request.EmailConfirmed,
            request.Page, request.PageSize);

        return HandleResult(await sender.Send(query, cancellationToken));
    }

    /// <summary>Enables or disables sign-in for an account.</summary>
    [HttpPut("users/{id:guid}/status")]
    [ProducesResponseType(typeof(AdminUserDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> SetUserStatus(
        Guid id, SetUserStatusRequest request, CancellationToken cancellationToken) =>
        HandleResult(await sender.Send(new SetUserStatusCommand(id, request.IsActive), cancellationToken));

    /// <summary>Replaces an account's roles with exactly the set supplied.</summary>
    [HttpPut("users/{id:guid}/roles")]
    [ProducesResponseType(typeof(AdminUserDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> UpdateUserRoles(
        Guid id, UpdateUserRolesRequest request, CancellationToken cancellationToken) =>
        HandleResult(await sender.Send(new UpdateUserRolesCommand(id, request.Roles), cancellationToken));

    /// <summary>Confirms an account's email on their behalf and clears any lockout.</summary>
    [HttpPost("users/{id:guid}/verify-email")]
    [ProducesResponseType(typeof(AdminUserDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> VerifyUserEmail(Guid id, CancellationToken cancellationToken) =>
        HandleResult(await sender.Send(new VerifyUserEmailCommand(id), cancellationToken));

    /// <summary>The role names that may be assigned, so the client never hardcodes them.</summary>
    [HttpGet("roles")]
    [ProducesResponseType(typeof(IReadOnlyList<string>), StatusCodes.Status200OK)]
    public IActionResult GetRoles() => Ok(Roles.All);
}
