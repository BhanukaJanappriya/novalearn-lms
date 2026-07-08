using MediatR;
using NovaLearn.Shared.Results;

namespace NovaLearn.Application.Features.Authentication.Register;

/// <summary>Registers a new self-service account (assigned the Student role, email unverified).</summary>
public sealed record RegisterCommand(
    string FirstName,
    string LastName,
    string Email,
    string Password)
    : IRequest<Result<RegisterResponse>>;

/// <summary>Result of a successful registration. No tokens are issued until the email is verified.</summary>
public sealed record RegisterResponse(Guid UserId, string Email, bool RequiresEmailVerification = true);
