using Microsoft.Extensions.Options;
using NovaLearn.Application.Common.Interfaces;
using NovaLearn.Application.Common.Models;
using NovaLearn.Shared.Security;
using DomainRefreshToken = NovaLearn.Domain.Identity.RefreshToken;

namespace NovaLearn.Application.Features.Authentication.Common;

/// <summary>
/// Default <see cref="IAuthTokenIssuer"/>. Generates the JWT and a random refresh token, stores
/// only the refresh token's hash, and returns the raw refresh token to the caller exactly once.
/// </summary>
public sealed class AuthTokenIssuer(
    IJwtTokenService jwtTokenService,
    IRefreshTokenRepository refreshTokenRepository,
    IUnitOfWork unitOfWork,
    IDateTimeProvider dateTimeProvider,
    ICurrentUser currentUser,
    IOptions<AuthTokenOptions> options)
    : IAuthTokenIssuer
{
    private readonly AuthTokenOptions _options = options.Value;

    public async Task<AuthenticationResponse> IssueAsync(AuthenticatedUser user, CancellationToken cancellationToken)
    {
        AccessToken accessToken = jwtTokenService.CreateAccessToken(user);
        string rawRefreshToken = jwtTokenService.CreateRefreshToken();

        DateTimeOffset now = dateTimeProvider.UtcNow;
        var refreshToken = DomainRefreshToken.Issue(
            userId: user.Id,
            tokenHash: TokenHasher.Hash(rawRefreshToken),
            jwtId: accessToken.JwtId,
            createdAtUtc: now,
            expiresAtUtc: now.AddDays(_options.RefreshTokenLifetimeDays),
            createdByIp: currentUser.IpAddress);

        await refreshTokenRepository.AddAsync(refreshToken, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        var summary = new UserSummary(user.Id, user.Email, user.FullName, user.Roles);
        return new AuthenticationResponse(
            AccessToken: accessToken.Value,
            RefreshToken: rawRefreshToken,
            AccessTokenExpiresAtUtc: accessToken.ExpiresAtUtc,
            User: summary);
    }
}
