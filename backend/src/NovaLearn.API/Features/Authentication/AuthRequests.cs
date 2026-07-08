namespace NovaLearn.API.Features.Authentication;

/// <summary>Request bodies for the authentication endpoints (kept separate from Application commands).</summary>
public sealed record RegisterRequest(string FirstName, string LastName, string Email, string Password);

public sealed record LoginRequest(string Email, string Password);

public sealed record VerifyEmailRequest(Guid UserId, string Token);

/// <summary>Optional body for the refresh endpoint; if omitted the token is read from the cookie.</summary>
public sealed record RefreshRequest(string? RefreshToken);
