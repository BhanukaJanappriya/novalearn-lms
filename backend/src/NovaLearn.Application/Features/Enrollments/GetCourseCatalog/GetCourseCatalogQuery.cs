using MediatR;
using NovaLearn.Application.Features.Enrollments.Common;
using NovaLearn.Domain.Courses;
using NovaLearn.Shared.Common;
using NovaLearn.Shared.Results;

namespace NovaLearn.Application.Features.Enrollments.GetCourseCatalog;

/// <summary>
/// The learner-facing catalogue of published courses. All filters are optional; paging
/// defaults to the first page of 12.
/// </summary>
public sealed record GetCourseCatalogQuery(
    string? Search = null,
    string? Category = null,
    CourseLevel? Level = null,
    int Page = 1,
    int PageSize = 12) : IRequest<Result<PagedResult<CourseCatalogDto>>>;
