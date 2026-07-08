using NovaLearn.Application.Common.Models;

namespace NovaLearn.Application.Features.Authentication.Common;

/// <summary>
/// Issues an access token + rotating refresh token for a user and persists the refresh token.
/// Shared by the login and refresh-token use cases to keep token issuance in one place.
/// </summary>
public interface IAuthTokenIssuer
{
    Task<AuthenticationResponse> IssueAsync(AuthenticatedUser user, CancellationToken cancellationToken);
}
