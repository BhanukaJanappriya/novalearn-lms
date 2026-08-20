using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NovaLearn.API.Common;
using NovaLearn.Application.Features.Settings.Common;
using NovaLearn.Application.Features.Settings.GetSettings;
using NovaLearn.Application.Features.Settings.UpdateSettings;
using NovaLearn.Domain.Identity;
using NovaLearn.Shared.Results;

namespace NovaLearn.API.Features.Admin;

/// <summary>Body for editing platform settings.</summary>
public sealed record UpdateSettingsRequest(
    string SiteName,
    string SupportEmail,
    bool AllowNewRegistrations,
    bool MaintenanceModeEnabled,
    string? MaintenanceMessage,
    string DefaultCurrency,
    int MaxUploadSizeMb);

/// <summary>
/// Platform settings. Viewing is open to any administrator; editing is restricted to a super
/// administrator inside the command handler, since these switches — registration, maintenance
/// mode — affect every account on the platform, not one course or one user.
/// </summary>
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/admin/settings")]
[Authorize(Roles = $"{Roles.SuperAdministrator},{Roles.Administrator}")]
public sealed class SettingsController(ISender sender) : ApiControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(PlatformSettingsDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetSettings(CancellationToken cancellationToken) =>
        HandleResult(await sender.Send(new GetSettingsQuery(), cancellationToken));

    [HttpPut]
    [ProducesResponseType(typeof(PlatformSettingsDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> UpdateSettings(
        UpdateSettingsRequest request, CancellationToken cancellationToken)
    {
        var command = new UpdateSettingsCommand(
            request.SiteName,
            request.SupportEmail,
            request.AllowNewRegistrations,
            request.MaintenanceModeEnabled,
            request.MaintenanceMessage,
            request.DefaultCurrency,
            request.MaxUploadSizeMb);

        return HandleResult(await sender.Send(command, cancellationToken));
    }
}
