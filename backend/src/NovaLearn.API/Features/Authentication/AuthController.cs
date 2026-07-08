using System.Security.Claims;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.IdentityModel.JsonWebTokens;
using MediatR;
using NovaLearn.API.Common;
using NovaLearn.Application.Features.Authentication.Common;
using NovaLearn.Application.Features.Authentication.Login;
using NovaLearn.Application.Features.Authentication.RefreshToken;
using NovaLearn.Application.Features.Authentication.Register;
using NovaLearn.Application.Features.Authentication.VerifyEmail;
using NovaLearn.Shared.Results;

namespace NovaLearn.API.Features.Authentication;

[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/auth")]
[EnableRateLimiting("auth")]
public sealed class AuthController(ISender sender, IWebHostEnvironment environment) : ApiControllerBase
{
    private const string RefreshTokenCookie = "novalearn_rt";

    /// <summary>Registers a new account. A verification email is sent; login requires a verified email.</summary>
    [HttpPost("register")]
    [ProducesResponseType(typeof(RegisterResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Register(RegisterRequest request, CancellationToken cancellationToken)
    {
        var command = new RegisterCommand(request.FirstName, request.LastName, request.Email, request.Password);
        Result<RegisterResponse> result = await sender.Send(command, cancellationToken);
        return HandleResult(result);
    }

    /// <summary>Authenticates and returns an access token; the refresh token is set as an httpOnly cookie.</summary>
    [HttpPost("login")]
    [ProducesResponseType(typeof(AuthenticationResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login(LoginRequest request, CancellationToken cancellationToken)
    {
        Result<AuthenticationResponse> result =
            await sender.Send(new LoginCommand(request.Email, request.Password), cancellationToken);

        return HandleResult(result, WithRefreshCookie);
    }

    /// <summary>Rotates the refresh token (from cookie or body) and returns a fresh access token.</summary>
    [HttpPost("refresh")]
    [ProducesResponseType(typeof(AuthenticationResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Refresh(RefreshRequest? request, CancellationToken cancellationToken)
    {
        string? token = request?.RefreshToken ?? Request.Cookies[RefreshTokenCookie];
        if (string.IsNullOrWhiteSpace(token))
        {
            return Unauthorized();
        }

        Result<AuthenticationResponse> result =
            await sender.Send(new RefreshTokenCommand(token), cancellationToken);

        return HandleResult(result, WithRefreshCookie);
    }

    /// <summary>Confirms an email address using the token from the verification link.</summary>
    [HttpPost("verify-email")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> VerifyEmail(VerifyEmailRequest request, CancellationToken cancellationToken)
    {
        Result result = await sender.Send(new VerifyEmailCommand(request.UserId, request.Token), cancellationToken);
        return HandleResult(result);
    }

    /// <summary>Clears the refresh-token cookie. (Server-side revocation is handled on rotation.)</summary>
    [HttpPost("logout")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public IActionResult Logout()
    {
        // Must match the Path the cookie was written with, or the browser won't clear it.
        Response.Cookies.Delete(RefreshTokenCookie, new CookieOptions { Path = "/api/v1/auth" });
        return NoContent();
    }

    /// <summary>Returns the authenticated caller's profile from their token claims.</summary>
    [HttpGet("me")]
    [Authorize]
    [ProducesResponseType(typeof(UserSummary), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public IActionResult Me()
    {
        Guid.TryParse(User.FindFirstValue(JwtRegisteredClaimNames.Sub), out Guid id);
        string email = User.FindFirstValue(JwtRegisteredClaimNames.Email) ?? string.Empty;
        string name = User.FindFirstValue(JwtRegisteredClaimNames.Name) ?? string.Empty;
        string[] roles = User.FindAll(ClaimTypes.Role).Select(c => c.Value).ToArray();

        return Ok(new UserSummary(id, email, name, roles));
    }

    private IActionResult WithRefreshCookie(AuthenticationResponse response)
    {
        Response.Cookies.Append(RefreshTokenCookie, response.RefreshToken, new CookieOptions
        {
            HttpOnly = true,
            Secure = !environment.IsDevelopment(),
            SameSite = environment.IsDevelopment() ? SameSiteMode.Lax : SameSiteMode.None,
            Expires = response.AccessTokenExpiresAtUtc.AddDays(7),
            Path = "/api/v1/auth"
        });

        return Ok(response);
    }
}
