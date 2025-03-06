using Microsoft.AspNetCore.Diagnostics.HealthChecks;

var builder = WebApplication.CreateBuilder(args);

builder.AddRedisOutputCache("cache");

// Add service defaults & Aspire client integrations.
builder.AddServiceDefaults();

// Add services to the container.
builder.Services.AddProblemDetails();

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddNwsManager();

builder.Services.AddOpenTelemetry()
    .WithMetrics(m => m.AddMeter("NwsManagerMetrics"));

// Add health check services for redis cache and external service
builder.Services.AddHealthChecks()
    .AddRedis("localhost:6379", name: "redis")
    .AddUrlGroup(new Uri("https://api.weather.gov/"), name: "weatherApi");

var app = builder.Build();

app.MapDefaultEndpoints();

app.UseHttpsRedirection();

// Map the endpoints for the API
app.MapApiEndpoints();

// Add health check endpoints for /health and /alive
app.MapHealthChecks("/health");
app.MapHealthChecks("/alive", new HealthCheckOptions
{
    Predicate = r => r.Tags.Contains("live")
});

app.Run();
