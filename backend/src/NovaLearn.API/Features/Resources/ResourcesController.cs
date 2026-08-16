using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;
using NovaLearn.API.Common;
using NovaLearn.Application.Features.Resources.Common;
using NovaLearn.Application.Features.Resources.DeleteResource;
using NovaLearn.Application.Features.Resources.DownloadFile;
using NovaLearn.Application.Features.Resources.GetWall;
using NovaLearn.Application.Features.Resources.PostLink;
using NovaLearn.Application.Features.Resources.UploadFile;
using NovaLearn.Domain.Identity;
using NovaLearn.Domain.Resources;
using NovaLearn.Shared.Results;

namespace NovaLearn.API.Features.Resources;

/// <summary>Request body for posting an external address.</summary>
public sealed record PostLinkRequest(string Title, string? Description, string Url, Guid? CourseId);

/// <summary>
/// The platform wall: uploaded notes and videos, and links to material hosted elsewhere.
///
/// Posting is limited to teaching staff and administrators by role here, with the finer rule
/// (you may only attach a post to a course you teach) enforced in the handlers. Reading is open to
/// any signed-in account, because what a person may see is decided per post by course membership
/// rather than by role.
/// </summary>
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/resources")]
[Authorize]
public sealed class ResourcesController(ISender sender) : ApiControllerBase
{
    private const string PosterRoles =
        $"{Roles.Lecturer},{Roles.Administrator},{Roles.SuperAdministrator}";

    /// <summary>Upper bound on an upload request, a little above the configured file cap.</summary>
    private const long MaxUploadBytes = 210L * 1024 * 1024;

    /// <summary>The wall, newest first.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<ResourceDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetWall(
        [FromQuery] Guid? courseId,
        [FromQuery] ResourceKind? kind,
        [FromQuery] string? search,
        CancellationToken cancellationToken) =>
        HandleResult(await sender.Send(new GetWallQuery(courseId, kind, search), cancellationToken));

    /// <summary>Posts a link: a YouTube video, a Drive document, or any other address.</summary>
    [HttpPost("links")]
    [Authorize(Roles = PosterRoles)]
    [ProducesResponseType(typeof(ResourceDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> PostLink(
        PostLinkRequest request, CancellationToken cancellationToken)
    {
        Result<ResourceDto> result = await sender.Send(
            new PostLinkResourceCommand(request.Title, request.Description, request.Url, request.CourseId),
            cancellationToken);

        return HandleResult(
            result,
            resource => CreatedAtAction(nameof(GetWall), new { }, resource));
    }

    /// <summary>
    /// Uploads a file and posts it. Multipart, so a large video streams through rather than
    /// arriving as one lump of memory.
    /// </summary>
    [HttpPost("uploads")]
    [Authorize(Roles = PosterRoles)]
    [RequestSizeLimit(MaxUploadBytes)]
    [ProducesResponseType(typeof(ResourceDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status413PayloadTooLarge)]
    public async Task<IActionResult> Upload(
        [FromForm] IFormFile file,
        [FromForm] string title,
        [FromForm] string? description,
        [FromForm] Guid? courseId,
        CancellationToken cancellationToken)
    {
        if (file is null || file.Length == 0)
        {
            return BadRequest(new ProblemDetails
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "resource.empty_file",
                Detail = "The uploaded file is empty."
            });
        }

        await using Stream content = file.OpenReadStream();

        Result<ResourceDto> result = await sender.Send(
            new UploadResourceCommand(
                title, description, courseId, file.FileName, file.Length, content),
            cancellationToken);

        return HandleResult(
            result,
            resource => CreatedAtAction(nameof(GetWall), new { }, resource));
    }

    /// <summary>
    /// Streams an uploaded file to someone allowed to see it.
    ///
    /// Range requests are enabled so a video can be scrubbed rather than downloaded whole. Only
    /// types we actively want previewed are served inline; everything else is a download, so a
    /// file the browser might interpret cannot run in this origin.
    /// </summary>
    [HttpGet("{id:guid}/file")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetFile(Guid id, CancellationToken cancellationToken)
    {
        Result<ResourceFile> result = await sender.Send(
            new GetResourceFileQuery(id), cancellationToken);

        return HandleResult(result, file =>
        {
            Response.Headers[HeaderNames.XContentTypeOptions] = "nosniff";

            if (!file.Inline)
            {
                return File(file.Content, file.ContentType, file.FileName, enableRangeProcessing: true);
            }

            Response.Headers[HeaderNames.ContentDisposition] =
                new ContentDispositionHeaderValue("inline")
                {
                    FileNameStar = file.FileName
                }.ToString();

            return File(file.Content, file.ContentType, enableRangeProcessing: true);
        });
    }

    /// <summary>Removes a post. The person who posted it, or an administrator.</summary>
    [HttpDelete("{id:guid}")]
    [Authorize(Roles = PosterRoles)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken) =>
        HandleResult(await sender.Send(new DeleteResourceCommand(id), cancellationToken));
}
