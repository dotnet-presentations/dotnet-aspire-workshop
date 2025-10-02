var builder = DistributedApplication.CreateBuilder(args);

// External weather API (development-time modeling only)
builder.AddExternalService("weather-api", "https://api.weather.gov");

var api = builder.AddProject<Projects.Api>("api");
var web = builder.AddProject<Projects.MyWeatherHub>("myweatherhub")
	.WithReference(api)
	.WithExternalHttpEndpoints();

builder.Build().Run();
