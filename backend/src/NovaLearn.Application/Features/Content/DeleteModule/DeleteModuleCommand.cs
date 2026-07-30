using MediatR;
using NovaLearn.Shared.Results;

namespace NovaLearn.Application.Features.Content.DeleteModule;

/// <summary>Soft-deletes a module and its lessons. Admins any course; lecturers only their own.</summary>
public sealed record DeleteModuleCommand(Guid ModuleId) : IRequest<Result>;
