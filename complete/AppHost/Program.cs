var builder = DistributedApplication.CreateBuilder(args);

var cache = builder.AddRedis("cache")
									 .WithRedisInsight();

var api = builder.AddProject<Projects.Api>("api")
								 .WithReference(cache);

var postgres = builder.AddPostgres("postgres")
								.WithDataVolume(isReadOnly: false);

var weatherDb = postgres.AddDatabase("weatherdb");

var web = builder.AddProject<Projects.MyWeatherHub>("myweatherhub")
								 .WithReference(api)
								 .WithReference(weatherDb)
								 .WaitFor(postgres)
								 .WithExternalHttpEndpoints();

builder.Build().Run();
