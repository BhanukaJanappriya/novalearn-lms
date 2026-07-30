using MediatR;
using NovaLearn.Shared.Results;

namespace NovaLearn.Application.Features.Content.ReorderLessons;

/// <summary>
/// Rewrites the order of a module's lessons. <paramref name="LessonIds"/> must list every
/// lesson of the module exactly once, in the desired order.
/// </summary>
public sealed record ReorderLessonsCommand(
    Guid ModuleId,
    IReadOnlyList<Guid> LessonIds) : IRequest<Result>;
