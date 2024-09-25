var builder = DistributedApplication.CreateBuilder(args);

var cache = builder.AddRedis("redis")
    .WithRedisCommander();

var api = builder.AddProject<Projects.Api>("api")
    .WithReference(cache);

var web = builder.AddProject<Projects.MyWeatherHub>("myWeatherHub")
    .WithReference(api)
    .WithExternalHttpEndpoints();

builder.Build().Run();
