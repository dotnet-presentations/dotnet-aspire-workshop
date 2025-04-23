# Dashboard & Orchestration with .NET Aspire App Host

.NET Aspire provides APIs for modeling resources and dependencies within your distributed application. In addition to these APIs, there's tooling that enables some compelling scenarios. The orchestrator is intended for local development purposes.

Before continuing, consider some common terminology used in .NET Aspire:

- *App model*: A collection of resources that make up your distributed application ([DistributedApplication](https://learn.microsoft.com/dotnet/api/aspire.hosting.distributedapplication)). For a more formal definition, see [Define the app model](https://learn.microsoft.com/dotnet/aspire/fundamentals/app-host-overview?tabs=docker#define-the-app-model).
- *App host/Orchestrator project*: The .NET project that defines and orchestrates the *app model*, named with the **.AppHost* suffix (by convention).
- *Resource*: A [resource](https://learn.microsoft.com/dotnet/aspire/fundamentals/app-host-overview?tabs=docker#built-in-resource-types) represents a part of an application whether it be a .NET project, container, or executable, or some other resource like a database, cache, or cloud service (such as a storage service).
- *Reference*: A reference defines a connection between resources, expressed as a dependency using the `WithReference` API. For more information, see [Reference resources](https://learn.microsoft.com/dotnet/aspire/fundamentals/app-host-overview?tabs=docker#reference-resources).

## Create App Host Project

### Visual Studio & Visual Studio Code

1. Add a new project to the solution called `AppHost`:
   - Right-click on the solution and select `Add` > `New Project`.
   - Select the `.NET Aspire App Host` project template.
   - Name the project `AppHost`.
   - Click `Next` > `Create`.

    *Visual Studio*
    ![Visual Studio dialog to add a app host project](./media/vs-add-apphost.png)

    *Visual Studio Code*
    ![Visual Studio Code dialog to add a app host project](./media/vsc-add-apphost.png)

### Command Line

1. Create a new project using the `dotnet new aspire-apphost` command:

    ```bash
    dotnet new aspire-apphost -n AppHost
    ```

## Add Project References

1. Add a reference to the `Api` and `MyWeatherHub` projects in the new `AppHost` project:
   - Right-click on the `AppHost` project and select `Add` > `Reference`.
   - Check the `Api` and `MyWeatherHub` projects and click `OK`.

     > Pro Tip: In Visual Studio 2022, you can drag and drop the project onto another project to add a reference.

1. When these references are added, helper classes are automatically generated to help add them to the app model in the App Host.

## Orchestrate the Application

1. In the `AppHost` project, update the `Program.cs` file, adding the following line immediately after the `var builder = DistributedApplication.CreateBuilder(args);` line:

    ```csharp
    var api = builder.AddProject<Projects.Api>("api");

    var web = builder.AddProject<Projects.MyWeatherHub>("myweatherhub");
    ```

## Run the application

1. Set the `AppHost` project as the startup project in Visual Studio by right clicking on the `AppHost` and clicking `Set Default Project`.
1. If you are using Visual Studio Code open the `launch.json` and replace all of the contents with the following:

    ```json
    {
        "version": "0.2.0",
        "configurations": [
            {
                "name": "Run AppHost",
                "type": "dotnet",
                "request": "launch",
                "projectPath": "${workspaceFolder}\\AppHost\\AppHost.csproj"
            }
        ]
    }
    ```

1. Run the App Host using the `Run and Debug` panel in Visual Studio Code or Visual Studio.
1. The .NET Aspire Dashboard will open in your default browser and display the resources and dependencies of your application.

    ![.NET Aspire Dashboard](./media/dashboard.png)

1. Open the weather page by clicking the Endpoint for the `MyWeatherHub` project resource which will be [https://localhost:7274](https://localhost:7274).
1. Notice that both the `Api` and `MyWeatherHub` projects are running and can communicate with each other the same way as before using configuration settings.
1. Back on the Aspire Dashboard, click on the `View Logs` button to see the console logs from the `Api` and `MyWeatherHub` projects.
1. Select the `Traces` tab and select `View` on a trace where the API is being called.

    ![.NET Aspire Dashboard](./media/dashboard-trace.png)

1. Explore the `Metrics` tab to see the metrics for the `Api` and `MyWeatherHub` projects.

    ![.NET Aspire Dashboard](./media/dashboard-metrics.png)

## Create an error

1. Open the `Structured` tab on the dashboard.
1. Set the `Level` to `Error` and notice that no errors appear
1. On the `MyWeatherApp` website, click on several different cities to generate errors. Usually, clicking on 5 different cities will generate at least one error.
1. After generating the errors, the `Structured` tab will automatically update on the dashboard and display the errors.

    ![.NET Aspire Dashboard](./media/dashboard-error.png)

1. Click on the `Trace` or the `Details` to see the error message and stack trace.

## The Dashboard Resource Graph

We saw the table of resources in the .NET Aspire dashboard and that's a nice list of our resources, and we'll see that grow as our application system starts to utilize more resources.  Additionally, there is a **Graph** view of resources available by clicking the **Graph** text just above the table.

![.NET Aspire Dashboard Resource Graph](./media/dashboard-graph.png)

This graph is generated based on the references and relationships you configure for your application.

**Next**: [Module #4: Service Discovery](./4-servicediscovery.md)
