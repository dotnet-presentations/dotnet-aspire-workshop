var builder = DistributedApplication.CreateBuilder(args);

// External weather API (development-time modeling only)
var weatherApi = builder.AddExternalService("weather-api", "https://api.weather.gov");

// Redis cache
var cache = builder.AddRedis("cache").WithRedisInsight();

// PostgreSQL (Lesson 7) with data volume for persistence (optional but shown per README)
var postgres = builder.AddPostgres("postgres")
	.WithDataVolume(isReadOnly: false); // persistence across restarts

var weatherDb = postgres.AddDatabase("weatherdb");

// API project references external weather API + cache
var api = builder.AddProject<Projects.Api>("api")
	.WithReference(cache)
	.WithReference(weatherApi);

// Web project references API + database; waits for Postgres to be ready
var web = builder.AddProject<Projects.MyWeatherHub>("myweatherhub")
	.WithReference(api)
	.WithReference(weatherDb)
	.WaitFor(postgres)
	.WithExternalHttpEndpoints();

builder.Build().Run();
