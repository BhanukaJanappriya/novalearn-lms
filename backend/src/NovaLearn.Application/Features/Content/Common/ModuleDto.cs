using NovaLearn.Domain.Content;

namespace NovaLearn.Application.Features.Content.Common;

/// <summary>Read model for a module and its lessons, both in presentation order.</summary>
public sealed record ModuleDto(
    Guid Id,
    Guid CourseId,
    string Title,
    string? Description,
    int SortOrder,
    IReadOnlyList<LessonDto> Lessons)
{
    public static ModuleDto FromEntity(CourseModule module) => new(
        module.Id,
        module.CourseId,
        module.Title,
        module.Description,
        module.SortOrder,
        module.Lessons.OrderBy(l => l.SortOrder).Select(LessonDto.FromEntity).ToList());
}
