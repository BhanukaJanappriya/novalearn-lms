using MediatR;
using NovaLearn.Application.Features.Assessments.Common;
using NovaLearn.Shared.Results;

namespace NovaLearn.Application.Features.Assessments.GetCourseAssignments;

/// <summary>
/// A course's assignments. Staff see everything including drafts, with submission counts;
/// enrolled learners see published work only, each carrying their own submission.
/// </summary>
public sealed record GetCourseAssignmentsQuery(Guid CourseId)
    : IRequest<Result<IReadOnlyList<AssignmentDto>>>;
