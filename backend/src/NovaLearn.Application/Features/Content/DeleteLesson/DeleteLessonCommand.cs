using MediatR;
using NovaLearn.Shared.Results;

namespace NovaLearn.Application.Features.Content.DeleteLesson;

/// <summary>Soft-deletes a lesson. Admins any course; lecturers only their own.</summary>
public sealed record DeleteLessonCommand(Guid LessonId) : IRequest<Result>;
