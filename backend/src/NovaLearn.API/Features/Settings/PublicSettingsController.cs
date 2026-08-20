using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NovaLearn.API.Common;
using NovaLearn.Application.Features.Settings.Common;
using NovaLearn.Application.Features.Settings.GetPublicSettings;

namespace NovaLearn.API.Features.Settings;

/// <summary>
/// The handful of settings an anonymous visitor may see: branding, and whether the platform is
/// presently in maintenance. Open to anyone, signed in or not — the sign-in page needs the site
/// name before anyone has signed in, and the maintenance banner has to render for the very
/// visitors maintenance mode is blocking.
/// </summary>
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/settings")]
[AllowAnonymous]
public sealed class PublicSettingsController(ISender sender) : ApiControllerBase
{
    [HttpGet("public")]
    [ProducesResponseType(typeof(PublicSettingsDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPublicSettings(CancellationToken cancellationToken) =>
        HandleResult(await sender.Send(new GetPublicSettingsQuery(), cancellationToken));
}
