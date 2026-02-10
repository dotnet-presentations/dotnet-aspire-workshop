# Service Discovery

Aspire includes functionality for configuring service discovery at development and testing time. This allows resources to refer to other resources by their name and have the concrete address details resolved at runtime. Service discovery functionality works by providing configuration in the format expected by the configuration-based endpoint resolver from the Aspire App Host project to the individual service projects added to the application model.

## Service Discovery Configuration

Currently, the `MyWeatherHub` is using static configuration in its *appsettings.json* file to connect to the `Api`. This is not ideal for several reasons including:

- The port number of the `Api` service may change.
- The IP address of the `Api` service may change.
- Multiple configuration settings would need to be defined for http and https settings.
- As we add more services, the configuration would become more complex.

To address these issues, we will use the service discovery functionality provided by the Aspire App Host project. This will allow the `MyWeatherHub` service to discover the `Api` service at runtime.

1. Open the `Program.cs` file in the `AppHost` project.
1. Earlier we added orchestration to include several projects by using the `builder.AddProject` method. This returned an `IResourceBuilder<TResource>` that can be used to reference other resources. Let's reference the `Api` project in the `MyWeatherHub` project by updating the code:

    ```csharp
    var api = builder.AddProject<Projects.Api>("api");

    var web = builder.AddProject<Projects.MyWeatherHub>("myweatherhub")
        .WithReference(api)
        .WithExternalHttpEndpoints();
    ```

1. The `WithReference` method is used to reference the `Api` project. This will inject the required configuration to allow the `MyWeatherHub` project to discover the `Api` project at runtime via its resource name, `api`.
1. If you later choose to deploy this app, you'd need the call to `WithExternalHttpEndpoints` to ensure that it's public to the outside world.

## Enabling Service Discovery in MyWeatherHub

When we added `ServiceDefaults` to the projects we automatically enrolled them in the service discovery system. This means that the `MyWeatherHub` project is already configured to use service discovery.

Some services expose multiple, named endpoints. By default, the scheme portion of the URI is used to refer to the name of the endpoint being resolved, e.g. the URI `https://basket` will resolve an endpoint named `https` on the `basket` service. By default in Aspire, project resources declare their endpoints according to the contents of their *launchSettings.json* file, so most projects will by default receive `https` and `http` named endpoints. The scheme portion of the referring URI can include multiple names separated by a `+` character and in preference order, e.g. `https+http://basket` will attempt to resolve the `https` named endpoint, and if not found it will resolve the `http` endpoint, of the `basket` service.

