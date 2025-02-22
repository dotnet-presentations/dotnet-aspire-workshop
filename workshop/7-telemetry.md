# Telemetry Module

## Introduction

In this module, we will add more advanced telemetry to the application. This includes additional logging and tracing, distributed tracing, custom metrics, and browser telemetry support using OpenTelemetry Protocol (OTLP) over HTTP and cross-origin resource sharing (CORS).

## Additional Logging and Tracing

1. Open the `Program.cs` file in the `Api` and `MyWeatherHub` projects.
2. Add the following code to configure additional logging and tracing:

    ```csharp
    builder.Logging.AddOpenTelemetry(logging =>
    {
        logging.IncludeFormattedMessage = true;
        logging.IncludeScopes = true;
    });

    builder.Services.AddOpenTelemetry()
        .WithMetrics(metrics =>
        {
            metrics.AddAspNetCoreInstrumentation()
                .AddHttpClientInstrumentation()
                .AddRuntimeInstrumentation();
        })
        .WithTracing(tracing =>
        {
            tracing.AddSource(builder.Environment.ApplicationName)
                .AddAspNetCoreInstrumentation()
                .AddHttpClientInstrumentation();
        });
    ```

## Distributed Tracing

1. Open the `Program.cs` file in the `Api` and `MyWeatherHub` projects.
2. Add the following code to configure distributed tracing:

    ```csharp
    builder.Services.AddOpenTelemetry()
        .WithTracing(tracing =>
        {
            tracing.AddSource(builder.Environment.ApplicationName)
                .AddAspNetCoreInstrumentation()
                .AddHttpClientInstrumentation();
        });
    ```

## Custom Metrics

1. Open the `Program.cs` file in the `Api` and `MyWeatherHub` projects.
2. Add the following code to configure custom metrics:

    ```csharp
    builder.Services.AddOpenTelemetry()
        .WithMetrics(metrics =>
        {
            metrics.AddAspNetCoreInstrumentation()
                .AddHttpClientInstrumentation()
                .AddRuntimeInstrumentation();
        });
    ```

## Browser Telemetry Support

1. Follow the guidance in the docs to enable browser telemetry support using OpenTelemetry Protocol (OTLP) over HTTP and cross-origin resource sharing (CORS).

## Telemetry Integrations

Consider using telemetry integrations including Application Insights, New Relic, DataDog, etc.

**Next**: Module #8 - Next Module
