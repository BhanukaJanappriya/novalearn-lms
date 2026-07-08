using NovaLearn.Application.Common.Models;

namespace NovaLearn.Application.Common.Interfaces;

/// <summary>Issues access tokens (JWT) and cryptographically-random refresh tokens.</summary>
public interface IJwtTokenService
{
    /// <summary>Creates a signed JWT embedding the user's id, email and role claims.</summary>
    AccessToken CreateAccessToken(AuthenticatedUser user);

    /// <summary>Generates a high-entropy, opaque refresh token (raw value, unhashed).</summary>
    string CreateRefreshToken();
}
