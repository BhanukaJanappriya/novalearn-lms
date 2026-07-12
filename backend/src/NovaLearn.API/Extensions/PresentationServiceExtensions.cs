using System.Text.Json.Serialization;
using System.Threading.RateLimiting;
using Asp.Versioning;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi.Models;
using NovaLearn.API.Middleware;
using NovaLearn.API.Services;
using NovaLearn.Application.Common.Interfaces;

namespace NovaLearn.API.Extensions;

/// <summary>Registers presentation-layer (web) services: MVC, versioning, Swagger, CORS, etc.</summary>
public static class PresentationServiceExtensions
{
    public const string CorsPolicy = "NovaLearnCors";

    public static IServiceCollection AddPresentation(this IServiceCollection services, IConfiguration configuration)
    {
        services
            .AddControllers()
            .AddJsonOptions(options =>
                options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));
        services.AddEndpointsApiExplorer();
        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUser, CurrentUser>();

        services.AddProblemDetails();
        services.AddExceptionHandler<GlobalExceptionHandler>();

        services.AddApiVersioningConfigured();
        services.AddSwagger();
        services.AddCorsConfigured(configuration);
        services.AddRateLimiting();
        services.AddHealthChecks();

        return services;
    }

    private static void AddApiVersioningConfigured(this IServiceCollection services)
    {
        services
            .AddApiVersioning(options =>
            {
                options.DefaultApiVersion = new ApiVersion(1, 0);
                options.AssumeDefaultVersionWhenUnspecified = true;
                options.ReportApiVersions = true;
                options.ApiVersionReader = new UrlSegmentApiVersionReader();
            })
            .AddMvc()
            .AddApiExplorer(options =>
            {
                options.GroupNameFormat = "'v'VVV";
                options.SubstituteApiVersionInUrl = true;
            });
    }

    private static void AddSwagger(this IServiceCollection services)
    {
        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo { Title = "NovaLearn API", Version = "v1" });

            var scheme = new OpenApiSecurityScheme
            {
                Name = "Authorization",
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT",
                In = ParameterLocation.Header,
                Description = "Enter the JWT access token (without the 'Bearer ' prefix).",
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
            };

            options.AddSecurityDefinition("Bearer", scheme);
            options.AddSecurityRequirement(new OpenApiSecurityRequirement { [scheme] = [] });
        });
    }

    private static void AddCorsConfigured(this IServiceCollection services, IConfiguration configuration)
    {
        string[] origins = configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];

        services.AddCors(options => options.AddPolicy(CorsPolicy, policy =>
            policy.WithOrigins(origins)
                .AllowAnyHeader()
                .AllowAnyMethod()
                .AllowCredentials()));
    }

    private static void AddRateLimiting(this IServiceCollection services)
    {
        services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

            // Throttle authentication endpoints to blunt credential-stuffing / brute force.
            options.AddFixedWindowLimiter("auth", limiter =>
            {
                limiter.PermitLimit = 10;
                limiter.Window = TimeSpan.FromMinutes(1);
                limiter.QueueLimit = 0;
                limiter.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
            });
        });
    }
}
