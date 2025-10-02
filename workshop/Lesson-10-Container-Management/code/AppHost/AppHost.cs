var builder = DistributedApplication.CreateBuilder(args);

var cache = builder.AddRedis("cache")
	.WithRedisInsight()
	.WithLifetime(ContainerLifetime.Persistent);

var weatherApi = builder.AddExternalService("weather-api", "https://api.weather.gov");

var api = builder.AddProject<Projects.Api>("api")
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
