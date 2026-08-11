using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using NovaLearn.API.Common;
using NovaLearn.Application.Features.Profile.Common;
using NovaLearn.Application.Features.Profile.GetMyProfile;
using NovaLearn.Application.Features.Profile.UpdateAvatar;

namespace NovaLearn.API.Features.Profile;

/// <summary>
/// The signed-in person's own profile.
///
/// No route here takes a user id. The subject is always taken from the caller's token, so a
/// person can only ever read or change their own profile, whatever they send. That is why
/// picture editing lives here rather than alongside the administrative user endpoints: an
/// administrator has no route to change somebody else's picture.
/// </summary>
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/profile")]
[Authorize]
public sealed class ProfileController(ISender sender) : ApiControllerBase
{
    /// <summary>The caller's own profile, read from storage rather than token claims.</summary>
    [HttpGet("me")]
    [ProducesResponseType(typeof(MyProfileDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Me(CancellationToken cancellationToken) =>
        HandleResult(await sender.Send(new GetMyProfileQuery(), cancellationToken));

    /// <summary>Sets or clears the caller's own picture. Send a null url to remove it.</summary>
    [HttpPut("me/avatar")]
    [ProducesResponseType(typeof(MyProfileDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> UpdateAvatar(
        UpdateAvatarRequest request, CancellationToken cancellationToken) =>
        HandleResult(await sender.Send(new UpdateMyAvatarCommand(request.AvatarUrl), cancellationToken));
}

/// <summary>Body for setting a profile picture. A null or empty url clears it.</summary>
public sealed record UpdateAvatarRequest(string? AvatarUrl);
