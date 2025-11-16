# Azure Integrations with Aspire

## Introduction

Ready to take your Aspire apps to the cloud? You're in the right place! In this module, we'll explore how Aspire makes working with Azure services a breeze - both during development and in production.

We'll cover:

1. Using local emulators to develop against Azure services (without spending a dime!)
2. Adding serverless powers with Azure Functions (because who doesn't love going serverless?)
3. Storing data in Azure Cosmos DB (with local emulation magic)
4. Working with existing Azure resources (for when you've already got cloud goodies)
5. Managing infrastructure as code (because nobody likes clicking through portals)

The best part? All of these integrations follow the same patterns you've been learning throughout this workshop. Let's dive in!

## The Azure Developer Experience in Aspire

Cloud development often involves frustrating disconnects between local development and production environments. Aspire tackles these challenges head-on with:

- **Local Emulators**: Work with containerized versions of Azure services right on your laptop
- **Consistent Configuration**: Use the same code for local development and production
- **Automatic Resource Provisioning**: Generate infrastructure code for your Azure resources
- **Seamless Deployment**: Deploy easily to Azure Container Apps or other Azure services

Let's see how Aspire brings these benefits to life by building an application that uses Azure Functions and Cosmos DB.

## Local Emulators - Azure Without the Cloud

One of Aspire's superpowers is its support for local emulators of Azure services. Here's what you can use without an internet connection:

| Azure Service | Local Development Magic |
|---------------|--------------------------|
| Azure Cosmos DB | Azure Cosmos DB Emulator container (it's just like the real thing!) |
| Azure Storage | Azurite container (blob, queue, and table storage at your fingertips) |
| Azure Service Bus | Azure Service Bus Emulator container (messaging goodness) |
| Azure Functions | Native integration (works right with your Aspire app) |
| Azure Key Vault | Local emulation within Aspire (secrets, secrets, secrets) |
| Azure API Management | Local emulation within Aspire (API management without the cloud) |

These emulators let you develop and test like you're in the cloud - without spending a cent on Azure resources. They're perfect for development, testing, and even running demos when the conference WiFi inevitably fails!

## Creating a Serverless Weather App

Let's build a fun little weather application that combines Azure Functions with Cosmos DB. We'll start with a new Aspire project and add our cloud goodies step by step.

### Setting Up Our Project

#### Using Visual Studio 2022

1. Fire up Visual Studio 2022 and click **Create a new project**.
2. In the search box, type "aspire" and pick the **Aspire Starter Application** template.
3. Click **Next**.
4. Fill in the project details:
   - Project name: `AzureIntegrationDemo`
   - Choose your favorite location
   - Solution name: `AzureIntegrationDemo`
   - Use Redis: No (unchecked)
   - Create a test project: None
5. Click **Create**

Your solution now contains:

- `AzureIntegrationDemo.AppHost` - Your orchestrator project
- `AzureIntegrationDemo.ServiceDefaults` - Your trusty service defaults
- `AzureIntegrationDemo.Web` - A frontend web app
- `AzureIntegrationDemo.ApiService` - Your API backend

#### Using Command Line

1. Open your favorite terminal and run:

   ```bash
   dotnet new aspire-starter -n AzureIntegrationDemo
   cd AzureIntegrationDemo
   ```

2. This creates the same set of projects as the Visual Studio approach.

3. Open the solution in your editor of choice:
   - Visual Studio: Just double-click the `.sln` file
   - VS Code: `code .` (and select the solution when prompted)

### Adding Serverless Powers with Azure Functions

Now let's add some serverless capabilities with Azure Functions!

#### Visual Studio Path (Functions)

1. Right-click on your solution in Solution Explorer and select **Add** > **New Project**.
2. Search for "Azure Functions" and select the **Azure Functions** template.
3. Click **Next**.
4. Name it `AzureIntegrationDemo.Functions` and click **Next**.
5. Choose these settings:
   - Functions worker: `.NET Isolated`
   - Function template: `HTTP trigger`
   - Enlist in Aspire Orchestration: Checked
   - Authentication level: `Function`
6. Click **Create** to add your Functions project.
7. Add a reference from the Azure Functions project to **AzureIntegrationDemo.ServiceDefaults** project
8. Open the Functions `Program.cs` file to set up Aspire:

   ```csharp
   using Microsoft.Azure.Functions.Worker.Builder;
   using Microsoft.Extensions.Hosting;

   var builder = FunctionsApplication.CreateBuilder(args);
   builder.AddServiceDefaults();
   builder.ConfigureFunctionsWebApplication();
   builder.Build().Run();
   ```

9. Find the HTTP trigger function file that was created for you, rename it to `WeatherFunction.cs`, and update it with:

   ```csharp
   using System.Net;
   using Microsoft.Azure.Functions.Worker;
   using Microsoft.Azure.Functions.Worker.Http;
   using Microsoft.Extensions.Logging;

   namespace AzureIntegrationDemo.Functions;

   public class WeatherFunction
   {
       private readonly ILogger _logger;

       public WeatherFunction(ILoggerFactory loggerFactory)
       {
           _logger = loggerFactory.CreateLogger<WeatherFunction>();
       }

       [Function("GetWeather")]
       public HttpResponseData Run(
           [HttpTrigger(AuthorizationLevel.Function, "get", Route = "weather")] 
           HttpRequestData req)
       {
           _logger.LogInformation("Weather function processed a request. ☁️");
           
           var response = req.CreateResponse(HttpStatusCode.OK);
           response.Headers.Add("Content-Type", "application/json");
           
           var forecast = new WeatherForecast
           {
               Date = DateOnly.FromDateTime(DateTime.Now),
               TemperatureC = Random.Shared.Next(-20, 55),
               Summary = "Aspire Functions Weather - Partly Cloudy with a chance of containers!"
           };
           
           response.WriteAsJsonAsync(forecast);
           
           return response;
       }
   }

   public class WeatherForecast
   {
       public DateOnly Date { get; set; }
       public int TemperatureC { get; set; }
       public string? Summary { get; set; }
       public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
   }
   ```

#### Command Line Path (Functions)

1. Add a new Azure Functions project:

   ```bash
   dotnet new func -n AzureIntegrationDemo.Functions
   ```

2. Connect it to your ServiceDefaults:

   ```bash
   dotnet add AzureIntegrationDemo.Functions/AzureIntegrationDemo.Functions.csproj reference AzureIntegrationDemo.ServiceDefaults/AzureIntegrationDemo.ServiceDefaults.csproj
   ```

3. Create the necessary files as described in the Visual Studio path above.

### Wiring Functions into the Aspire World

Now, let's make our Functions project a full-fledged citizen in our Aspire application:

#### Visual Studio Path (Wiring Functions)

1. Right-click on the AppHost project and add a reference to the Functions project.
1. In the AppHost project, add a reference to the `Aspire.Hosting.Azure.Functions` package:

```bash
dotnet add package Aspire.Hosting.Azure.Functions --prerelease
```

1. Open the `Program.cs` file in the AppHost project and update it:

   ```csharp
    var builder = DistributedApplication.CreateBuilder(args);

    var apiService = builder.AddProject<Projects.AzureIntegrationDemo_ApiService>("apiservice");

    var functions = builder .AddAzureFunctionsProject<Projects.AzureIntegrationDemo_Functions>("functions")
        .WithHttpEndpoint(name: "functions-http"); // This makes our function available via HTTP

    builder.AddProject<Projects.AzureIntegrationDemo_Web>("webfrontend")
        .WithExternalHttpEndpoints()
        .WithReference(apiService)
        .WaitFor(apiService)
        .WithReference(functions)
        .WaitFor(functions);

    builder.Build().Run();
   ```

#### Command Line Path (Wiring Functions)

1. Add a project reference:

   ```bash
   dotnet add AzureIntegrationDemo.AppHost/AzureIntegrationDemo.AppHost.csproj reference AzureIntegrationDemo.Functions/AzureIntegrationDemo.Functions.csproj
   ```

2. Update the AppHost's Program.cs file as shown above.

## Adding Cosmos DB

Now it's time to add some persistent data storage with Azure Cosmos DB. The coolest part? We'll use the Cosmos DB Emulator locally, which means no Azure account needed for development!

### Visual Studio Path (Cosmos DB)

1. Right-click on your AppHost project and select **Manage NuGet Packages**
1. Search for and install `Aspire.Hosting.Azure.CosmosDB`
1. Let's update our AppHost to include Cosmos DB:

   ```csharp
    var builder = DistributedApplication.CreateBuilder(args);

    var cosmos = builder.AddAzureCosmosDB("cosmos-db");

    var apiService = builder.AddProject<Projects.AzureIntegrationDemo_ApiService>("apiservice");

    var functions = builder.AddAzureFunctionsProject<Projects.AzureIntegrationDemo_Functions>("functions")
        .WithReference(cosmos)
        .WithHttpEndpoint(name: "functions-http"); // This makes our function available via HTTP

    builder.AddProject<Projects.AzureIntegrationDemo_Web>("webfrontend")
        .WithExternalHttpEndpoints()
        .WithReference(apiService)
        .WaitFor(apiService)
        .WithReference(functions)
        .WaitFor(functions);

    builder.Build().Run();  
   ```

1. Add the `Aspire.Microsoft.Azure.Cosmos` to the Functions project:
1. Now let's update the `Program.cs` file of our Functions project to use Cosmos DB:

   ```csharp
    using Microsoft.Azure.Functions.Worker.Builder;
    using Microsoft.Extensions.Hosting;

    var builder = FunctionsApplication.CreateBuilder(args);
    builder.AddServiceDefaults();
    builder.AddAzureCosmosClient(connectionName: "cosmos-db");
    builder.ConfigureFunctionsWebApplication();
    builder.Build().Run();
   ```

1. Let's add a data model for our weather data. Create a `WeatherData.cs` file:

   ```csharp
   namespace AzureIntegrationDemo.Functions;

   public class WeatherData
   {
       public string Id { get; set; } = Guid.NewGuid().ToString();
       public DateOnly Date { get; set; }
       public int TemperatureC { get; set; }
       public string? Summary { get; set; }
       public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
   }
   ```

1. Now let's create a repository to work with Cosmos DB. Add a `WeatherRepository.cs` file:

   ```csharp
   using Microsoft.Azure.Cosmos;
   using Microsoft.Extensions.Configuration;
   
   namespace AzureIntegrationDemo.Functions;
   
   public class WeatherRepository
   {
       private readonly Container _container;
       
       public WeatherRepository(CosmosClient cosmosClient, IConfiguration configuration)
       {
           var databaseName = "weatherdb";
           var containerName = "forecasts";
           
           // Create container if it doesn't exist
           DatabaseResponse database = cosmosClient.CreateDatabaseIfNotExistsAsync(databaseName).GetAwaiter().GetResult();
           database.Database.CreateContainerIfNotExistsAsync(containerName, "/id").GetAwaiter().GetResult();
           
           _container = cosmosClient.GetContainer(databaseName, containerName);
       }
       
       public async Task<WeatherData> AddForecastAsync(WeatherData forecast)
       {
           return await _container.CreateItemAsync(forecast);
       }
       
       public async Task<IEnumerable<WeatherData>> GetForecastsAsync()
       {
           var query = new QueryDefinition("SELECT * FROM c ORDER BY c.Date DESC");
           var results = new List<WeatherData>();
           
           var resultSet = _container.GetItemQueryIterator<WeatherData>(query);
           while (resultSet.HasMoreResults)
           {
               var response = await resultSet.ReadNextAsync();
               results.AddRange(response);
           }
           
           return results;
       }
   }
   ```

### Command Line Path (Cosmos DB)

1. Install the required NuGet packages:

   ```bash
   dotnet add AzureIntegrationDemo.Functions/AzureIntegrationDemo.Functions.csproj package Aspire.Microsoft.Azure.Cosmos
   dotnet add AzureIntegrationDemo.Functions/AzureIntegrationDemo.Functions.csproj package Microsoft.Azure.Cosmos
   ```

2. Then follow the same file creation steps as in the Visual Studio path.

## Connecting Everything Together

Now let's wire up our Functions to use the Cosmos DB repository:

1. Register the repository in your Functions' `Program.cs`:

   ```csharp
   using Microsoft.Extensions.Hosting;
   using Microsoft.Extensions.DependencyInjection;

   var host = new HostBuilder()
       .ConfigureFunctionsWorkerDefaults()
       .ConfigureServices(services =>
       {
           services.AddServiceDefaults();
           services.AddAzureCosmosDBClient("cosmosdb");
           services.AddSingleton<WeatherRepository>(); // Register our repo
       })
       .Build();

   await host.RunAsync();
   ```

2. Update our `WeatherFunction.cs` to use the repository:

   ```csharp
   using System.Net;
   using Microsoft.Azure.Functions.Worker;
   using Microsoft.Azure.Functions.Worker.Http;
   using Microsoft.Extensions.Logging;

   namespace AzureIntegrationDemo.Functions;

   public class WeatherFunction
   {
       private readonly ILogger _logger;
       private readonly WeatherRepository _repository;

       public WeatherFunction(ILoggerFactory loggerFactory, WeatherRepository repository)
       {
           _logger = loggerFactory.CreateLogger<WeatherFunction>();
           _repository = repository;
       }

       [Function("GetWeather")]
       public async Task<HttpResponseData> Get(
           [HttpTrigger(AuthorizationLevel.Function, "get", Route = "weather")] 
           HttpRequestData req)
       {
           _logger.LogInformation("☀️ Weather function processing GET request...");
           
           var forecasts = await _repository.GetForecastsAsync();
           
           var response = req.CreateResponse(HttpStatusCode.OK);
           response.Headers.Add("Content-Type", "application/json");
           await response.WriteAsJsonAsync(forecasts);
           
           return response;
       }

       [Function("AddWeather")]
       public async Task<HttpResponseData> Post(
           [HttpTrigger(AuthorizationLevel.Function, "post", Route = "weather")] 
           HttpRequestData req)
       {
           _logger.LogInformation("🌧️ Weather function processing POST request...");
           
           var forecast = await req.ReadFromJsonAsync<WeatherData>();
           if (forecast == null)
           {
               var badResponse = req.CreateResponse(HttpStatusCode.BadRequest);
               await badResponse.WriteStringAsync("This forecast data isn't looking good! Please check your JSON.");
               return badResponse;
           }
           
           var savedForecast = await _repository.AddForecastAsync(forecast);
           
           var response = req.CreateResponse(HttpStatusCode.Created);
           response.Headers.Add("Content-Type", "application/json");
           await response.WriteAsJsonAsync(savedForecast);
           
           return response;
       }
   }
   ```

## Let's Run Our Cloud-Ready App Locally

Time to see the magic happen:

1. Set the AppHost as your startup project:
   - In Visual Studio: Right-click > Set as Startup Project
   - In command line: Use `--project AzureIntegrationDemo.AppHost`

2. Run the application:
   - Visual Studio: Hit F5 or click that Play button!
   - Command line: `dotnet run --project AzureIntegrationDemo.AppHost`

3. When the Aspire dashboard opens, you'll see:
   - The Cosmos DB emulator running in a container (no Azure account needed!)
   - Your Azure Functions project running locally
   - The web frontend and API projects

4. Click the Functions endpoint in the dashboard to test it out.
5. Want to see your data? The Cosmos DB emulator includes a data explorer at `https://localhost:8081/_explorer/index.html`
   - Password: `C2y6yDjf5/R+ob0N8A7Cgv30VRDJIWEHLM+4QDU5DE2nQ9nDuVTqobD4b8mGGyPMbIZnqyMsEcaGQy67XIw/Jw==`

Isn't it amazing? You're running "Azure" services without even being connected to Azure!

## Using Existing Azure Resources

While the local emulators are fantastic for development, eventually you'll want to connect to real Azure resources. Aspire makes this super easy with the `RunAsExisting` mode:

1. To use an existing Azure Cosmos DB, just update your AppHost:

   ```csharp
   var cosmos = builder.AddAzureCosmosDB("cosmosdb")
       .RunAsExisting("https://your-cosmos-account.documents.azure.com:443/")
       .AddDatabase("weatherdb");
   ```

2. For security, you'll need to provide connection strings. During development, User Secrets are your friend:

   ```bash
   cd AzureIntegrationDemo.AppHost
   dotnet user-secrets set ConnectionStrings:cosmosdb "AccountEndpoint=https://your-cosmos-account.documents.azure.com:443/;AccountKey=your-key;"
   ```

3. Functions work the same way:

   ```csharp
   var functions = builder.AddAzureFunctionsProject<Projects.AzureIntegrationDemo_Functions>("functions")
       .RunAsExisting("https://your-function-app.azurewebsites.net")
       .WithHttpEndpoint();
   ```

This approach gives you the best of both worlds - you can develop locally with emulators, but easily switch to real Azure resources when needed.

## Infrastructure as Code with Bicep

When it's time to deploy, you'll need to provision Azure resources. Aspire integrates with the Azure Developer CLI (`azd`) to generate Bicep templates:

1. Install the Aspire extension for azd:

   ```bash
   azd extension add aspire
   ```

2. Initialize your project for Azure:

   ```bash
   azd init
   ```

3. Generate Bicep templates:

   ```bash
   azd env provision
   ```

The generated templates in the `.azure` folder include everything you need to deploy your app:

- `main.bicep` - The main infrastructure template
- Resource-specific templates for Cosmos DB, Functions, etc.

You can customize these templates or use them directly for continuous deployment pipelines.

## Tips for Success with Azure Integrations

Here are some tips from the trenches:

1. **Consistency is Key**: Keep your parameter names consistent between development and production
2. **Secret Management**: Use user secrets during development, Key Vault in production
3. **Test Early with Emulators**: Find issues when you're still in development
4. **Mind Your Resources**: Watch those Azure resource limits and costs!
5. **Telemetry**: Use Application Insights to keep an eye on your live services

## Summary

Congratulations! You've successfully:

- Created an Aspire application with Azure Functions and Cosmos DB
- Set up local development with emulators (no Azure account needed!)
- Built a serverless API that stores and retrieves data
- Learned how to work with existing Azure resources
- Discovered how to generate infrastructure as code

With these skills, you're ready to build cloud-native applications that work seamlessly both in development and in Azure. Happy cloud computing!

**Next**: [Module #12: Custom Commands](../Lesson-12-Custom-Commands/README.md)
