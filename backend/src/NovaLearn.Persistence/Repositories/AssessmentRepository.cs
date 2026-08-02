using Microsoft.EntityFrameworkCore;
using NovaLearn.Application.Common.Interfaces;
using NovaLearn.Domain.Assessments;

namespace NovaLearn.Persistence.Repositories;

public sealed class AssessmentRepository(ApplicationDbContext dbContext) : IAssessmentRepository
{
    public async Task AddAssignmentAsync(Assignment assignment, CancellationToken cancellationToken) =>
        await dbContext.Assignments.AddAsync(assignment, cancellationToken);

    public Task<Assignment?> GetAssignmentAsync(Guid assignmentId, CancellationToken cancellationToken) =>
        dbContext.Assignments
            .Include(a => a.Course)
            .FirstOrDefaultAsync(a => a.Id == assignmentId, cancellationToken);

    public async Task<IReadOnlyList<Assignment>> ListAssignmentsAsync(
        Guid courseId, CancellationToken cancellationToken) =>
        await dbContext.Assignments
            .Where(a => a.CourseId == courseId)
            // Undated work sorts last: a null due date means open ended, not overdue.
            .OrderBy(a => a.DueAtUtc == null)
            .ThenBy(a => a.DueAtUtc)
            .ThenBy(a => a.Title)
            .ToListAsync(cancellationToken);

    public void RemoveAssignment(Assignment assignment) => dbContext.Assignments.Remove(assignment);

    public async Task AddSubmissionAsync(Submission submission, CancellationToken cancellationToken) =>
        await dbContext.Submissions.AddAsync(submission, cancellationToken);

    public Task<Submission?> GetSubmissionAsync(Guid submissionId, CancellationToken cancellationToken) =>
        dbContext.Submissions
            .Include(s => s.Student)
            .Include(s => s.Assignment)
            .ThenInclude(a => a!.Course)
            .FirstOrDefaultAsync(s => s.Id == submissionId, cancellationToken);

    public Task<Submission?> GetSubmissionForStudentAsync(
        Guid assignmentId, Guid studentId, CancellationToken cancellationToken) =>
        dbContext.Submissions
            .FirstOrDefaultAsync(
                s => s.AssignmentId == assignmentId && s.StudentId == studentId, cancellationToken);

    public async Task<IReadOnlyList<Submission>> ListSubmissionsAsync(
        Guid assignmentId, CancellationToken cancellationToken) =>
        await dbContext.Submissions
            .Include(s => s.Student)
            .Where(s => s.AssignmentId == assignmentId)
            .OrderByDescending(s => s.SubmittedAtUtc)
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<Submission>> ListSubmissionsForStudentAsync(
        Guid courseId, Guid studentId, CancellationToken cancellationToken) =>
        await dbContext.Submissions
            .Include(s => s.Assignment)
            .Where(s => s.StudentId == studentId && s.Assignment!.CourseId == courseId)
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<SubmissionTally>> TallySubmissionsAsync(
        Guid courseId, CancellationToken cancellationToken) =>
        await dbContext.Submissions
            .Where(s => s.Assignment!.CourseId == courseId)
            .GroupBy(s => s.AssignmentId)
            .Select(g => new SubmissionTally(
                g.Key,
                g.Count(),
                g.Count(s => s.Status == SubmissionStatus.Graded)))
            .ToListAsync(cancellationToken);
}
