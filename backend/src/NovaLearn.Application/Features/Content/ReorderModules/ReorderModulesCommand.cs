using MediatR;
using NovaLearn.Shared.Results;

namespace NovaLearn.Application.Features.Content.ReorderModules;

/// <summary>
/// Rewrites the order of a course's modules. <paramref name="ModuleIds"/> must list every
/// module of the course exactly once, in the desired order.
/// </summary>
public sealed record ReorderModulesCommand(
    Guid CourseId,
    IReadOnlyList<Guid> ModuleIds) : IRequest<Result>;
