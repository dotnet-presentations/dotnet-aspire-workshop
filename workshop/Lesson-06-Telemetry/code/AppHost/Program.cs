var builder = DistributedApplication.CreateBuilder(args);

// External weather API (development-time modeling only)
builder.AddExternalService("weather-api", "https://api.weather.gov");


var cache = builder.AddRedis("cache").WithRedisInsight();
var api = builder.AddProject<Projects.Api>("api").WithReference(cache);
var web = builder.AddProject<Projects.MyWeatherHub>("myweatherhub")
	.WithReference(api)
	.WithExternalHttpEndpoints();

builder.Build().Run();
