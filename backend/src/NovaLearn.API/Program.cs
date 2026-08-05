using NovaLearn.API.Extensions;
using NovaLearn.API.Features.Notifications;
using NovaLearn.Application.Common.Interfaces;
using NovaLearn.Application;
using NovaLearn.Infrastructure;
using NovaLearn.Persistence;
using Serilog;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

// Structured logging via Serilog, configured from appsettings + console sink.
builder.Host.UseSerilog((context, loggerConfiguration) => loggerConfiguration
    .ReadFrom.Configuration(context.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console());

// Compose the layers (dependencies flow inward).
builder.Services
    .AddApplication()
    .AddInfrastructure(builder.Configuration)
    .AddPersistence(builder.Configuration)
    .AddJwtAuthentication(builder.Configuration)
    .AddPresentation(builder.Configuration);

// Real-time delivery. The hub lives in the presentation layer, so the Application layer only
// knows the INotificationPublisher port.
builder.Services.AddSignalR();
builder.Services.AddScoped<INotificationPublisher, SignalRNotificationPublisher>();

WebApplication app = builder.Build();

// --- HTTP pipeline (order matters) ---
app.UseExceptionHandler();
app.UseSerilogRequestLogging();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    await app.InitialiseDatabaseAsync();
}

app.UseHttpsRedirection();
app.UseCors(PresentationServiceExtensions.CorsPolicy);
app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHub<NotificationHub>("/hubs/notifications");
app.MapHealthChecks("/health");

app.Run();

// Exposed so the integration test host (WebApplicationFactory) can reference the entry point.
public partial class Program;
