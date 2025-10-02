#:sdk Aspire.AppHost.Sdk@9.5.0
#:project Api
#:project MyWeatherHub

var builder = DistributedApplication.CreateBuilder(args);

var api = builder.AddProject<Projects.Api>("api");
var web = builder.AddProject<Projects.MyWeatherHub>("myweatherhub");

builder.Build().Run();