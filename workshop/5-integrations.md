# .NET Aspire Integrations

.NET Aspire integrations are a curated suite of NuGet packages specifically selected to facilitate the integration of cloud-native applications with prominent services and platforms, including but not limited to Redis and PostgreSQL. Each integration furnishes essential cloud-native functionalities through either automatic provisioning or standardized configuration patterns.

.NET Aspire integrations come in two distinct flavors: **hosting integrations**, and **client integrations**. Hosting integrations are used to model and configure various resources in a .NET Aspire app, while client integrations are used to map configuration to various client libraries.

There is an ever growing list [.NET Aspire integrations](https://learn.microsoft.com/dotnet/aspire/fundamentals/integrations-overview?tabs=dotnet-cli#available-integrations) created and shipped by Microsoft and the community. .NET Aspire is flexible and anyone can create their own package to integrate with their own services.

Let's improve our application by adding an integration to it. We will add a integration that will help us to connect to a Redis cache to improve our API performance.

## Add Redis Integration to App Host

To add the Redis hosting integration to our App Host, we need to install the `Aspire.Hosting.Redis` NuGet package. This package provides the necessary pirces to configure the service in the App Host. Redis is provided through a container image in this workshop, and when we start the .NET Aspire App Host, it will automatically download the Redis container image and start the Redis server.

With the NuGet installed we can configure it.

1. Open the `Program.cs` file in the `AppHost` project.
1. Add the following code under `var builder = DistributedApplication.CreateBuilder(args);`

	```csharp
	var cache = builder.AddRedis("cache")
	```
	Here, we have configured the Redis cache with the name `cache`. This name is used to identify the cache in the `Api` or `MyWeatherHub`.
1. Update the `api` in the App Host with a reference to the cache.

	```csharp
	var api = builder.AddProject<Projects.Api>("api")
			.WithReference(cache);
	```

1. Additionally, we could configure [Redis Commander](https://joeferner.github.io/redis-commander/), a Redis management tool. As part of the `Aspire.Hosting.Redis` package,  Redis Commander is available in the same integration. To add Redis Commander, add the following code under to the newly added Redis configuration.

	```csharp
	var cache = builder.AddRedis("cache")
			.WithRedisCommander();
	```

## Run the application

We haven't made any changes to the `Api` or `MyWeatherHub` projects, but we can see the Redis cache start when we start the App Host. 

> [!IMPORTANT]
> Since Redis runs in a container you will need to ensure that Docker is running on your machine.

1. Start Docker Desktop or Podman
1. Start the App Host project.
1. You will see both the Redis container and Redis Commander container download and start in both the dashboard and in Docker Desktop.

	![Redis running in dashboard and desktop](./media/redis-started.png)

## Integrate Output Caching in API

There are two types of caching that we could integrate into our ASP.NET Core applications:

- **Output caching**: A configurable, extensible caching method for storing entire HTTP responses for future requests.
- **Distributed caching**: A cache shared by multiple app servers that allows you to cache specific pieces of data. A distributed cache is typically maintained as an external service to the app servers that access it and can improve the performance and scalability of an ASP.NET Core app.

We will add the _Output caching_ Redis client integration to our Api project. This integration will help us to cache the response of our API in Redis cache.

1. Install the `Aspire.StackExchange.Redis.OutputCaching` NuGet package in the `Api` project to get access to the Redis APIs.
1. Open the `Program.cs` file in the `Api` project.
1. Add the following code under the `var builder = WebApplication.CreateBuilder(args);` at the top of the file:

	```csharp
	builder.AddRedisOutputCache("cache");
	```

	> Note that we are using the "cache" name to reference the Redis cache that we configured in the App Host.
1. The `NwsManager` has already been configured to use Output caching, but with a memory cache. We will update it to use the Redis cache. Open the `NwsManager.cs` file in the `Data` folder.
1. In the `NwsManagerExtensions` class you will find a `AddNwsManager` method.
1. **DELETE** the following code:

	```csharp
	// Add default output caching
	services.AddOutputCache(options =>
	{
		options.AddBasePolicy(builder => builder.Cache());
	});
	```

	Because we configured the application to use Redis cache in the `Program.cs` file, we no longer need to add the default output caching policy.


## Run the application
1. Start the App Host project and open the `MyWeatherHub` project from the dashboard
1. Click on a city and then click on it again. You will see that the response is cached and the second request is much faster than the first one under the `Traces` tab.

	![Output caching in action](./media/output-caching.png)


1. You can also see the cached response in the Redis Commander. Open the Redis Commander by clicking on the `Redis Commander` endpoint in the dashboard. Under stats you will see connections and commands processed.

	![Redis Commander](./media/redis-commander.png)
1. In addition, you can see logs for the Redis cache and Redis Commander in the `Console` tab.

	![Redis logs](./media/redis-logs.png)


## Custom Redis Containers

.NET Aspire integrations are flexible and customizable. By default, the Redis integration uses a Redis container image from Docker Hub. However, you can use your own Redis container image by providing the image name and tag after the `AddRedis` method. For example, if you have a custom Redis container image such as [Garnet](https://github.com/microsoft/garnet), you can provide the image name and tag in the App Host as follows:

```csharp
var cache = builder.AddRedis("cache")
	.WithImage("ghcr.io/microsoft/garnet")
	.WithImageTag("latest")
	.WithRedisCommander();
```

1. Run the application and you will now see Garnet running in the dashboard and in Docker Desktop.

	![Garnet running in dashboard and desktop](./media/garnet-started.png)
1. You can also see the logs for Garnet in the `Console` tab.

	![Garnet logs](./media/garnet-logs.png)


## Summary
In this section, we added a Redis hosting integration to the App Host and Redis output caching client integration in the API. We saw how the response was cached in the Redis cache and how the second request was much faster than the first one. We also saw how to use Redis Commander to manage the Redis cache.

There are many more Aspire integrations available that you can use to integrate with your services. You can find the list of available integrations [in the .NET Aspire documentation](https://learn.microsoft.com/dotnet/aspire/fundamentals/integrations-overview?tabs=dotnet-cli#available-integrations).

A natural next step would be to integrate a database or leverage Azure Redis Cache as a hosted solution. Integrations for these and more are [available on NuGet](https://www.nuget.org/packages?q=owner%3Aaspire+tags%3Aintegration).