using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using NovaLearn.API.Common;
using NovaLearn.Application.Features.Admin.Directory;
using NovaLearn.Domain.Identity;

namespace NovaLearn.API.Features.Admin;

/// <summary>
/// The people directory: who is registered, and how they are doing in aggregate.
///
/// Read only by design. There is no endpoint here that changes anyone, and nothing returned
/// carries account-security state or an individual academic record. Acting on an account is
/// done at /admin/users, where the authority rules live.
/// </summary>
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/admin/directory")]
[Authorize(Roles = $"{Roles.SuperAdministrator},{Roles.Administrator}")]
public sealed class DirectoryController(ISender sender) : ApiControllerBase
{
    /// <summary>Everyone holding the Student role, with their learning totals.</summary>
    [HttpGet("students")]
    [ProducesResponseType(typeof(IReadOnlyList<DirectoryEntryDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Students(
        [FromQuery] string? search, CancellationToken cancellationToken) =>
        HandleResult(await sender.Send(
            new GetDirectoryQuery(DirectoryAudience.Students, search), cancellationToken));

    /// <summary>Lecturers and teaching assistants, with what they are responsible for.</summary>
    [HttpGet("lecturers")]
    [ProducesResponseType(typeof(IReadOnlyList<DirectoryEntryDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Lecturers(
        [FromQuery] string? search, CancellationToken cancellationToken) =>
        HandleResult(await sender.Send(
            new GetDirectoryQuery(DirectoryAudience.TeachingStaff, search), cancellationToken));
}
