using NovaLearn.Domain.Content;

namespace NovaLearn.Application.Features.Content.Common;

/// <summary>Read model for a lesson. Enums are surfaced as their string names for the client.</summary>
public sealed record LessonDto(
    Guid Id,
    Guid ModuleId,
    string Title,
    string Type,
    string? ContentUrl,
    string? TextContent,
    int? DurationMinutes,
    int SortOrder,
    bool IsPreview)
{
    public static LessonDto FromEntity(Lesson lesson) => new(
        lesson.Id,
        lesson.ModuleId,
        lesson.Title,
        lesson.Type.ToString(),
        lesson.ContentUrl,
        lesson.TextContent,
        lesson.DurationMinutes,
        lesson.SortOrder,
        lesson.IsPreview);
}
