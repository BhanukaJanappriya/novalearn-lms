using MediatR;
using NovaLearn.Application.Features.Content.Common;
using NovaLearn.Domain.Content;
using NovaLearn.Shared.Results;

namespace NovaLearn.Application.Features.Content.CreateLesson;

/// <summary>Appends a lesson to a module. Admins any course; lecturers only their own.</summary>
public sealed record CreateLessonCommand(
    Guid ModuleId,
    string Title,
    LessonType Type,
    string? ContentUrl,
    string? TextContent,
    int? DurationMinutes,
    bool IsPreview) : IRequest<Result<LessonDto>>;
