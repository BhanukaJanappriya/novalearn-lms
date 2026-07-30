using MediatR;
using NovaLearn.Application.Features.Content.Common;
using NovaLearn.Shared.Results;

namespace NovaLearn.Application.Features.Content.GetCourseContent;

/// <summary>Reads a course's full module tree. Unpublished courses are owner/admin only.</summary>
public sealed record GetCourseContentQuery(Guid CourseId) : IRequest<Result<CourseContentDto>>;
