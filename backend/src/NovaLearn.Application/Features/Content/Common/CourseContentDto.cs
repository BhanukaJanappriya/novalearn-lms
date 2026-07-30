using NovaLearn.Domain.Content;
using NovaLearn.Domain.Courses;

namespace NovaLearn.Application.Features.Content.Common;

/// <summary>
/// The full content tree of a course: enough course header detail for the builder page,
/// plus every module with its nested lessons in presentation order.
/// </summary>
public sealed record CourseContentDto(
    Guid CourseId,
    string CourseTitle,
    string CourseCode,
    string CourseStatus,
    Guid LecturerId,
    IReadOnlyList<ModuleDto> Modules)
{
    public static CourseContentDto FromEntities(Course course, IEnumerable<CourseModule> modules) => new(
        course.Id,
        course.Title,
        course.Code,
        course.Status.ToString(),
        course.LecturerId,
        modules.OrderBy(m => m.SortOrder).Select(ModuleDto.FromEntity).ToList());
}
