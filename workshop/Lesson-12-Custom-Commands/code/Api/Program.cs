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
		.WithMetrics(m => m.AddMeter("NwsManagerMetrics"))
		.WithTracing(m => m.AddSource("NwsManager"));

builder.Services.AddHealthChecks()
	.AddUrlGroup(new Uri("https://api.weather.gov/"), "NWS Weather API", HealthStatus.Unhealthy,
		configureClient: (services, client) =>
		{
			client.DefaultRequestHeaders.Add("User-Agent", "Microsoft - .NET Aspire Demo");
		});


var app = builder.Build();

app.MapDefaultEndpoints();

if (app.Environment.IsDevelopment())
{
	app.MapOpenApi();
}

// force the SSL redirect
app.UseWhen(context => !context.Request.Path.StartsWithSegments("/health"),
													 builder => builder.UseHttpsRedirection());

// Map the endpoints for the API
app.MapApiEndpoints();

app.Run();
