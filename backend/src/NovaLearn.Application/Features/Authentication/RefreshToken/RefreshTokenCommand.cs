using MediatR;
using NovaLearn.Application.Features.Authentication.Common;
using NovaLearn.Shared.Results;

namespace NovaLearn.Application.Features.Authentication.RefreshToken;

/// <summary>Exchanges a valid refresh token for a new access token, rotating the refresh token.</summary>
public sealed record RefreshTokenCommand(string RefreshToken) : IRequest<Result<AuthenticationResponse>>;
