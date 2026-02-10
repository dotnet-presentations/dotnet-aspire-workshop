# Aspire Integrations

Aspire integrations are a curated suite of NuGet packages specifically selected to facilitate the integration of cloud-native applications with prominent services and platforms, e.g. Redis, PostgreSQL, etc. Each integration furnishes essential cloud-native functionalities through either automatic provisioning or standardized configuration patterns.

Aspire integrations come in two distinct flavors: **hosting integrations**, and **client integrations**. Hosting integrations are used to model and configure various resources in a Aspire app, while client integrations are used to map configuration to various client libraries.

There is an ever growing list [Aspire integrations](https://learn.microsoft.com/dotnet/aspire/fundamentals/integrations-overview?tabs=dotnet-cli#available-integrations) created and shipped by Microsoft and the community. Aspire is flexible and anyone can create their own package to integrate with their own services.

Let's improve our application by adding an integration to it. We will add a integration that will help us to connect to a Redis cache to improve our API performance.

## Add Redis Integration to App Host

To add the Redis hosting integration to our App Host, we need to install the `Aspire.Hosting.Redis` NuGet package. This package provides the necessary pieces to configure the service in the App Host. Redis is provided through a container image in this workshop, and when we start the Aspire App Host, it will automatically download the Redis container image and start the Redis server.

With the NuGet package installed, we can add Redis to our App Host:

1. Open the `Program.cs` file in the `AppHost` project.
1. Add the following code under `var builder = DistributedApplication.CreateBuilder(args);`

    ```csharp
    var cache = builder.AddRedis("cache");
    ```

    Here, we have configured the Redis cache with the name `cache`. This name is used to identify the cache in the `Api` or `MyWeatherHub`.
1. Update the `api` in the App Host with a reference to the cache.

    ```csharp
    var api = builder.AddProject<Projects.Api>("api")
                     .WithReference(cache);
    ```

1. Add [Redis Insight](https://github.com/RedisInsight/RedisInsight), a Redis management tool from the Redis team. As part of the `Aspire.Hosting.Redis` package, Redis Insight is available in the same integration. Update the Redis resource to call the `WithRedisInsight()` method on the returned builder:

    ```csharp
    var cache = builder.AddRedis("cache")
                       .WithRedisInsight();
    ```

## Run the application

We haven't made any changes to the `Api` or `MyWeatherHub` projects, but we can see the Redis cache start when we start the App Host.

> [!IMPORTANT]
> Since Redis runs in a container you will need to ensure that a container runtime is running on your machine (Docker Desktop or Podman). See the [container runtime setup instructions](https://learn.microsoft.com/dotnet/aspire/fundamentals/setup-tooling#container-runtime) for Podman.

1. Ensure Docker Desktop or Podman is running.
1. Start the App Host project.
1. You will see both the Redis container and Redis Insight container download and start in both the dashboard and in Docker Desktop.

    ![Redis running in dashboard and desktop](../media/redis-started.png)

## Integrate Output Caching in API

There are two types of caching that we could integrate into our ASP.NET Core applications:

- **Output caching**: A configurable, extensible caching method for storing entire HTTP responses for future requests.
- **Distributed caching**: A cache shared by multiple app servers that allows you to cache specific pieces of data. A distributed cache is typically maintained as an external service to the app servers that access it and can improve the performance and scalability of an ASP.NET Core app.

We will add the _Output caching_ Redis client integration to our `Api` project. This integration will help us to cache the response of our API in Redis cache.

1. Install the `Aspire.StackExchange.Redis.OutputCaching` NuGet package in the `Api` project to get access to the Redis APIs.
1. Open the `Program.cs` file in the `Api` project.
1. Add the following code under the `var builder = WebApplication.CreateBuilder(args);` at the top of the file:

    ```csharp
    builder.AddRedisOutputCache("cache");
    ```

    > Note that we are using the "cache" name to reference the Redis cache that we configured in the App Host.
1. The `NwsManager` has already been configured to use Output caching, but with a memory cache. We will update it to use the Redis cache. Open the `NwsManager.cs` file in the `Data` folder.
1. In the `NwsManagerExtensions` class you will find a `AddNwsManager` method.
1. **UPDATE** the existing `AddOutputCache` configuration to add cache tagging for better cache management:

    Find this code:
    ```csharp
    // Add default output caching
    services.AddOutputCache(options =>
    {
        options.AddBasePolicy(builder => builder.Cache());
    });
    ```

    And **modify** it to:
    ```csharp
    // Add default output caching
    services.AddOutputCache(options =>
    {
        options.AddBasePolicy(builder => builder.Tag("AllCache")
                                .Cache());
    });
    ```

    This adds a cache tag that will be useful for cache invalidation. Because we configured the application to use Redis cache in the `Program.cs` file, this output cache configuration will now operate against Redis instead of the in-memory store.

    > Note: We kept the `AddOutputCache` configuration but added tagging. The Redis output cache integration works alongside the existing output cache policies, directing them to use Redis instead of the in-memory store.

## Run the updated application

1. Start the App Host project and open the `MyWeatherHub` project from the dashboard.
1. Click on a city and then click on it again. You will see that the response is cached and the second request is much faster than the first one under the `Traces` tab.

    ![Output caching in action](../media/output-caching.png)

1. You can also see the cached response in Redis Insight. Open Redis Insight by clicking on the `cache-insight` endpoint in the dashboard. Under stats you will see connections and commands processed.

    ![Redis Insight](../media/redis-insight.png)

1. In addition, you can see logs for the Redis cache and Redis Insight in the `Console` tab.

    ![Redis logs](../media/redis-logs.png)

## Custom Redis Containers

Aspire integrations are flexible and customizable. By default, the Redis integration uses a Redis container image from Docker Hub. However, you can use your own Redis container image by providing the image name and tag after the `AddRedis` method. For example, if you have a custom Redis container image such as [Garnet](https://github.com/microsoft/garnet), you can provide the image registry, name, and tag in the App Host as follows:

```csharp
var cache = builder.AddRedis("cache")
    .WithImageRegistry("ghcr.io")
    .WithImage("microsoft/garnet")
    .WithImageTag("latest");
```

> Note: there is a new [Garnet integration](https://learn.microsoft.com/dotnet/aspire/caching/stackexchange-redis-integration?pivots=garnet&tabs=dotnet-cli) that simplifies this integration, but the above highlights the flexibility of the integrations.

1. Run the application and you will now see Garnet running in the dashboard and in Docker Desktop.

    ![Garnet running in dashboard and desktop](../media/garnet-started.png)

1. You can also see the logs for Garnet in the `Console` tab.

    ![Garnet logs](../media/garnet-logs.png)

## Summary

In this section, we added a Redis hosting integration to the App Host and Redis output caching client integration in the API. We saw how the response was cached in the Redis cache and how the second and subsequent requests were much faster than the first one. We also saw how to use Redis Insight to manage the Redis cache.

There are many more Aspire integrations available that you can use to integrate with your services. You can find the list of available integrations [in the Aspire documentation](https://learn.microsoft.com/dotnet/aspire/fundamentals/integrations-overview?tabs=dotnet-cli#available-integrations).

A natural next step would be to integrate a database or leverage Azure Redis Cache as a hosted solution. Integrations for these and more are [available on NuGet](https://www.nuget.org/packages?q=owner%3Aaspire+tags%3Aintegration).

**Next**: [Module #6: Telemetry](../Lesson-06-Telemetry/README.md)
