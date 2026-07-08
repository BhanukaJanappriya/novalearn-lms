namespace NovaLearn.Application.Common.Models;

/// <summary>A signed JWT access token together with its identifier and expiry.</summary>
public sealed record AccessToken(string Value, string JwtId, DateTimeOffset ExpiresAtUtc);
