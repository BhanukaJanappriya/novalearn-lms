using MediatR;
using NovaLearn.Application.Features.Content.Common;
using NovaLearn.Shared.Results;

namespace NovaLearn.Application.Features.Content.CreateModule;

/// <summary>Appends a module to a course. Admins any course; lecturers only their own.</summary>
public sealed record CreateModuleCommand(
    Guid CourseId,
    string Title,
    string? Description) : IRequest<Result<ModuleDto>>;
