using MediatR;
using NovaLearn.Application.Common.Interfaces;
using NovaLearn.Application.Features.Enrollments.Common;
using NovaLearn.Shared.Common;
using NovaLearn.Shared.Results;

namespace NovaLearn.Application.Features.Enrollments.GetCourseCatalog;

public sealed class GetCourseCatalogQueryHandler(
    IEnrollmentRepository enrollments,
    ICurrentUser currentUser)
    : IRequestHandler<GetCourseCatalogQuery, Result<PagedResult<CourseCatalogDto>>>
{
    public async Task<Result<PagedResult<CourseCatalogDto>>> Handle(
        GetCourseCatalogQuery request, CancellationToken cancellationToken)
    {
        // An anonymous-but-authenticated caller (e.g. an admin browsing) simply sees no
        // "enrolled" flags rather than being rejected.
        PagedResult<CourseCatalogDto> page = await enrollments.GetCatalogAsync(
            string.IsNullOrWhiteSpace(request.Search) ? null : request.Search.Trim(),
            string.IsNullOrWhiteSpace(request.Category) ? null : request.Category.Trim(),
            request.Level,
            currentUser.UserId,
            request.Page,
            request.PageSize,
            cancellationToken);

        return Result.Success(page);
    }
}
