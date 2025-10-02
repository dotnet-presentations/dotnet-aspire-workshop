using MyWeatherHub;
using MyWeatherHub.Components;
using Microsoft.EntityFrameworkCore;


var builder = WebApplication.CreateBuilder(args);
builder.AddServiceDefaults();

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddHttpClient<NwsManager>(c =>
{
    var url = builder.Configuration["WeatherEndpoint"] ?? throw new InvalidOperationException("WeatherEndpoint is not set");

    c.BaseAddress = new(url);
});

// Register EF Core DbContext via Aspire integration (Lesson 7)
builder.AddNpgsqlDbContext<MyWeatherContext>(connectionName: "weatherdb");


var app = builder.Build();
app.MapDefaultEndpoints();

// Ensure database is created in Development (Lesson 7)
if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var context = scope.ServiceProvider.GetRequiredService<MyWeatherContext>();
    await context.Database.EnsureCreatedAsync();
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
