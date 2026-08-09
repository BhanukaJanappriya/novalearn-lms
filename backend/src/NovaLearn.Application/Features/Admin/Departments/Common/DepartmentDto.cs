using NovaLearn.Domain.Departments;

namespace NovaLearn.Application.Features.Admin.Departments.Common;

/// <summary>
/// Read model for the departments table. Carries the head's name and the course count so the
/// page needs one request rather than one per row.
/// </summary>
public sealed record DepartmentDto(
    Guid Id,
    string Name,
    string Code,
    string? Description,
    Guid? HeadId,
    string? HeadName,
    bool IsActive,
    int CourseCount,
    DateTimeOffset CreatedAtUtc)
{
    public static DepartmentDto FromEntity(Department department, int courseCount) => new(
        department.Id,
        department.Name,
        department.Code,
        department.Description,
        department.HeadId,
        department.Head?.FullName,
        department.IsActive,
        courseCount,
        department.CreatedAtUtc);
}
