var builder = DistributedApplication.CreateBuilder(args);

var invalidationKey = builder.AddParameter("ApiCacheInvalidationKey");	

var cache = builder.AddRedis("cache")
	.WithClearCommand()
	.WithRedisInsight();

var weatherApi = builder.AddExternalService("weather-api", "https://api.weather.gov");

var api = builder.AddProject<Projects.Api>("api")
	.WithApiCacheInvalidation(invalidationKey)
	.WithReference(weatherApi)
	.WithReference(cache);

var postgres = builder.AddPostgres("postgres")
								.WithDataVolume(isReadOnly: false)
								.WithLifetime(ContainerLifetime.Persistent);

var weatherDb = postgres.AddDatabase("weatherdb");

var web = builder.AddProject<Projects.MyWeatherHub>("myweatherhub")
								 .WithReference(api)
								 .WithReference(weatherDb)
								 .WaitFor(postgres)
								 .WithExternalHttpEndpoints();

builder.Build().Run();
