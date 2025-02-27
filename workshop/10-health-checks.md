# Health Checks

## Introduction

In this module, we will add health checks to our application. Health checks are used to determine the health of an application and its dependencies. They can be used to monitor the health of the application and its dependencies, and to determine if the application is ready to accept traffic.

## Adding Health Checks

### Step 1: Add Health Check Packages

First, we need to add the necessary packages to our projects. For the API project, open the `complete/Api/Api.csproj` file and add the following package references:

```xml
<ItemGroup>
  <PackageReference Include="Microsoft.Extensions.Diagnostics.HealthChecks" Version="9.0.2" />
  <PackageReference Include="AspNetCore.HealthChecks.Redis" Version="9.0.0" />
  <PackageReference Include="AspNetCore.HealthChecks.Uris" Version="9.0.0" />
</ItemGroup>
```

For the MyWeatherHub project, open the `complete/MyWeatherHub/MyWeatherHub.csproj` file and add the following package references:

```xml
<ItemGroup>
  <PackageReference Include="Microsoft.Extensions.Diagnostics.HealthChecks" Version="9.0.2" />
  <PackageReference Include="AspNetCore.HealthChecks.Npgsql" Version="9.0.0" />
</ItemGroup>
```

### Step 2: Add Health Check Services

Next, we need to add the health check services to our applications.

For the API project, open the `complete/Api/Program.cs` file and add the following code:

```csharp
// Add health check services for redis cache and external service
builder.Services.AddHealthChecks()
    .AddRedis("localhost:6379", name: "redis")
    .AddUrlGroup(new Uri("https://api.weather.gov/"), name: "weatherApi");
```

For the MyWeatherHub project, open the `complete/MyWeatherHub/Program.cs` file and add the following code:

```csharp
// Add health check services for database
builder.Services.AddHealthChecks()
    .AddNpgSql(builder.Configuration.GetConnectionString("weatherdb"), name: "postgresql");
```

### Step 3: Map Health Check Endpoints

Now, we need to add the health check endpoints to our applications.

The ServiceDefaults project already maps default health check endpoints using the `MapDefaultEndpoints()` extension method. This method is provided as part of the .NET Aspire service defaults and maps the standard `/health` and `/alive` endpoints.

To use these endpoints, simply call the method in your application's `Program.cs` file:

```csharp
app.MapDefaultEndpoints();
```

If you need to add additional health check endpoints, you can add them like this in the Api's `Program.cs` file:

```csharp
// Add health check endpoints for /health and /alive
app.MapHealthChecks("/health");
app.MapHealthChecks("/alive", new HealthCheckOptions
{
    Predicate = r => r.Tags.Contains("live")
});
```

### Step 4: Understand the Default Health Check Implementation

The default implementation in ServiceDefaults/Extensions.cs already includes smart behavior for handling health checks in different environments:

```csharp
public static WebApplication MapDefaultEndpoints(this WebApplication app)
{
    // Adding health checks endpoints to applications in non-development environments has security implications.
    // See https://aka.ms/dotnet/aspire/healthchecks for details before enabling these endpoints in non-development environments.
    if (app.Environment.IsDevelopment())
    {
        // All health checks must pass for app to be considered ready to accept traffic after starting
        app.MapHealthChecks("/health");

        // Only health checks tagged with the "live" tag must pass for app to be considered alive
        app.MapHealthChecks("/alive", new HealthCheckOptions
        {
            Predicate = r => r.Tags.Contains("live")
        });
    }
    else
    {
        // Considerations for non-development environments
        app.MapHealthChecks("/health", new HealthCheckOptions
        {
            Predicate = _ => true,
            ResponseWriter = async (context, report) =>
            {
                context.Response.ContentType = "application/json";
                var result = JsonSerializer.Serialize(new
                {
                    status = report.Status.ToString(),
                    checks = report.Entries.Select(e => new
                    {
                        name = e.Key,
                        status = e.Value.Status.ToString(),
                        exception = e.Value.Exception?.Message,
                        duration = e.Value.Duration.ToString()
                    })
                });
                await context.Response.WriteAsync(result);
            }
        });

        app.MapHealthChecks("/alive", new HealthCheckOptions
        {
            Predicate = r => r.Tags.Contains("live"),
            ResponseWriter = async (context, report) =>
            {
                context.Response.ContentType = "application/json";
                var result = JsonSerializer.Serialize(new
                {
                    status = report.Status.ToString(),
                    checks = report.Entries.Select(e => new
                    {
                        name = e.Key,
                        status = e.Value.Status.ToString(),
                        exception = e.Value.Exception?.Message,
                        duration = e.Value.Duration.ToString()
                    })
                });
                await context.Response.WriteAsync(result);
            }
        });
    }

    return app;
}
```

The implementation includes different approaches for development and production environments:

- In development: Simple endpoints for quick diagnostics
- In production: More detailed JSON output with additional security considerations

## References

For more information on health checks, see the following documentation:

- [Health Checks in .NET Aspire](https://learn.microsoft.com/en-us/dotnet/aspire/fundamentals/health-checks)
- [Health Checks in ASP.NET Core](https://learn.microsoft.com/en-us/aspnet/core/host-and-deploy/health-checks)

## HealthChecksUI Sample

You can also add a UI for your health checks using the [HealthChecksUI sample](https://github.com/dotnet/aspire-samples/tree/main/samples/HealthChecksUI). This sample shows how to add the UI as a container and provides a link to the sample for those who are interested.
