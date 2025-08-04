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
								.WithDataVolume(isReadOnly: false);

var weatherDb = postgres.AddDatabase("weatherdb");

// Add IT-Tools Docker container
var itTools = builder.AddContainer("it-tools", "corentinth/it-tools")
	.WithHttpEndpoint(targetPort: 80)
	.WithExternalHttpEndpoints();

// Add GitHub Models integration
var githubModel = builder.AddGitHubModel("chat-model", "gpt-4o-mini");

var web = builder.AddProject<Projects.MyWeatherHub>("myweatherhub")
								 .WithReference(api)
								 .WithReference(weatherDb)
								 .WithReference(githubModel)
								//  .WithReference(itTools)
								 .WaitFor(postgres)
								 .WithExternalHttpEndpoints();

builder.AddHealthChecksUI("healthchecks")
.WaitFor(web)
.WithReference(web)
.WaitFor(api)
.WithReference(api);


builder.Build().Run();
