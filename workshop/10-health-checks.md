# Health Checks

## Introduction

In this module, we will add health checks to our application. Health checks are used to determine the health of an application and its dependencies. They can be used to monitor the health of the application and its dependencies, and to determine if the application is ready to accept traffic.

## Adding Health Checks

### Step 1: Add Health Check Packages

First, we need to add the necessary packages to our project. Open the `complete/Api/Api.csproj` file and add the following package references:

```xml
<ItemGroup>
  <PackageReference Include="Microsoft.Extensions.Diagnostics.HealthChecks" Version="6.0.0" />
  <PackageReference Include="AspNetCore.HealthChecks.Npgsql" Version="6.0.0" />
  <PackageReference Include="AspNetCore.HealthChecks.Redis" Version="6.0.0" />
</ItemGroup>
```

### Step 2: Add Health Check Services

Next, we need to add the health check services to our application. Open the `complete/Api/Program.cs` file and add the following code:

```csharp
builder.Services.AddHealthChecks()
    .AddNpgSql(builder.Configuration.GetConnectionString("DefaultConnection"), name: "postgresql")
    .AddRedis("localhost:6379", name: "redis")
    .AddUrlGroup(new Uri("https://api.weather.gov/"), name: "weatherapi");
```

### Step 3: Add Health Check Endpoints

Now, we need to add the health check endpoints to our application. Open the `complete/Api/Program.cs` file and add the following code:

```csharp
app.MapHealthChecks("/health");
app.MapHealthChecks("/alive", new HealthCheckOptions
{
    Predicate = r => r.Tags.Contains("live")
});
```

### Step 4: Add Considerations for Non-Development Environments

Finally, we need to add considerations for non-development environments. Open the `complete/ServiceDefaults/Extensions.cs` file and update the `MapDefaultEndpoints` method as follows:

```csharp
public static WebApplication MapDefaultEndpoints(this WebApplication app)
{
    if (app.Environment.IsDevelopment())
    {
        app.MapHealthChecks("/health");
        app.MapHealthChecks("/alive", new HealthCheckOptions
        {
            Predicate = r => r.Tags.Contains("live")
        });
    }
    else
    {
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

## References

For more information on health checks, see the following documentation:

- [Health Checks in .NET Aspire](https://learn.microsoft.com/en-us/dotnet/aspire/fundamentals/health-checks)
- [Health Checks in ASP.NET Core](https://learn.microsoft.com/en-us/aspnet/core/host-and-deploy/health-checks)

## HealthChecksUI Sample

You can also add a UI for your health checks using the [HealthChecksUI sample](https://github.com/dotnet/aspire-samples/tree/main/samples/HealthChecksUI). This sample shows how to add the UI as a container and provides a link to the sample for those who are interested.
