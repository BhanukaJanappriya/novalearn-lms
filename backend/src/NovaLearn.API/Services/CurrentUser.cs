using System.Security.Claims;
using Microsoft.IdentityModel.JsonWebTokens;
using NovaLearn.Application.Common.Interfaces;

namespace NovaLearn.API.Services;

/// <summary>Resolves the current request's user from the authenticated <see cref="ClaimsPrincipal"/>.</summary>
public sealed class CurrentUser(IHttpContextAccessor httpContextAccessor) : ICurrentUser
{
    private ClaimsPrincipal? Principal => httpContextAccessor.HttpContext?.User;

    public Guid? UserId =>
        Guid.TryParse(Principal?.FindFirstValue(JwtRegisteredClaimNames.Sub), out Guid id) ? id : null;

    public string? Email => Principal?.FindFirstValue(JwtRegisteredClaimNames.Email);

    public bool IsAuthenticated => Principal?.Identity?.IsAuthenticated ?? false;

    public string? IpAddress => httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString();
}
