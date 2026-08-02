using MediatR;
using NovaLearn.Application.Common.Errors;
using NovaLearn.Application.Common.Interfaces;
using NovaLearn.Application.Features.Assessments.Common;
using NovaLearn.Domain.Assessments;
using NovaLearn.Domain.Enrollments;
using NovaLearn.Shared.Results;

namespace NovaLearn.Application.Features.Assessments.SubmitAssignment;

public sealed class SubmitAssignmentCommandHandler(
    IAssessmentRepository assessments,
    IEnrollmentRepository enrollments,
    IUnitOfWork unitOfWork,
    ICurrentUser currentUser,
    IDateTimeProvider dateTime)
    : IRequestHandler<SubmitAssignmentCommand, Result<SubmissionDto>>
{
    public async Task<Result<SubmissionDto>> Handle(
        SubmitAssignmentCommand request, CancellationToken cancellationToken)
    {
        if (currentUser.UserId is not { } studentId)
        {
            return Result.Failure<SubmissionDto>(AssessmentErrors.Unauthenticated);
        }

        Assignment? assignment = await assessments.GetAssignmentAsync(request.AssignmentId, cancellationToken);
        if (assignment is null)
        {
            return Result.Failure<SubmissionDto>(AssessmentErrors.AssignmentNotFound);
        }

        if (assignment.Status != AssessmentStatus.Published)
        {
            return Result.Failure<SubmissionDto>(AssessmentErrors.AssignmentNotPublished);
        }

        // Enrolment is the gate, not role: a lecturer enrolled as a learner elsewhere still
        // submits as a learner, and a learner who dropped the course no longer can.
        Enrollment? enrollment =
            await enrollments.GetActiveAsync(studentId, assignment.CourseId, cancellationToken);

        if (enrollment is null)
        {
            return Result.Failure<SubmissionDto>(AssessmentErrors.NotEnrolled);
        }

        DateTimeOffset now = dateTime.UtcNow;
        if (!assignment.AcceptsSubmissionAt(now))
        {
            return Result.Failure<SubmissionDto>(AssessmentErrors.NotOpen);
        }

        bool isLate = assignment.IsLateAt(now);

        Submission? existing = await assessments.GetSubmissionForStudentAsync(
            request.AssignmentId, studentId, cancellationToken);

        if (existing is null)
        {
            Submission created = Submission.Create(
                request.AssignmentId, studentId, request.Content, request.AttachmentUrl, now, isLate);

            // Stated explicitly: BaseEntity assigns the key client-side, so an entity reached
            // only through a navigation would be tracked as Modified and save as a no-op UPDATE.
            await assessments.AddSubmissionAsync(created, cancellationToken);
            await unitOfWork.SaveChangesAsync(cancellationToken);

            return await ReloadAsync(created.Id, cancellationToken);
        }

        // Replacing the work discards any mark it already earned.
        existing.Resubmit(request.Content, request.AttachmentUrl, now, isLate);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return await ReloadAsync(existing.Id, cancellationToken);
    }

    /// <summary>Re-reads with the assignment and learner attached so the DTO projects fully.</summary>
    private async Task<Result<SubmissionDto>> ReloadAsync(Guid submissionId, CancellationToken cancellationToken)
    {
        Submission? saved = await assessments.GetSubmissionAsync(submissionId, cancellationToken);
        return saved is null
            ? Result.Failure<SubmissionDto>(AssessmentErrors.SubmissionNotFound)
            : SubmissionDto.FromEntity(saved);
    }
}
