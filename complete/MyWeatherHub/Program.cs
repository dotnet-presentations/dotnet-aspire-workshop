using Microsoft.Extensions.Diagnostics.HealthChecks;
using MyWeatherHub;
using MyWeatherHub.Components;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();
builder.Services.AddHttpClient<NwsManager>(client =>
{
	client.BaseAddress = new("https+http://api");
});

// Add GitHub Models chat client
builder.AddAzureChatCompletionsClient("chat-model")
       .AddChatClient();

// Register the ForecastSummarizer service
builder.Services.AddScoped<ForecastSummarizer>();

// Add services to the container.
builder.Services.AddRazorComponents()
		.AddInteractiveServerComponents();

builder.Services.AddMemoryCache();

builder.AddNpgsqlDbContext<MyWeatherContext>(connectionName: "weatherdb");

builder.Services.AddHealthChecks()
	.AddUrlGroup(new Uri(builder.Configuration["services:api:http:0"] + "/openapi/v1.json"),
		"Weather Microservice", HealthStatus.Unhealthy);


var app = builder.Build();

app.MapDefaultEndpoints();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
	app.UseExceptionHandler("/Error", createScopeForErrors: true);
	// The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
	app.UseHsts();
}
else
{
	using var scope = app.Services.CreateScope();
	var context = scope.ServiceProvider.GetRequiredService<MyWeatherContext>();
	await context.Database.EnsureCreatedAsync();
}

// force the SSL redirect
app.UseWhen(context => !context.Request.Path.StartsWithSegments("/health"),
													 builder => builder.UseHttpsRedirection());

app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
		.AddInteractiveServerRenderMode();

app.Run();