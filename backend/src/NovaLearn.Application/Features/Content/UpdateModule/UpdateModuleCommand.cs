using MediatR;
using NovaLearn.Application.Features.Content.Common;
using NovaLearn.Shared.Results;

namespace NovaLearn.Application.Features.Content.UpdateModule;

/// <summary>Edits a module's details. Admins any course; lecturers only their own.</summary>
public sealed record UpdateModuleCommand(
    Guid ModuleId,
    string Title,
    string? Description) : IRequest<Result<ModuleDto>>;
