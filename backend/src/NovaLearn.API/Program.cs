using NovaLearn.API.Extensions;
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
app.MapHealthChecks("/health");

app.Run();

// Exposed so the integration test host (WebApplicationFactory) can reference the entry point.
public partial class Program;
