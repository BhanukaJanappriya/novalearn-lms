using MediatR;
using NovaLearn.Application.Common.Interfaces;
using NovaLearn.Application.Features.Admin.Departments.Common;
using NovaLearn.Domain.Departments;
using NovaLearn.Shared.Results;

namespace NovaLearn.Application.Features.Admin.Departments.GetDepartments;

/// <summary>
/// Every department, alphabetical. Readable by lecturers as well as admins, because the course
/// form needs the list to offer a department picker.
/// </summary>
public sealed record GetDepartmentsQuery : IRequest<Result<IReadOnlyList<DepartmentDto>>>;

public sealed class GetDepartmentsQueryHandler(IDepartmentRepository departments)
    : IRequestHandler<GetDepartmentsQuery, Result<IReadOnlyList<DepartmentDto>>>
{
    public async Task<Result<IReadOnlyList<DepartmentDto>>> Handle(
        GetDepartmentsQuery request, CancellationToken cancellationToken)
    {
        IReadOnlyList<Department> all = await departments.ListAsync(cancellationToken);

        // One grouped query for every count, rather than one per department.
        IReadOnlyDictionary<Guid, int> counts = await departments.CountCoursesAsync(cancellationToken);

        return all
            .Select(d => DepartmentDto.FromEntity(d, counts.TryGetValue(d.Id, out int n) ? n : 0))
            .ToList();
    }
}
