using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using NovaLearn.API.Common;
using NovaLearn.Application.Features.Assessments.Common;
using NovaLearn.Application.Features.Assessments.CreateAssignment;
using NovaLearn.Application.Features.Assessments.DeleteAssignment;
using NovaLearn.Application.Features.Assessments.GetAssignmentSubmissions;
using NovaLearn.Application.Features.Assessments.GetCourseAssignments;
using NovaLearn.Application.Features.Assessments.GetGradebook;
using NovaLearn.Application.Features.Assessments.GradeSubmission;
using NovaLearn.Application.Features.Assessments.SubmitAssignment;
using NovaLearn.Application.Features.Assessments.UpdateAssignment;
using NovaLearn.Domain.Identity;
using NovaLearn.Shared.Results;

namespace NovaLearn.API.Features.Assessments;

/// <summary>
/// Assignments, submissions and the gradebook. Routes span the <c>courses</c>,
/// <c>assignments</c> and <c>submissions</c> segments, so each action declares its own.
///
/// Authoring endpoints are gated to lecturers and admins here, with the finer-grained
/// "must own the course" rule enforced in the handlers. Submitting is open to any authenticated
/// caller because the real gate is enrolment, not role.
/// </summary>
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}")]
[Authorize]
public sealed class AssessmentsController(ISender sender) : ApiControllerBase
{
    private const string ManagerRoles =
        $"{Roles.Lecturer},{Roles.Administrator},{Roles.SuperAdministrator}";

    /// <summary>Lists a course's assignments (staff see drafts and tallies; learners see their own work).</summary>
    [HttpGet("courses/{courseId:guid}/assignments")]
    [ProducesResponseType(typeof(IReadOnlyList<AssignmentDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ListAssignments(Guid courseId, CancellationToken cancellationToken) =>
        HandleResult(await sender.Send(new GetCourseAssignmentsQuery(courseId), cancellationToken));

    /// <summary>Creates an assignment on a course you own.</summary>
    [HttpPost("courses/{courseId:guid}/assignments")]
    [Authorize(Roles = ManagerRoles)]
    [ProducesResponseType(typeof(AssignmentDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CreateAssignment(
        Guid courseId, CreateAssignmentRequest request, CancellationToken cancellationToken)
    {
        var command = new CreateAssignmentCommand(
            courseId, request.Title, request.Instructions, request.DueAtUtc,
            request.MaxPoints, request.AllowLateSubmissions, request.Status);

        Result<AssignmentDto> result = await sender.Send(command, cancellationToken);

        return HandleResult(
            result,
            assignment => CreatedAtAction(
                nameof(ListAssignments), new { courseId }, assignment));
    }

    /// <summary>Edits an assignment.</summary>
    [HttpPut("assignments/{assignmentId:guid}")]
    [Authorize(Roles = ManagerRoles)]
    [ProducesResponseType(typeof(AssignmentDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateAssignment(
        Guid assignmentId, UpdateAssignmentRequest request, CancellationToken cancellationToken)
    {
        var command = new UpdateAssignmentCommand(
            assignmentId, request.Title, request.Instructions, request.DueAtUtc,
            request.MaxPoints, request.AllowLateSubmissions, request.Status);

        return HandleResult(await sender.Send(command, cancellationToken));
    }

    /// <summary>Deletes an assignment.</summary>
    [HttpDelete("assignments/{assignmentId:guid}")]
    [Authorize(Roles = ManagerRoles)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteAssignment(Guid assignmentId, CancellationToken cancellationToken) =>
        HandleResult(await sender.Send(new DeleteAssignmentCommand(assignmentId), cancellationToken));

    /// <summary>Hands work in, replacing any previous attempt. Requires an active enrolment.</summary>
    [HttpPost("assignments/{assignmentId:guid}/submissions")]
    [ProducesResponseType(typeof(SubmissionDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Submit(
        Guid assignmentId, SubmitAssignmentRequest request, CancellationToken cancellationToken) =>
        HandleResult(await sender.Send(
            new SubmitAssignmentCommand(assignmentId, request.Content, request.AttachmentUrl),
            cancellationToken));

    /// <summary>Lists every submission for an assignment (course owner or admin only).</summary>
    [HttpGet("assignments/{assignmentId:guid}/submissions")]
    [Authorize(Roles = ManagerRoles)]
    [ProducesResponseType(typeof(IReadOnlyList<SubmissionDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ListSubmissions(Guid assignmentId, CancellationToken cancellationToken) =>
        HandleResult(await sender.Send(new GetAssignmentSubmissionsQuery(assignmentId), cancellationToken));

    /// <summary>Marks a submission (course owner or admin only).</summary>
    [HttpPut("submissions/{submissionId:guid}/grade")]
    [Authorize(Roles = ManagerRoles)]
    [ProducesResponseType(typeof(SubmissionDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Grade(
        Guid submissionId, GradeSubmissionRequest request, CancellationToken cancellationToken) =>
        HandleResult(await sender.Send(
            new GradeSubmissionCommand(submissionId, request.PointsAwarded, request.Feedback),
            cancellationToken));

    /// <summary>The marking grid for a course (course owner or admin only).</summary>
    [HttpGet("courses/{courseId:guid}/gradebook")]
    [Authorize(Roles = ManagerRoles)]
    [ProducesResponseType(typeof(GradebookDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Gradebook(Guid courseId, CancellationToken cancellationToken) =>
        HandleResult(await sender.Send(new GetGradebookQuery(courseId), cancellationToken));
}
