using MediatR;
using Microsoft.Extensions.Logging;
using NovaLearn.Application.Common.Errors;
using NovaLearn.Application.Common.Interfaces;
using NovaLearn.Application.Common.Models;
using NovaLearn.Application.Features.Authentication.Common;
using NovaLearn.Shared.Results;
using NovaLearn.Shared.Security;
using DomainRefreshToken = NovaLearn.Domain.Identity.RefreshToken;

namespace NovaLearn.Application.Features.Authentication.RefreshToken;

/// <summary>
/// Validates and rotates a refresh token. If a token that has already been rotated (revoked) is
/// presented again, this is treated as theft/replay: the entire active token chain for that user
/// is revoked and the request is rejected.
/// </summary>
public sealed class RefreshTokenCommandHandler(
    IRefreshTokenRepository refreshTokenRepository,
    IIdentityService identityService,
    IAuthTokenIssuer tokenIssuer,
    IUnitOfWork unitOfWork,
    IDateTimeProvider dateTimeProvider,
    ICurrentUser currentUser,
    ILogger<RefreshTokenCommandHandler> logger)
    : IRequestHandler<RefreshTokenCommand, Result<AuthenticationResponse>>
{
    public async Task<Result<AuthenticationResponse>> Handle(
        RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        string tokenHash = TokenHasher.Hash(request.RefreshToken);
        DomainRefreshToken? stored = await refreshTokenRepository.GetByHashAsync(tokenHash, cancellationToken);
        DateTimeOffset now = dateTimeProvider.UtcNow;

        if (stored is null)
        {
            return Result.Failure<AuthenticationResponse>(AuthenticationErrors.InvalidRefreshToken);
        }

        // Replay of an already-rotated token → assume compromise and revoke the whole chain.
        if (stored.IsRevoked)
        {
            logger.LogWarning(
                "Detected reuse of a revoked refresh token for user {UserId}. Revoking all active tokens.",
                stored.UserId);
            await refreshTokenRepository.RevokeAllActiveForUserAsync(
                stored.UserId, now, currentUser.IpAddress, cancellationToken);
            await unitOfWork.SaveChangesAsync(cancellationToken);
            return Result.Failure<AuthenticationResponse>(AuthenticationErrors.InvalidRefreshToken);
        }

        if (stored.IsExpired(now))
        {
            return Result.Failure<AuthenticationResponse>(AuthenticationErrors.InvalidRefreshToken);
        }

        Result<AuthenticatedUser> userResult =
            await identityService.FindByIdAsync(stored.UserId, cancellationToken);
        if (userResult.IsFailure)
        {
            return Result.Failure<AuthenticationResponse>(AuthenticationErrors.InvalidRefreshToken);
        }

        // Issue the replacement, then revoke the old token and link it to its successor.
        AuthenticationResponse response = await tokenIssuer.IssueAsync(userResult.Value, cancellationToken);
        stored.Revoke(now, currentUser.IpAddress, TokenHasher.Hash(response.RefreshToken));
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return response;
    }
}
