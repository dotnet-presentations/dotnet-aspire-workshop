# Health Checks

## Introduction

In this optional module, we will add health checks to our application. Health checks are used to determine the health of an application and its dependencies. They can be used to monitor the health of the application and its dependencies, and to determine if the application is ready to accept traffic.

## Adding Health Checks

### Step 1: Add Health Check Packages

First, we need to add the necessary packages to our projects. Aspire clients managed by the Aspire team automatically add support for Health Checks, and we would like to also add a health check to both the Api and MyWeatherHub projects to ensure they can reach the National Weather Service and the Api microservice respectfully.  For the API project, open the `complete/Api/Api.csproj` file and add the following package reference:

```xml
<ItemGroup>
  <PackageReference Include="AspNetCore.HealthChecks.Uris" Version="9.0.0" />
</ItemGroup>
```

For the MyWeatherHub project, open the `MyWeatherHub/MyWeatherHub.csproj` file and add the following package reference:

```xml
<ItemGroup>
  <PackageReference Include="AspNetCore.HealthChecks.Uris" Version="9.0.0" />
</ItemGroup>
```

### Step 2: Add Health Check Services

Next, we need to add the health check services to our applications.

For the API project, open the `complete/Api/Program.cs` file and add the following code:

```csharp
// Add health check services for the National Weather Service external service, carrying our User-Agent header
builder.Services.AddHealthChecks()
  .AddUrlGroup(new Uri("https://api.weather.gov/"), "NWS Weather API", HealthStatus.Unhealthy,
    configureClient: (services, client) =>
    {
      client.DefaultRequestHeaders.Add("User-Agent", "Microsoft - Aspire Demo");
    });
```

For the MyWeatherHub project, open the `complete/MyWeatherHub/Program.cs` file and add the following code:

```csharp
// Add health check services for the API service
builder.Services.AddHealthChecks()
  .AddUrlGroup(new Uri(builder.Configuration["services:api:http:0"] + "/openapi/v1.json"),
    "Weather Microservice", HealthStatus.Unhealthy);
```

### Step 3: Map Health Check Endpoints

Now, we need to add the health check endpoints to our applications.

The ServiceDefaults project already maps default health check endpoints using the `MapDefaultEndpoints()` extension method. This method is provided as part of the Aspire service defaults and maps the standard `/health` and `/alive` endpoints.

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

## Results

With these two enhancements in place, you can start the application and navigate from the dashboard into the Api or MyWeatherHub projects and see that they still run.  Once browsing either project, you can replace the URL path with the segment `/Health` and see the results of your hard work.

`Healthy`

That's it.  Your applications are healthy.

What if... you wanted more details?

## HealthChecksUI

There is a great tool available that was built with ASP.NET called [HealthChecksUI that is available from Xabaril](https://github.com/Xabaril/AspNetCore.Diagnostics.HealthChecks).  Fortunately, its also available as a Docker container and that makes it easy to integrate with Aspire.

### Step 1: Add the HealthCheckUI integration and updated Service Defaults

There are two files in the [complete folder](../complete) that contain the setup code for use of the HealthChecksUI interface.  Grab a copy of the `AppHost/HealthChecksUIResource.cs` file and the `ServiceDefaults/HealthChecks.cs` file and copy them to the same locations in your project.

These files have extensive comments if you would like to read further about how they connect and make additional health check informatoin available.

### Step 2: Add new references to ServiceDefaults

The ServiceDefaults project now needs a reference to the HealthChecks.UI.Client package to assist in formatting health information for the UI.  Update `ServiceDefaults.csproj` with this entry:

```xml
<ItemGroup>
  <PackageReference Include="AspNetCore.HealthChecks.UI.Client" Version="9.0.0" />
</ItemGroup>
```

### Step 3: Use the new health checks DefaultEndpoints

In `ServiceDefaults/Extensions.cs` we need to use the newly formatted healthchecks endpoint and connection management provided by the `HealthChecks.cs` file.  Update the `MapDefaultEndpoints` method to immediately return the new implementation from the `HealthChecks` class:

```csharp
return ServiceDefaults.HealthChecks.MapDefaultEndpoints(app);
```

### Step 4: Configure SSL to allow requests for Health

The Api and MyWeatherHub projects will now start listening on a new port that is dedicated for HealthChecks interaction.  This port is not supported for SSL requests when running locally with Docker or Podman, so we need to add an exception for these requests to allow `http` requests.  In the `Program.cs` file of each project, update the line `app.UseHttpsRedirection();` to this instead:

```csharp
// force the SSL redirect
app.UseWhen(context => !context.Request.Path.StartsWithSegments("/health"),
  builder => builder.UseHttpsRedirection());
```

### Step 5: Add the HealthChecksUI resource

The `HealthChecksUIResource.cs` file that was added to the AppHost project contains all of the information needed to start a HealthChecks container and connect our projects to it.  This may be wrapped up into a proper Aspire integration in the future that can be installed with a NuGet package, but for now the logic and configuration is all wrapped up in that file.

We can instruct Aspire to start the HealthChecksUI and configure it to listen to the MyWeatherHub and Api projects with this code.  Add this to the end of `Program.cs` just about the `builder.Build().Run();` statement:

```csharp
builder.AddHealthChecksUI("healthchecks")
  .WaitFor(web)
  .WithReference(web)
  .WaitFor(api)
  .WithReference(api);
```

We want to add the HealthChecksUI resource using a name of "healthchecks" and we want it to connect to the `web` and `api` projects after they are running.

### Step 6: Run the app, enjoy your new HealthChecksUI

Run your application, and visit the new HealthChecks resource from the dashboard.  You could see this more complete review of your application's health monitoring and child object statuses:

![HealthChecks User Interface](../media/healthchecksui.png)

## References

For more information on health checks, see the following documentation:

- [Health Checks in Aspire](https://learn.microsoft.com/en-us/dotnet/aspire/fundamentals/health-checks)
- [Health Checks in ASP.NET Core](https://learn.microsoft.com/en-us/aspnet/core/host-and-deploy/health-checks)

A more complete [HealthChecks UI sample](https://github.com/dotnet/aspire-samples/tree/main/samples/HealthChecksUI) is available on GitHub

### HealthChecks and Security

Adding health checks endpoints to applications in non-development environments has security implications. See [Health Checks in Aspire](https://aka.ms/dotnet/aspire/healthchecks) for details before enabling these endpoints in non-development environments.

> NOTE: We strongly recommend adding caching, timeouts, and security to all of your healthchecks endpoints and user interfaces before publishing them to the public internet.  The sample as demonstrated here is not recommended for production use.  Consult the links above for more information about securing health check endpoints.

**Next**: [Module #14 - GitHub Models Integration with Aspire](../Lesson-14-GitHub-Models-Integration/README.md)
