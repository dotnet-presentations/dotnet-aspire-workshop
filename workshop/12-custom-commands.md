# Custom Commands in .NET Aspire

## Introduction

Want to add superpowers to your .NET Aspire dashboard? Custom commands are here to save the day! In this module, we'll explore how to create interactive commands that let you manage your application resources directly from the Aspire dashboard - no more switching between terminals, browsers, and tools.

We'll cover:
1. Understanding the custom commands architecture
2. Creating resource-specific commands (like clearing a Redis cache)
3. Building HTTP commands for API interactions
4. Adding visual polish with icons and confirmations
5. State management and conditional command availability

Think of custom commands as buttons with brains - they know when to be available, what to do when clicked, and how to provide feedback to developers.

## What Are Custom Commands?

Custom commands in .NET Aspire are interactive actions you can perform on resources directly from the dashboard. They provide a unified way to:

- **Manage Resources**: Clear caches, restart services, trigger maintenance tasks
- **Execute Operations**: Call specific API endpoints, run database migrations, invalidate caches
- **Provide Developer Tools**: Debug helpers, data seeders, test utilities
- **Integrate with External Systems**: Trigger deployments, send notifications, update configurations

The best part? All commands are discoverable in the dashboard UI and can include rich metadata like descriptions, icons, and confirmation dialogs.

## Types of Custom Commands

.NET Aspire supports several types of custom commands:

| Command Type | Use Case | Example |
|--------------|----------|---------|
| **Resource Commands** | Direct resource manipulation | Clear Redis cache, restart container |
| **HTTP Commands** | API endpoint calls | Invalidate cache via HTTP, trigger webhooks |
| **Executable Commands** | Run external processes | Database migrations, file operations |
| **State-Aware Commands** | Context-sensitive actions | Enable when healthy, disable when offline |

## Building Our First Custom Command: Clear Redis Cache 

Let's start by adding a command to clear our Redis cache. This is perfect for development when you want to reset the cache and review cache loading scenarios.

### Creating the Redis Clear Command

First, let's create an extension method that adds a clear command to Redis resources. Create a new file `RedisResourceBuilderExtensions.cs` in your AppHost project:

```csharp
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace Aspire.Hosting;

internal static class RedisResourceBuilderExtensions
{
    public static IResourceBuilder<RedisResource> WithClearCommand(
        this IResourceBuilder<RedisResource> builder)
    {
        builder.WithCommand(
            name: "clear-cache",
            displayName: "Clear Cache",
            executeCommand: context => OnRunClearCacheCommandAsync(builder, context),
            commandOptions: new CommandOptions
            {
                IconName = "AnimalRabbitOff",
                IconVariant = IconVariant.Filled,
                UpdateState = OnUpdateResourceState,
                ConfirmationMessage = "Are you sure you want to clear the cache?",
                Description = "This command will clear all cached data in the Redis database.",
            }
        );

        return builder;
    }

    private static async Task<ExecuteCommandResult> OnRunClearCacheCommandAsync(
        IResourceBuilder<RedisResource> builder,
        ExecuteCommandContext context)
    {
        var connectionString = await builder.Resource.GetConnectionStringAsync() ??
            throw new InvalidOperationException(
                $"Unable to get the '{context.ResourceName}' connection string.");

        await using var connection = ConnectionMultiplexer.Connect(connectionString);
        var database = connection.GetDatabase();
        await database.ExecuteAsync("FLUSHALL");

        return CommandResults.Success();
    }

    private static ResourceCommandState OnUpdateResourceState(
        UpdateCommandStateContext context)
    {
        var logger = context.ServiceProvider.GetRequiredService<ILogger<Program>>();

        if (logger.IsEnabled(LogLevel.Information))
        {
            logger.LogInformation(
                "Updating resource state: {ResourceSnapshot}",
                context.ResourceSnapshot);
        }

        return context.ResourceSnapshot.HealthStatus is HealthStatus.Healthy
            ? ResourceCommandState.Enabled
            : ResourceCommandState.Disabled;
    }
}
```

### Understanding the Command Components

Let's break down what makes this command work:

#### 1. Command Registration
```csharp
builder.WithCommand(
    name: "clear-cache",                    // Internal identifier
    displayName: "Clear Cache",             // What users see
    executeCommand: context => OnRunClearCacheCommandAsync(builder, context),
    commandOptions: new CommandOptions { /* ... */ }
);
```

