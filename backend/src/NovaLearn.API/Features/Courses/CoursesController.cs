using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using NovaLearn.API.Common;
using NovaLearn.Application.Features.Courses.Common;
using NovaLearn.Application.Features.Courses.CreateCourse;
using NovaLearn.Application.Features.Courses.DeleteCourse;
using NovaLearn.Application.Features.Courses.GetCourses;
using NovaLearn.Domain.Identity;
using NovaLearn.Shared.Results;

namespace NovaLearn.API.Features.Courses;

/// <summary>
/// Course management. Listing is open to any authenticated user; creating and deleting are
/// restricted to lecturers and admins (a lecturer may only delete their own courses).
/// </summary>
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/courses")]
[Authorize]
public sealed class CoursesController(ISender sender) : ApiControllerBase
{
    private const string ManagerRoles =
        $"{Roles.Lecturer},{Roles.Administrator},{Roles.SuperAdministrator}";

    /// <summary>Lists all courses, newest first.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<CourseDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> List(CancellationToken cancellationToken) =>
        HandleResult(await sender.Send(new GetCoursesQuery(), cancellationToken));

    /// <summary>Creates a course owned by the caller.</summary>
    [HttpPost]
    [Authorize(Roles = ManagerRoles)]
    [ProducesResponseType(typeof(CourseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create(CreateCourseRequest request, CancellationToken cancellationToken)
    {
        var command = new CreateCourseCommand(
            request.Title, request.Code, request.Description, request.Category,
            request.Level, request.Status, request.Price, request.CoverImageUrl);

        Result<CourseDto> result = await sender.Send(command, cancellationToken);
        return HandleResult(result, course => CreatedAtAction(nameof(List), new { id = course.Id }, course));
    }

    /// <summary>Deletes a course (admins any; lecturers only their own).</summary>
    [HttpDelete("{id:guid}")]
    [Authorize(Roles = ManagerRoles)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken) =>
        HandleResult(await sender.Send(new DeleteCourseCommand(id), cancellationToken));
}
