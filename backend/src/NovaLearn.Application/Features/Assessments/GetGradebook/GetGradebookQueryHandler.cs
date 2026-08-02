using MediatR;
using NovaLearn.Application.Common.Errors;
using NovaLearn.Application.Common.Interfaces;
using NovaLearn.Application.Features.Assessments.Common;
using NovaLearn.Domain.Assessments;
using NovaLearn.Domain.Courses;
using NovaLearn.Domain.Enrollments;
using NovaLearn.Shared.Results;

namespace NovaLearn.Application.Features.Assessments.GetGradebook;

/// <summary>
/// Assembles the marking grid from the course roster and every submission on the course.
/// Only published assignments are columns: draft work is not something a learner could have
/// submitted, so scoring them would misreport everyone as missing.
/// </summary>
public sealed class GetGradebookQueryHandler(
    ICourseRepository courses,
    IAssessmentRepository assessments,
    IEnrollmentRepository enrollments,
    ICurrentUser currentUser)
    : IRequestHandler<GetGradebookQuery, Result<GradebookDto>>
{
    private const string StatusMissing = "Missing";
    private const string StatusSubmitted = "Submitted";
    private const string StatusGraded = "Graded";

    public async Task<Result<GradebookDto>> Handle(
        GetGradebookQuery request, CancellationToken cancellationToken)
    {
        Course? course = await courses.GetByIdAsync(request.CourseId, cancellationToken);
        if (course is null)
        {
            return Result.Failure<GradebookDto>(AssessmentErrors.CourseNotFound);
        }

        if (AssessmentAuthority.CheckCanManage(course, currentUser) is { } denied)
        {
            return Result.Failure<GradebookDto>(denied);
        }

        List<Assignment> published = (await assessments.ListAssignmentsAsync(request.CourseId, cancellationToken))
            .Where(a => a.Status == AssessmentStatus.Published)
            .ToList();

        IReadOnlyList<Enrollment> roster =
            await enrollments.ListForCourseAsync(request.CourseId, cancellationToken);

        List<Enrollment> active = roster.Where(e => e.Status != EnrollmentStatus.Dropped).ToList();

        // One pass over the submissions for the whole course, keyed for O(1) cell lookup.
        var submissions = new List<Submission>();
        foreach (Assignment assignment in published)
        {
            submissions.AddRange(await assessments.ListSubmissionsAsync(assignment.Id, cancellationToken));
        }

        Dictionary<(Guid AssignmentId, Guid StudentId), Submission> byCell =
            submissions.ToDictionary(s => (s.AssignmentId, s.StudentId));

        List<GradebookRowDto> rows = active
            .Select(enrollment => BuildRow(enrollment, published, byCell))
            .OrderBy(r => r.StudentName)
            .ToList();

        List<GradebookColumnDto> columns = published
            .Select(a => BuildColumn(a, submissions))
            .ToList();

        return new GradebookDto(
            course.Id,
            course.Title,
            course.Code,
            published.Sum(a => a.MaxPoints),
            columns,
            rows,
            BuildSummary(rows, columns, submissions));
    }

    private static GradebookRowDto BuildRow(
        Enrollment enrollment,
        IReadOnlyList<Assignment> assignments,
        IReadOnlyDictionary<(Guid, Guid), Submission> byCell)
    {
        var cells = new List<GradebookCellDto>(assignments.Count);
        int awarded = 0;
        int gradedOutOf = 0;
        int missing = 0;

        foreach (Assignment assignment in assignments)
        {
            if (!byCell.TryGetValue((assignment.Id, enrollment.StudentId), out Submission? submission))
            {
                missing++;
                cells.Add(new GradebookCellDto(assignment.Id, null, StatusMissing, null, false));
                continue;
            }

            bool isGraded = submission.Status == SubmissionStatus.Graded;
            if (isGraded)
            {
                awarded += submission.PointsAwarded ?? 0;

                // Only marked work counts towards the denominator, so a learner's percentage
                // reflects what has actually been assessed rather than what is outstanding.
                gradedOutOf += assignment.MaxPoints;
            }

            cells.Add(new GradebookCellDto(
                assignment.Id,
                submission.Id,
                isGraded ? StatusGraded : StatusSubmitted,
                submission.PointsAwarded,
                submission.IsLate));
        }

        return new GradebookRowDto(
            enrollment.StudentId,
            enrollment.Student?.FullName ?? "Unknown",
            enrollment.Student?.Email ?? string.Empty,
            cells,
            awarded,
            gradedOutOf,
            gradedOutOf == 0 ? null : Math.Round(awarded * 100.0 / gradedOutOf, 1),
            missing);
    }

    private static GradebookColumnDto BuildColumn(Assignment assignment, IReadOnlyList<Submission> submissions)
    {
        List<Submission> mine = submissions.Where(s => s.AssignmentId == assignment.Id).ToList();
        List<Submission> graded = mine.Where(s => s.Status == SubmissionStatus.Graded).ToList();

        return new GradebookColumnDto(
            assignment.Id,
            assignment.Title,
            assignment.MaxPoints,
            assignment.DueAtUtc,
            mine.Count,
            graded.Count,
            graded.Count == 0 ? null : Math.Round(graded.Average(s => s.PointsAwarded ?? 0), 1));
    }

    private static GradebookSummaryDto BuildSummary(
        IReadOnlyList<GradebookRowDto> rows,
        IReadOnlyList<GradebookColumnDto> columns,
        IReadOnlyList<Submission> submissions)
    {
        List<double> percentages = rows
            .Where(r => r.PercentageOfGraded.HasValue)
            .Select(r => r.PercentageOfGraded!.Value)
            .ToList();

        return new GradebookSummaryDto(
            rows.Count,
            columns.Count,
            submissions.Count(s => s.Status == SubmissionStatus.Submitted),
            percentages.Count == 0 ? null : Math.Round(percentages.Average(), 1));
    }
}
