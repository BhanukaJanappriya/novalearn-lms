namespace NovaLearn.Application.Features.Authentication.Common;

/// <summary>
/// The payload returned on a successful sign-in / token refresh. The refresh token is opaque
/// and is normally delivered to the browser as an httpOnly cookie rather than in this body.
/// </summary>
public sealed record AuthenticationResponse(
    string AccessToken,
    string RefreshToken,
    DateTimeOffset AccessTokenExpiresAtUtc,
    UserSummary User,
    string TokenType = "Bearer");