#### 2. Command Options
```csharp
commandOptions: new CommandOptions
{
    IconName = "AnimalRabbitOff",           // Fluent UI icon
    IconVariant = IconVariant.Filled,       // Icon style
    UpdateState = OnUpdateResourceState,    // State management
    ConfirmationMessage = "Are you sure...", // Safety prompt
    Description = "This command will...",   // Help text
}
```

#### 3. Command Execution
The `OnRunClearCacheCommandAsync` method:
- Gets the Redis connection string
- Connects to Redis
- Executes the `FLUSHALL` command
- Returns success or failure

#### 4. State Management
The `OnUpdateResourceState` method determines when the command should be available:
- **Enabled**: When Redis is healthy
- **Disabled**: When Redis is unhealthy or offline

### Wiring Up the Command

Now update your AppHost `Program.cs` to use the new command:

```csharp
var builder = DistributedApplication.CreateBuilder(args);

var cache = builder.AddRedis("cache")
    .WithClearCommand()           // 🎉 Our new command!
    .WithRedisInsight();

// ...rest of your configuration
```

## Creating HTTP Commands for API Integration

HTTP commands are perfect for triggering API operations. Let's create a command that invalidates our API cache via an HTTP endpoint.

### Adding the Cache Invalidation API Endpoint

Before we can create an HTTP command, we need an actual API endpoint to call. Let's add a cache invalidation endpoint to our API project.

Open the `NwsManager.cs` file in the `Api/Data` folder and update the `MapApiEndpoints` method to include a new cache invalidation endpoint:

```csharp
public static WebApplication? MapApiEndpoints(this WebApplication app)
{
    app.UseOutputCache();

    app.MapGet("/zones", async (Api.NwsManager manager) =>
        {
            var zones = await manager.GetZonesAsync();
            return TypedResults.Ok(zones);
        })
        .CacheOutput(policy => policy.Expire(TimeSpan.FromHours(1)))
        .WithName("GetZones")
        .WithOpenApi();

    app.MapGet("/forecast/{zoneId}", async Task<Results<Ok<Api.Forecast[]>, NotFound>> (Api.NwsManager manager, string zoneId) =>
        {
            try
            {
                var forecasts = await manager.GetForecastByZoneAsync(zoneId);
                return TypedResults.Ok(forecasts);
            }
            catch (HttpRequestException)
            {
                return TypedResults.NotFound();
            }
        })
        .CacheOutput(policy => policy.Expire(TimeSpan.FromMinutes(15)).SetVaryByRouteValue("zoneId"))
        .WithName("GetForecastByZone")
        .WithOpenApi();

    // 🎉 Add this new cache invalidation endpoint
    app.MapPost("/cache/invalidate", static async (
        [FromHeader(Name = "X-CacheInvalidation-Key")] string? header,
        IOutputCacheStore cacheStore,
        IConfiguration config) =>
    {
        var hasValidHeader = config.GetValue<string>("ApiCacheInvalidationKey") is { } key
            && header == $"Key: {key}";

        if (hasValidHeader is false)
        {
            return Results.Unauthorized();
        }

        await cacheStore.EvictByTagAsync("AllCache", CancellationToken.None);

        return Results.Ok();
    });

    return app;
}
```

You'll also need to add the required using statements at the top of the file:

```csharp
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;
```

And update the output caching configuration in the `AddNwsManager` method to include cache tags:

```csharp
services.AddOutputCache(options =>
{
    options.AddBasePolicy(builder => builder.Tag("AllCache")
        .Cache());
});
```

This endpoint:
- **Validates the invalidation key** through a secure header
- **Clears all cached data** by evicting the "AllCache" tag
- **Returns appropriate HTTP status codes** (200 OK or 401 Unauthorized)

### Building the Cache Invalidation Command

Create `ApiCommandExtensions.cs` in your AppHost project:

```csharp
namespace Aspire.Hosting;

public static class ApiCommandExtensions
{
    public static IResourceBuilder<ProjectResource> WithApiCacheInvalidation(
        this IResourceBuilder<ProjectResource> builder,
        IResourceBuilder<ParameterResource> invalidationKey)
    {
        builder.WithEnvironment("ApiCacheInvalidationKey", invalidationKey)
            .WithHttpCommand(
                path: "/cache/invalidate",
                displayName: "Invalidate Cache",
                commandOptions: new HttpCommandOptions
                {
                    Description = "Invalidates the API cache by removing all output cache.",
                    PrepareRequest = (context) =>
                    {
                        var key = invalidationKey.Resource.Value;
                        context.Request.Headers.Add("X-CacheInvalidation-Key",
                            $"Key: {key}");
                        return Task.CompletedTask;
                    },
                    Method = HttpMethod.Post,
                    IconName = "DocumentLightning",
                    IconVariant = IconVariant.Filled,
                    ConfirmationMessage = "Are you sure you want to invalidate the API cache?",
                });

        return builder;
    }
}
```

