using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using NovaLearn.API.Common;
using NovaLearn.Application.Features.Enrollments.Common;
using NovaLearn.Application.Features.Enrollments.EnrollInCourse;
using NovaLearn.Application.Features.Enrollments.GetCourseCatalog;
using NovaLearn.Application.Features.Enrollments.GetCourseRoster;
using NovaLearn.Application.Features.Enrollments.GetMyEnrollments;
using NovaLearn.Application.Features.Enrollments.UnenrollFromCourse;
using NovaLearn.Application.Features.Enrollments.UpdateProgress;
using NovaLearn.Domain.Identity;
using NovaLearn.Shared.Common;
using NovaLearn.Shared.Results;

namespace NovaLearn.API.Features.Enrollments;

/// <summary>
/// Enrolment endpoints. The catalogue and "my enrolments" are open to any authenticated user;
/// enrolling is a student action (admins are included for testing convenience), unenrolling is
/// restricted to the owning student or an admin, and the roster to the course owner or an admin.
/// Routes span both the <c>courses</c> and <c>enrollments</c> segments, so each action declares
/// its own template beneath the shared version prefix.
/// </summary>
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}")]
[Authorize]
public sealed class EnrollmentsController(ISender sender) : ApiControllerBase
{
    private const string EnrollingRoles =
        $"{Roles.Student},{Roles.Administrator},{Roles.SuperAdministrator}";

    /// <summary>Lists published courses with optional search, category and level filters.</summary>
    [HttpGet("courses/catalog")]
    [ProducesResponseType(typeof(PagedResult<CourseCatalogDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Catalog(
        [FromQuery] CourseCatalogRequest request, CancellationToken cancellationToken)
    {
        var query = new GetCourseCatalogQuery(
            request.Search, request.Category, request.Level, request.Page, request.PageSize);

        return HandleResult(await sender.Send(query, cancellationToken));
    }

    /// <summary>Enrols the caller in a published course.</summary>
    [HttpPost("courses/{courseId:guid}/enrollments")]
    [Authorize(Roles = EnrollingRoles)]
    [ProducesResponseType(typeof(EnrollmentDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Enroll(Guid courseId, CancellationToken cancellationToken)
    {
        Result<EnrollmentDto> result =
            await sender.Send(new EnrollInCourseCommand(courseId), cancellationToken);

        return HandleResult(
            result,
            enrollment => CreatedAtAction(nameof(Mine), new { id = enrollment.Id }, enrollment));
    }

    /// <summary>Lists the caller's own enrolments, newest first.</summary>
    [HttpGet("enrollments/me")]
    [ProducesResponseType(typeof(IReadOnlyList<EnrollmentDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Mine(CancellationToken cancellationToken) =>
        HandleResult(await sender.Send(new GetMyEnrollmentsQuery(), cancellationToken));

    /// <summary>Records progress through a course (learners their own; admins any).</summary>
    [HttpPut("enrollments/{id:guid}/progress")]
    [ProducesResponseType(typeof(EnrollmentDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> UpdateProgress(
        Guid id, UpdateProgressRequest request, CancellationToken cancellationToken) =>
        HandleResult(await sender.Send(
            new UpdateProgressCommand(id, request.ProgressPercent), cancellationToken));

    /// <summary>Drops an enrolment (students their own; admins any).</summary>
    [HttpDelete("enrollments/{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Unenroll(Guid id, CancellationToken cancellationToken) =>
        HandleResult(await sender.Send(new UnenrollFromCourseCommand(id), cancellationToken));

    /// <summary>Lists the students enrolled in a course (course owner or admin only).</summary>
    [HttpGet("courses/{courseId:guid}/enrollments")]
    [ProducesResponseType(typeof(IReadOnlyList<EnrollmentDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Roster(Guid courseId, CancellationToken cancellationToken) =>
        HandleResult(await sender.Send(new GetCourseRosterQuery(courseId), cancellationToken));
}
