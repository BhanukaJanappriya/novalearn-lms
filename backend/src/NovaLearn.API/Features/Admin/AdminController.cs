using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using NovaLearn.API.Common;
using NovaLearn.Application.Features.Admin.Dashboard;
using NovaLearn.Domain.Identity;
using NovaLearn.Shared.Results;

namespace NovaLearn.API.Features.Admin;

/// <summary>Administrative endpoints. Restricted to administrator roles.</summary>
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/admin")]
[Authorize(Roles = $"{Roles.SuperAdministrator},{Roles.Administrator}")]
public sealed class AdminController(ISender sender) : ApiControllerBase
{
    /// <summary>Returns the aggregate dashboard payload (KPIs, analytics, feeds, health, security).</summary>
    [HttpGet("dashboard")]
    [ProducesResponseType(typeof(AdminDashboardResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetDashboard(CancellationToken cancellationToken)
    {
        Result<AdminDashboardResponse> result =
            await sender.Send(new GetAdminDashboardQuery(), cancellationToken);

        return HandleResult(result);
    }
}
