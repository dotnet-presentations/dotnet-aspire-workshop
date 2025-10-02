# Service Defaults aka Smart Defaults

## Introduction

.NET Aspire provides a set of smart defaults for services that are commonly used in .NET applications. These defaults are designed to help you get started quickly and provide a consistent experience across different types of applications. This includes:

- Telemetry: Metrics, Tracing, Logging
- Resiliency
- Health Checks
- Service Discovery

## Create Service Defaults Project

### Visual Studio & Visual Studio Code

1. Add a new project to the solution called `ServiceDefaults`:
   - Right-click on the solution and select `Add` > `New Project`.
   - Select the `.NET Aspire Service Defaults` project template.
   - Name the project `ServiceDefaults`.
   - Click `Next` > `Create`.

    *Visual Studio*
    ![Visual Studio dialog to add a service defaults project](../media/vs-add-servicedefaults.png)

    *Visual Studio Code*
    ![Visual Studio Code dialog to add a service defaults project](../media/vsc-add-servicedefaults.png)

### Command Line

1. Create a new project using the `dotnet new aspire-servicedefaults` command:

   ```bash
   dotnet new aspire-servicedefaults -n ServiceDefaults
   ```

## Configure Service Defaults

Before we dive into the mechanics, here’s why we do each of the next steps:

- Add project references: This lets the `Api` and `MyWeatherHub` projects consume the shared configuration, resilience, telemetry, and discovery wiring that lives in the new `ServiceDefaults` project without repeating that setup in every service.
- Call `builder.AddServiceDefaults()`: This opt‑in extension applies the opinionated “smart defaults” (OpenTelemetry logging/tracing/metrics, health checks, service discovery, and resilient HTTP handlers) so you start with production‑minded behavior immediately.
- Call `app.MapDefaultEndpoints()`: This maps development‑time health endpoints (`/health`, `/alive`) and prepares the app for later dashboard and orchestration scenarios. We’ll go deeper into these concepts in later modules; for now just know this lights up diagnostics and readiness probes early.

1. Add a reference to the `ServiceDefaults` project in the `Api` and `MyWeatherHub` projects:
   - Right-click on the `Api` project and select `Add` > `Reference`.
     - Check the `ServiceDefaults` project and click `OK`.
   - Right-click on the `MyWeatherHub` project and select `Add` > `Reference`.
     - Check the `ServiceDefaults` project and click `OK`.

   > Pro Tip: In Visual Studio 2022, you can drag and drop the project onto another project to add a reference.

1. In both the `Api` and `MyWeatherHub` projects, update their `Program.cs` files, adding the following line immediately after their `var builder = WebApplication.CreateBuilder(args);` line:

   ```csharp
   builder.AddServiceDefaults();
   ```

1. In both the `Api` and `MyWeatherHub` projects, update their `Program.cs` files,adding the following line immediately after their `var app = builder.Build();` line:

   ```csharp
   app.MapDefaultEndpoints();
   ```

## Run the application

1. Run the application using a multiple-project launch configuration in Visual Studio or Visual Studio Code:
   - Visual Studio: Right click on the `MyWeatherHub` solution and go to properties. Select the `Api` and `MyWeatherHub` as startup projects, select `OK`.
     - ![Visual Studio solution properties](../media/vs-multiproject.png)
     - Click `Start` to start and debug both projects.
   - Visual Studio Code: Run the `Api` and `MyWeatherHub` projects using the `Run and Debug` panel. We have provided a `launch.json` file with the necessary configurations to run both.
1. Test the application by navigating to the following URLs:
   - [https://localhost:7032/openapi/v1.json](https://localhost:7032/openapi/v1.json) - API OpenAPI document
   - [https://localhost:7274/](https://localhost:7274/) - MyWeatherHub
1. You should see the OpenAPI document (JSON) for the API and the MyWeatherHub home page.
1. You can also view the health checks for the API by navigating to [https://localhost:7032/health](https://localhost:7032/health).
1. You can also view the health checks for the MyWeatherHub by navigating to [https://localhost:7274/health](https://localhost:7274/health).
1. View the logs in the terminal to see the health checks and other telemetry data such as resiliency with Polly:

   ```bash
   Polly: Information: Execution attempt. Source: '-standard//Standard-Retry', Operation Key: '', Result: '200', Handled: 'False', Attempt: '0', Execution Time: '13.0649'
   ```

1. Click on 5 different cities and a "random" error will be thrown. You will see the Polly retry policy in action.

   ```bash
   Polly: Warning: Execution attempt. Source: '-standard//Standard-Retry', Operation Key: '', Result: '500', Handled: 'True', Attempt: '0', Execution Time: '9732.8258'
   Polly: Warning: Resilience event occurred. EventName: 'OnRetry', Source: '-standard//Standard-Retry', Operation Key: '', Result: '500'
   System.Net.Http.HttpClient.NwsManager.ClientHandler: Information: Sending HTTP request GET http://localhost:5271/forecast/AKZ318
   ```

**Next**: [Module #3 - Dashboard & App Host](../Lesson-03-Dashboard-AppHost/README.md)
