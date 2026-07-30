using NovaLearn.Domain.Content;

namespace NovaLearn.API.Features.Content;

/// <summary>Body for creating a module (the course id comes from the route).</summary>
public sealed record CreateModuleRequest(
    string Title,
    string? Description);

/// <summary>Body for editing a module (the module id comes from the route).</summary>
public sealed record UpdateModuleRequest(
    string Title,
    string? Description);

/// <summary>Body for creating a lesson. Enums accept their string names (e.g. "Video").</summary>
public sealed record CreateLessonRequest(
    string Title,
    LessonType Type,
    string? ContentUrl,
    string? TextContent,
    int? DurationMinutes,
    bool IsPreview);

/// <summary>Body for editing a lesson (the lesson id comes from the route).</summary>
public sealed record UpdateLessonRequest(
    string Title,
    LessonType Type,
    string? ContentUrl,
    string? TextContent,
    int? DurationMinutes,
    bool IsPreview);

/// <summary>Body for a reorder: the ids of every child, in the desired order.</summary>
public sealed record ReorderRequest(IReadOnlyList<Guid> Ids);
