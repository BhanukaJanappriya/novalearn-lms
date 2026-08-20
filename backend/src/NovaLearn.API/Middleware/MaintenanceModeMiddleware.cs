using Microsoft.AspNetCore.Mvc;
using NovaLearn.Application.Common.Interfaces;
using NovaLearn.Application.Common.Models;
using NovaLearn.Domain.Identity;

namespace NovaLearn.API.Middleware;

/// <summary>
/// Blocks the platform for everyone except administrators while maintenance mode is on — the one
/// genuinely enforced effect of the setting, not just a value the admin screen shows back.
///
/// Placed after <c>UseAuthentication</c> so <see cref="HttpContext.User"/> already carries the
/// caller's roles from their bearer token by the time this runs, and before <c>UseAuthorization</c>
/// so a blocked request never reaches routing or a controller at all.
///
/// <see cref="ISettingsProvider"/> is resolved per request through <see cref="InvokeAsync"/>'s own
/// parameter rather than a constructor field: middleware built with <c>UseMiddleware&lt;T&gt;</c>
/// is constructed once for the whole application, so a scoped service taken in the constructor
/// would be captured past the scope it belongs to. Taking it as a method parameter is what lets
/// the framework hand this middleware a fresh instance from the current request's own scope.
/// </summary>
public sealed class MaintenanceModeMiddleware(RequestDelegate next)
{
    /// <summary>
    /// Paths that stay reachable regardless of maintenance mode: the public settings read (which
    /// carries the maintenance banner itself), the admin settings screen (the only way to turn
    /// maintenance mode back off), signing in (an administrator needs a fresh token to reach that
    /// screen), infrastructure checks, and the API's own documentation.
    /// </summary>
    private static readonly string[] ExemptPathPrefixes =
    [
        "/api/v1/settings",
        "/api/v1/admin/settings",
        "/api/v1/auth",
        "/health",
        "/swagger",
        "/hubs",
    ];

    public async Task InvokeAsync(HttpContext context, ISettingsProvider settings)
    {
        string path = context.Request.Path.Value ?? string.Empty;

        if (ExemptPathPrefixes.Any(prefix => path.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)))
        {
            await next(context);
            return;
        }

        PlatformSettingsSnapshot platform = await settings.GetAsync(context.RequestAborted);

        bool isAdmin =
            context.User.IsInRole(Roles.Administrator) || context.User.IsInRole(Roles.SuperAdministrator);

        if (!platform.MaintenanceModeEnabled || isAdmin)
        {
            await next(context);
            return;
        }

        context.Response.StatusCode = StatusCodes.Status503ServiceUnavailable;
        context.Response.ContentType = "application/problem+json";

        var problem = new ProblemDetails
        {
            Status = StatusCodes.Status503ServiceUnavailable,
            Title = "The platform is temporarily unavailable.",
            Detail = platform.MaintenanceMessage
                ?? "The platform is undergoing maintenance. Please check back shortly.",
        };

        await context.Response.WriteAsJsonAsync(problem, context.RequestAborted);
    }
}
