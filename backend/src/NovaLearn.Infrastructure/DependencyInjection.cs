using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using NovaLearn.Application.Common.Interfaces;
using NovaLearn.Application.Features.Authentication.Common;
using NovaLearn.Infrastructure.Authentication;
using NovaLearn.Infrastructure.Email;
using NovaLearn.Infrastructure.Identity;
using NovaLearn.Infrastructure.Storage;
using NovaLearn.Infrastructure.Time;

namespace NovaLearn.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<JwtOptions>()
            .Bind(configuration.GetSection(JwtOptions.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.Configure<AuthTokenOptions>(configuration.GetSection(AuthTokenOptions.SectionName));
        services.Configure<AppUrlsOptions>(configuration.GetSection(AppUrlsOptions.SectionName));
        services.Configure<FileStorageOptions>(configuration.GetSection(FileStorageOptions.SectionName));

        services.AddSingleton<IDateTimeProvider, DateTimeProvider>();
        services.AddSingleton<IJwtTokenService, JwtTokenService>();
        services.AddScoped<IIdentityService, IdentityService>();
        services.AddScoped<IUserAdministration, UserAdministrationService>();
        services.AddScoped<IProfileService, ProfileService>();
        services.AddScoped<IEmailSender, LoggingEmailSender>();
        services.AddSingleton<IFileStorage, LocalFileStorage>();
        services.AddSingleton<IUploadLimits, UploadLimits>();

        return services;
    }

    /// <summary>Wires up JWT bearer authentication using the validated "Jwt" options.</summary>
    public static IServiceCollection AddJwtAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        var jwt = configuration.GetSection(JwtOptions.SectionName).Get<JwtOptions>()
            ?? throw new InvalidOperationException("The 'Jwt' configuration section is missing.");

        var validationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwt.Issuer,
            ValidateAudience = true,
            ValidAudience = jwt.Audience,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.SigningKey)),
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromSeconds(30),
            RoleClaimType = ClaimTypes.Role,
            NameClaimType = JwtRegisteredClaimNames.Name
        };

        services
            .AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.MapInboundClaims = false;
                options.SaveToken = true;
                options.TokenValidationParameters = validationParameters;

                options.Events = new JwtBearerEvents
                {
                    OnMessageReceived = context =>
                    {
                        // A SignalR WebSocket cannot carry an Authorization header, so the client
                        // passes the token as a query parameter. Restricted to hub paths so a
                        // token can never be accepted from a query string on a normal endpoint,
                        // where it would end up in logs and browser history.
                        string? accessToken = context.Request.Query["access_token"];

                        if (!string.IsNullOrEmpty(accessToken)
                            && context.HttpContext.Request.Path.StartsWithSegments("/hubs"))
                        {
                            context.Token = accessToken;
                        }

                        return Task.CompletedTask;
                    }
                };
            });

        services.AddAuthorization();

        return services;
    }
}
