using MediatR;
using NovaLearn.Shared.Results;

namespace NovaLearn.Application.Features.Authentication.VerifyEmail;

/// <summary>Confirms a user's email address using the token from the verification link.</summary>
public sealed record VerifyEmailCommand(Guid UserId, string Token) : IRequest<Result>;
