
var builder = WebApplication.CreateBuilder(args);
builder.AddRedisOutputCache("cache");
builder.AddServiceDefaults();

// Register custom metrics & tracing sources for telemetry module
builder.Services.AddOpenTelemetry()
    .WithMetrics(m => m.AddMeter("NwsManagerMetrics"))
    .WithTracing(t => t.AddSource("NwsManager"));

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddNwsManager();


var app = builder.Build();
app.MapDefaultEndpoints();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

// Map the endpoints for the API
app.MapApiEndpoints();

app.Run();
