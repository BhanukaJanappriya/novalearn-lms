using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NovaLearn.API.Common;
using NovaLearn.Application.Features.Content.Common;
using NovaLearn.Application.Features.Content.CreateLesson;
using NovaLearn.Application.Features.Content.CreateModule;
using NovaLearn.Application.Features.Content.DeleteLesson;
using NovaLearn.Application.Features.Content.DeleteModule;
using NovaLearn.Application.Features.Content.GetCourseContent;
using NovaLearn.Application.Features.Content.ReorderLessons;
using NovaLearn.Application.Features.Content.ReorderModules;
using NovaLearn.Application.Features.Content.UpdateLesson;
using NovaLearn.Application.Features.Content.UpdateModule;
using NovaLearn.Domain.Identity;
using NovaLearn.Shared.Results;

namespace NovaLearn.API.Features.Content;

/// <summary>
/// Course content: the modules of a course and the lessons inside them. Reading a published
/// course's content is open to any authenticated user; every mutation is restricted to lecturers
/// and admins, with the finer-grained ownership rule enforced in the handlers.
/// </summary>
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}")]
[Authorize]
public sealed class ContentController(ISender sender) : ApiControllerBase
{
    private const string ManagerRoles =
        $"{Roles.Lecturer},{Roles.Administrator},{Roles.SuperAdministrator}";

    /// <summary>Returns a course's modules with their nested lessons, in presentation order.</summary>
    [HttpGet("courses/{courseId:guid}/content")]
    [ProducesResponseType(typeof(CourseContentDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetContent(Guid courseId, CancellationToken cancellationToken) =>
        HandleResult(await sender.Send(new GetCourseContentQuery(courseId), cancellationToken));

    /// <summary>Appends a module to a course (admins any; lecturers only their own).</summary>
    [HttpPost("courses/{courseId:guid}/modules")]
    [Authorize(Roles = ManagerRoles)]
    [ProducesResponseType(typeof(ModuleDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CreateModule(
        Guid courseId, CreateModuleRequest request, CancellationToken cancellationToken)
    {
        var command = new CreateModuleCommand(courseId, request.Title, request.Description);

        Result<ModuleDto> result = await sender.Send(command, cancellationToken);
        return HandleResult(
            result,
            module => CreatedAtAction(nameof(GetContent), new { courseId }, module));
    }

    /// <summary>Updates a module (admins any; lecturers only their own courses).</summary>
    [HttpPut("modules/{moduleId:guid}")]
    [Authorize(Roles = ManagerRoles)]
    [ProducesResponseType(typeof(ModuleDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateModule(
        Guid moduleId, UpdateModuleRequest request, CancellationToken cancellationToken) =>
        HandleResult(await sender.Send(
            new UpdateModuleCommand(moduleId, request.Title, request.Description), cancellationToken));

    /// <summary>Deletes a module and its lessons (admins any; lecturers only their own courses).</summary>
    [HttpDelete("modules/{moduleId:guid}")]
    [Authorize(Roles = ManagerRoles)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteModule(Guid moduleId, CancellationToken cancellationToken) =>
        HandleResult(await sender.Send(new DeleteModuleCommand(moduleId), cancellationToken));

    /// <summary>Rewrites the order of a course's modules.</summary>
    [HttpPut("courses/{courseId:guid}/modules/order")]
    [Authorize(Roles = ManagerRoles)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ReorderModules(
        Guid courseId, ReorderRequest request, CancellationToken cancellationToken) =>
        HandleResult(await sender.Send(
            new ReorderModulesCommand(courseId, request.Ids), cancellationToken));

    /// <summary>Appends a lesson to a module (admins any; lecturers only their own courses).</summary>
    [HttpPost("modules/{moduleId:guid}/lessons")]
    [Authorize(Roles = ManagerRoles)]
    [ProducesResponseType(typeof(LessonDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CreateLesson(
        Guid moduleId, CreateLessonRequest request, CancellationToken cancellationToken)
    {
        var command = new CreateLessonCommand(
            moduleId,
            request.Title,
            request.Type,
            request.ContentUrl,
            request.TextContent,
            request.DurationMinutes,
            request.IsPreview);

        // No Location header: a lesson has no standalone GET, and the content tree it belongs
        // to is keyed by course id, which this route does not carry.
        Result<LessonDto> result = await sender.Send(command, cancellationToken);
        return HandleResult(result, lesson => StatusCode(StatusCodes.Status201Created, lesson));
    }

    /// <summary>Updates a lesson (admins any; lecturers only their own courses).</summary>
    [HttpPut("lessons/{lessonId:guid}")]
    [Authorize(Roles = ManagerRoles)]
    [ProducesResponseType(typeof(LessonDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateLesson(
        Guid lessonId, UpdateLessonRequest request, CancellationToken cancellationToken)
    {
        var command = new UpdateLessonCommand(
            lessonId,
            request.Title,
            request.Type,
            request.ContentUrl,
            request.TextContent,
            request.DurationMinutes,
            request.IsPreview);

        return HandleResult(await sender.Send(command, cancellationToken));
    }

    /// <summary>Deletes a lesson (admins any; lecturers only their own courses).</summary>
    [HttpDelete("lessons/{lessonId:guid}")]
    [Authorize(Roles = ManagerRoles)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteLesson(Guid lessonId, CancellationToken cancellationToken) =>
        HandleResult(await sender.Send(new DeleteLessonCommand(lessonId), cancellationToken));

    /// <summary>Rewrites the order of a module's lessons.</summary>
    [HttpPut("modules/{moduleId:guid}/lessons/order")]
    [Authorize(Roles = ManagerRoles)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ReorderLessons(
        Guid moduleId, ReorderRequest request, CancellationToken cancellationToken) =>
        HandleResult(await sender.Send(
            new ReorderLessonsCommand(moduleId, request.Ids), cancellationToken));
}
