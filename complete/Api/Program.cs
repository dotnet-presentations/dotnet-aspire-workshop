using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;
using Api.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;

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

// Add health check services for database, redis cache, and external service
builder.Services.AddHealthChecks()
    .AddNpgSql(builder.Configuration.GetConnectionString("DefaultConnection"), name: "postgresql")
    .AddRedis("localhost:6379", name: "redis")
    .AddUrlGroup(new Uri("https://api.weather.gov/"), name: "weatherapi");

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
