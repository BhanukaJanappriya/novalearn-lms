using MediatR;
using NovaLearn.Application.Features.Admin.Users.Common;
using NovaLearn.Shared.Results;

namespace NovaLearn.Application.Features.Admin.Users.VerifyUserEmail;

/// <summary>
/// Confirms an account's email on the user's behalf and clears any lockout, so a stuck
/// registration can be cleared without the original link.
/// </summary>
public sealed record VerifyUserEmailCommand(Guid UserId) : IRequest<Result<AdminUserDto>>;