### Understanding HTTP Command Features

#### 1. Request Preparation
The `PrepareRequest` callback lets you customize the HTTP request:
```csharp
PrepareRequest = (context) =>
{
    var key = invalidationKey.Resource.Value;
    context.Request.Headers.Add("X-CacheInvalidation-Key", $"Key: {key}");
    return Task.CompletedTask;
}
```

#### 2. HTTP Method Configuration
```csharp
Method = HttpMethod.Post,    // GET, POST, PUT, DELETE, etc.
```

#### 3. Security Integration
The command uses a parameter for the invalidation key, ensuring secure API access.

### Using the HTTP Command

Update your AppHost to include the cache invalidation command:

```csharp
var builder = DistributedApplication.CreateBuilder(args);

var invalidationKey = builder.AddParameter("ApiCacheInvalidationKey");

var cache = builder.AddRedis("cache")
    .WithClearCommand()
    .WithRedisInsight();

var api = builder.AddProject<Projects.Api>("api")
    .WithApiCacheInvalidation(invalidationKey)    // 🎉 HTTP command!
    .WithReference(cache);

// ...rest of your configuration
```

## Command UI and User Experience

### Icons and Visual Design

.NET Aspire uses [Fluent UI icons](https://www.fluentui-blazor.net/Icon) for command buttons. Here are some popular choices:  

| Icon Name | Use Case | Visual Style |
|-----------|----------|--------------|
| `Delete` | Clear, remove, reset | Destructive actions |
| `Refresh` | Restart, reload | Refresh operations |
| `DocumentLightning` | Fast operations | Performance actions |
| `AnimalRabbitOff` | Stop, disable | Toggle off states |
| `Play` | Start, execute | Execution actions |
| `Settings` | Configure, setup | Configuration |

### Confirmation Messages

For potentially destructive operations, always include confirmation messages:

```csharp
ConfirmationMessage = "Are you sure you want to clear the cache?"
```

This creates a dialog that users must confirm before the command executes.

### Descriptions and Help Text

Provide clear descriptions to help users understand what commands do:

```csharp
Description = "This command will clear all cached data in the Redis database."
```

## Advanced Command Patterns

### New in .NET Aspire 9.4: Resource Command Service

.NET Aspire 9.4 introduces the `ResourceCommandService` API for executing commands programmatically. This enables scenarios like composite commands that coordinate multiple operations or unit testing of commands.

```csharp
// Add a composite command that coordinates multiple operations
var api = builder.AddProject<Projects.Api>("api")
    .WithReference(database)
    .WithReference(cache)
    .WithCommand("reset-all", "Reset Everything", async (context, ct) =>
    {
        var logger = context.ServiceProvider.GetRequiredService<ILogger<Program>>();
        var commandService = context.ServiceProvider.GetRequiredService<ResourceCommandService>();
        
        logger.LogInformation("Starting full system reset...");
        
        try
        {
            // Execute other resource commands programmatically
            var flushResult = await commandService.ExecuteCommandAsync(cache.Resource, "clear", ct);
            var restartResult = await commandService.ExecuteCommandAsync(database.Resource, "restart", ct);

            if (!restartResult.Success || !flushResult.Success)
            {
                return CommandResults.Failure("System reset failed");
            }
            
            logger.LogInformation("System reset completed successfully");
            return CommandResults.Success();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "System reset failed");
            return CommandResults.Failure(ex);
        }
    },
    displayDescription: "Reset cache and restart database in coordinated sequence",
    iconName: "ArrowClockwise");
```

#### Testing Commands with ResourceCommandService

You can also use `ResourceCommandService` in unit tests:

```csharp
[Fact]
public async Task Should_ResetCache_WhenTestStarts()
{
    var builder = DistributedApplication.CreateBuilder();
    
    var cache = builder.AddRedis("test-cache")
        .WithClearCommand();

    var api = builder.AddProject<Projects.TestApi>("test-api")
        .WithReference(cache);

    await using var app = builder.Build();
    await app.StartAsync();
    
    // Reset cache before running test using ResourceCommandService
    var result = await app.ResourceCommands.ExecuteCommandAsync(
        cache.Resource, 
        "clear", 
        CancellationToken.None);
        
    Assert.True(result.Success, $"Failed to reset cache: {result.ErrorMessage}");
}
```

### Conditional Command Availability

Commands can be enabled or disabled based on resource state:

```csharp
UpdateState = (context) =>
{
    // Enable only when resource is healthy
    return context.ResourceSnapshot.HealthStatus is HealthStatus.Healthy
        ? ResourceCommandState.Enabled
        : ResourceCommandState.Disabled;
}
```

### Error Handling

Always handle errors gracefully in your command implementations:

```csharp
private static async Task<ExecuteCommandResult> OnRunCommandAsync(
    ExecuteCommandContext context)
{
    try
    {
        // Your command logic here
        return CommandResults.Success();
    }
    catch (Exception ex)
    {
        return CommandResults.Failure(ex.Message);
    }
}
```

### Logging and Observability

Include logging in your commands for better debugging:

```csharp
private static async Task<ExecuteCommandResult> OnRunCommandAsync(
    ExecuteCommandContext context)
{
    var logger = context.ServiceProvider.GetRequiredService<ILogger<Program>>();
    
    logger.LogInformation("Executing command for resource {ResourceName}", 
        context.ResourceName);
    
    // Command logic...
    
    logger.LogInformation("Command completed successfully");
    return CommandResults.Success();
}
```

## Real-World Command Examples

### Database Migration Command

```csharp
public static IResourceBuilder<ProjectResource> WithMigrationCommand(
    this IResourceBuilder<ProjectResource> builder)
{
    return builder.WithCommand(
        name: "run-migrations",
        displayName: "Run Migrations",
        executeCommand: async (context) =>
        {
            // Execute EF migrations
            var connectionString = await GetConnectionStringAsync(context);
            await RunMigrationsAsync(connectionString);
            return CommandResults.Success();
        },
        commandOptions: new CommandOptions
        {
            IconName = "Database",
            Description = "Runs Entity Framework database migrations",
            ConfirmationMessage = "Run database migrations?"
        });
}
```

## Testing Your Commands

### Manual Testing

1. **Start your Aspire application**: `dotnet run` in the AppHost project
2. **Open the dashboard**: Usually at `https://localhost:17137` for this sample
3. **Find your resources**: Look for the new command buttons under the ellipsis button in the table of resources
4. **Test the commands**: Click and verify they work as expected

## Best Practices for Custom Commands

### 1. **Keep Commands Focused**
Each command should do one thing well. Don't create mega-commands that perform multiple unrelated operations.

### 2. **Use Descriptive Names**
Command names should clearly indicate what they do:
- ✅ `clear-cache`, `restart-service`, `run-migrations`
- ❌ `action1`, `do-stuff`, `command`

### 3. **Handle Failures Gracefully**
Always return appropriate results and log errors:
```csharp
try
{
    await PerformOperation();
    return CommandResults.Success();
}
catch (Exception ex)
{
    logger.LogError(ex, "Command failed");
    return CommandResults.Failure(ex.Message);
}
```

### 4. **Use Confirmations for Destructive Operations**
Any command that deletes, clears, or modifies data should require confirmation.

### 5. **Implement State Management**
Disable commands when they shouldn't be available (e.g., when a service is offline).

### 6. **Provide Good UX**
- Use meaningful icons
- Write clear descriptions
- Choose appropriate confirmation messages

## Conclusion

Custom commands transform the .NET Aspire dashboard from a passive monitoring tool into an active development environment. They provide:

- **Developer Productivity**: Common operations at your fingertips
- **Consistency**: Standardized way to interact with resources
- **Safety**: Built-in confirmations and state management
- **Discoverability**: All commands visible in one place

The examples we've built - Redis cache clearing and API cache invalidation - are just the beginning. You can create commands for:

- Database operations (migrations, seeding, backups)
- Service management (restarts, scaling, configuration updates)
- Development tools (test data generation, log clearing)
- Integration triggers (webhooks, notifications, deployments)

Start with simple commands and gradually build more sophisticated operations as your application grows. Your future self (and your teammates) will thank you for the productivity boost!

**Next**: [Module #13: Health Checks](13-healthchecks.md)
