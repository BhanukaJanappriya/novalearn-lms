using MediatR;
using NovaLearn.Application.Common.Errors;
using NovaLearn.Application.Common.Interfaces;
using NovaLearn.Application.Features.Assessments.Common;
using NovaLearn.Domain.Assessments;
using NovaLearn.Domain.Courses;
using NovaLearn.Domain.Enrollments;
using NovaLearn.Shared.Results;

namespace NovaLearn.Application.Features.Assessments.GetCourseAssignments;

public sealed class GetCourseAssignmentsQueryHandler(
    ICourseRepository courses,
    IAssessmentRepository assessments,
    IEnrollmentRepository enrollments,
    ICurrentUser currentUser,
    IDateTimeProvider dateTime)
    : IRequestHandler<GetCourseAssignmentsQuery, Result<IReadOnlyList<AssignmentDto>>>
{
    public async Task<Result<IReadOnlyList<AssignmentDto>>> Handle(
        GetCourseAssignmentsQuery request, CancellationToken cancellationToken)
    {
        if (currentUser.UserId is not { } callerId)
        {
            return Result.Failure<IReadOnlyList<AssignmentDto>>(AssessmentErrors.Unauthenticated);
        }

        Course? course = await courses.GetByIdAsync(request.CourseId, cancellationToken);
        if (course is null)
        {
            return Result.Failure<IReadOnlyList<AssignmentDto>>(AssessmentErrors.CourseNotFound);
        }

        bool isStaff = AssessmentAuthority.CheckCanManage(course, currentUser) is null;
        DateTimeOffset now = dateTime.UtcNow;

        IReadOnlyList<Assignment> assignments =
            await assessments.ListAssignmentsAsync(request.CourseId, cancellationToken);

        if (isStaff)
        {
            IReadOnlyList<SubmissionTally> tallies =
                await assessments.TallySubmissionsAsync(request.CourseId, cancellationToken);

            Dictionary<Guid, SubmissionTally> byAssignmentId = tallies.ToDictionary(t => t.AssignmentId);

            return assignments
                .Select(a => AssignmentDto.FromEntity(
                    a,
                    now,
                    submissionCount: byAssignmentId.TryGetValue(a.Id, out SubmissionTally? tally) ? tally.Total : 0,
                    gradedCount: byAssignmentId.TryGetValue(a.Id, out SubmissionTally? graded) ? graded.Graded : 0))
                .ToList();
        }

        // Learners have to be on the course to see its work at all.
        Enrollment? enrollment =
            await enrollments.GetActiveAsync(callerId, request.CourseId, cancellationToken);

        if (enrollment is null)
        {
            return Result.Failure<IReadOnlyList<AssignmentDto>>(AssessmentErrors.NotEnrolled);
        }

        // One query for the learner's whole course, rather than one per assignment.
        IReadOnlyList<Submission> mine =
            await assessments.ListSubmissionsForStudentAsync(request.CourseId, callerId, cancellationToken);

        Dictionary<Guid, Submission> mineByAssignment = mine.ToDictionary(s => s.AssignmentId);

        return assignments
            .Where(a => a.Status == AssessmentStatus.Published)
            .Select(a => AssignmentDto.FromEntity(
                a,
                now,
                mySubmission: mineByAssignment.TryGetValue(a.Id, out Submission? submission)
                    ? SubmissionDto.FromEntity(submission)
                    : null))
            .ToList();
    }
}
