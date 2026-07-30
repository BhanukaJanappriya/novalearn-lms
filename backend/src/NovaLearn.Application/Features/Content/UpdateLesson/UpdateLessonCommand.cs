using MediatR;
using NovaLearn.Application.Features.Content.Common;
using NovaLearn.Domain.Content;
using NovaLearn.Shared.Results;

namespace NovaLearn.Application.Features.Content.UpdateLesson;

/// <summary>Edits a lesson's details. Admins any course; lecturers only their own.</summary>
public sealed record UpdateLessonCommand(
    Guid LessonId,
    string Title,
    LessonType Type,
    string? ContentUrl,
    string? TextContent,
    int? DurationMinutes,
    bool IsPreview) : IRequest<Result<LessonDto>>;
