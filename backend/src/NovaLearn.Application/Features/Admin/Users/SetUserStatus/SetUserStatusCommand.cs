using MediatR;
using NovaLearn.Application.Features.Admin.Users.Common;
using NovaLearn.Shared.Results;

namespace NovaLearn.Application.Features.Admin.Users.SetUserStatus;

/// <summary>Enables or disables sign-in for an account.</summary>
public sealed record SetUserStatusCommand(Guid UserId, bool IsActive) : IRequest<Result<AdminUserDto>>;
