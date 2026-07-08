using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using NovaLearn.Application.Common.Interfaces;
using NovaLearn.Application.Common.Models;

namespace NovaLearn.Infrastructure.Authentication;

/// <summary>
/// Issues HMAC-SHA256-signed JWT access tokens and cryptographically random refresh tokens.
/// </summary>
public sealed class JwtTokenService(IOptions<JwtOptions> options, IDateTimeProvider dateTimeProvider)
    : IJwtTokenService
{
    private readonly JwtOptions _options = options.Value;

    public AccessToken CreateAccessToken(AuthenticatedUser user)
    {
        string jwtId = Guid.NewGuid().ToString("N");
        DateTimeOffset now = dateTimeProvider.UtcNow;
        DateTimeOffset expires = now.AddMinutes(_options.AccessTokenLifetimeMinutes);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(JwtRegisteredClaimNames.Jti, jwtId),
            new(JwtRegisteredClaimNames.Email, user.Email),
            new(JwtRegisteredClaimNames.Name, user.FullName),
            new("email_verified", user.EmailConfirmed ? "true" : "false")
        };

        claims.AddRange(user.Roles.Select(role => new Claim(ClaimTypes.Role, role)));

        var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.SigningKey));
        var credentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);

        var descriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Issuer = _options.Issuer,
            Audience = _options.Audience,
            IssuedAt = now.UtcDateTime,
            NotBefore = now.UtcDateTime,
            Expires = expires.UtcDateTime,
            SigningCredentials = credentials
        };

        string token = new JsonWebTokenHandler().CreateToken(descriptor);
        return new AccessToken(token, jwtId, expires);
    }

    public string CreateRefreshToken()
    {
        byte[] randomBytes = RandomNumberGenerator.GetBytes(64);
        return Convert.ToBase64String(randomBytes);
    }
}
