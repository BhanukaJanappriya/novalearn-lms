using MediatR;
using NovaLearn.Application.Features.Assessments.Common;
using NovaLearn.Shared.Results;

namespace NovaLearn.Application.Features.Assessments.GetGradebook;

/// <summary>The marking grid for a course. Restricted to the owning lecturer or an admin.</summary>
public sealed record GetGradebookQuery(Guid CourseId) : IRequest<Result<GradebookDto>>;