> [!NOTE]
> This workshop uses implicit service discovery with the default `http` and `https` named endpoints. Aspire also supports explicitly declaring and resolving additional named endpoints (via code, configuration, and Kubernetes) for advanced scenarios. See the [Named Endpoints section of the Service Discovery docs](https://learn.microsoft.com/dotnet/aspire/service-discovery/overview#named-endpoints)
 for details.

Now, let's update the `MyWeatherHub` project to use service discovery to connect to the `Api` service. This can be accomplished by updating the existing `WeatherEndpoint` configuration settings in the `appsettings.json`. This is convenient when enabling Aspire in an existing deployed application as you can continue to use your existing configuration settings.

1. Open the `appsettings.json` file in the `MyWeatherHub` project.

1. Update the `WeatherEndpoint` configuration settings to use service discovery:

    ```json
    "WeatherEndpoint": "https+http://api"
    ```

    > Multi-scheme service discovery: The value `https+http://api` tells the resolver to prefer an HTTPS endpoint for the `api` service if one exists, and fall back to HTTP otherwise. The schemes are evaluated left-to-right and separated by `+`. This lets the same configuration work for local (HTTP only) development and future HTTPS-enabled environments without further changes. Use this pattern only for internal service-to-service `HttpClient` base addresses—don’t surface the multi-scheme form directly in user-facing links.

1. The `WeatherEndpoint` configuration setting is now using service discovery to connect to the `Api` service.

Alternatively, we can update the url to not use the `WeatherEndpoint` configuration settings and instead set the URI directly in the `NwsManager` class:

1. Open the `Program.cs` file in the `MyWeatherHub` project.
1. Update the `WeatherEndpoint` configuration settings to use service discovery:

    ```csharp
    builder.Services.AddHttpClient<NwsManager>(
        static client => client.BaseAddress = new("https+http://api"));
    ```

## Run the Application

1. Run the application by pressing `F5` or selecting the `Start Debugging` option.
1. Open the `MyWeatherApp` by selecting the endpoint in the dashboard.
1. Notice that the `MyWeatherHub` app still works and is now using service discovery to connect to the `Api` service.
1. In the dashboard click on the `Details` for the `MyWeatherHub` project. This will bring up all of the settings that Aspire configured when running the app from the App Host.
1. Click on the eye icon to reveal the values and scroll to the bottom where you will see `services__api__http__0` and `services__api__https__0` configured with the correct values of the `Api` service.`

    ![Service discovery settings in the dashboard](../media/dashboard-servicediscovery.png)

## External Service Modeling

Aspire provides first-class support for modeling external services in your application graph. Modern applications frequently need to integrate with external APIs, third-party services, or existing infrastructure that isn't managed by Aspire.

### Adding External Services

External services can be added to your AppHost just like internal services, and they appear in the Aspire dashboard with health status monitoring.  Let's add an external service reference for the National Weather Service's endpoint:

```csharp
var builder = DistributedApplication.CreateBuilder(args);

// Reference an external API service
var weatherApi = builder.AddExternalService("weather-api", "https://api.weather.gov");

...

// Your services can reference external services just like internal ones
var api = builder.AddProject<Projects.Api>("api")
    .WithReference(weatherApi);
```

![Dashboard with the additional external resource](../media/external-service-resource.png)

### Benefits of External Service Modeling

- **Unified View**: All services (internal and external) appear in the Aspire dashboard
- **Health Monitoring**: External services can include health checks
- **Service Discovery**: External services work with the same service discovery patterns
- **Configuration Management**: Centralized configuration for external dependencies
- **Deployment Awareness**: External services are included in deployment manifests

This feature bridges the gap between your Aspire-managed services and the broader ecosystem of services your application depends on.

> Note: External service modeling is a development-time modeling feature. It doesn't proxy traffic—the clients still call the real external service endpoint; the AppHost provides discovery and visibility.

## Updating the API to Use the External Service

Now that we've defined the external weather API service in our AppHost, we need to update the API project to use service discovery to connect to it. The `Api` project already has an `NwsManager` class that makes HTTP requests to the National Weather Service.

1. Open the `NwsManager.cs` file in the `Api/Data` folder.
1. Locate the `AddNwsManager` extension method around line 130.
1. Update the `HttpClient` to use `https://weather-api` as the base address (no trailing slash) instead of api.weather.gov, and ensure the request path starts with a leading slash:

    ```csharp
    services.AddHttpClient<Api.NwsManager>(client =>
    {
        client.BaseAddress = new Uri("https://weather-api");
        client.DefaultRequestHeaders.Add("User-Agent", "Microsoft - Aspire Demo");
    });

    ...

    var zoneIdSegment = HttpUtility.UrlEncode(zoneId);
    var zoneUrl = $"/zones/forecast/{zoneIdSegment}/forecast";
    var forecasts = await httpClient.GetFromJsonAsync<ForecastResponse>(zoneUrl, options);

    ```

The service discovery system will now resolve `https://weather-api` to the actual URL of the National Weather Service API (`https://api.weather.gov`) that we defined in the AppHost configuration.

## Conclusion

This was just the start of what we can do with service discovery and Aspire. As our application grows and we add more services, we can continue to use service discovery to connect services at runtime. This will allow us to easily scale our application and make it more resilient to changes in the environment.

## Learn More

You can learn more about advanced usage and configuration of service discovery in the [Aspire Service Discovery](https://learn.microsoft.com/dotnet/aspire/service-discovery/overview) documentation.

**Next**: [Module #5: Integrations](../Lesson-05-Integrations/README.md)
