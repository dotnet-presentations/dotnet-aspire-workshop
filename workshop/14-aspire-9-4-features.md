# Exploring .NET Aspire 9.4 Features

## Introduction

.NET Aspire 9.4 brings exciting new capabilities that enhance the developer experience and expand deployment options. In this module, we'll explore the key new features that make building and managing cloud-native applications even easier.

## 🛠️ Aspire CLI Generally Available

The Aspire CLI is now generally available and provides a streamlined command-line experience for working with Aspire projects.

### Key CLI Commands

- **`aspire new`** - Create new Aspire projects from templates
- **`aspire run`** - Find and run the AppHost from anywhere in your repo
- **`aspire add`** - Add hosting integration packages to your AppHost
- **`aspire config`** - Configure Aspire settings and feature flags
- **`aspire publish`** - Generate deployment artifacts (Preview)

### Try the CLI

If you installed the Aspire CLI during setup, try these commands:

```bash
# Create a new Aspire project
aspire new aspire-starter -n MyNewApp

# Add Redis integration to an existing AppHost
aspire add redis

# Configure Aspire settings
aspire config list
```

## 🎛️ Interactive Parameter Prompting

One of the most developer-friendly features in 9.4 is interactive parameter prompting. Your Aspire application can now automatically prompt for missing configuration values in the dashboard.

### Hands-on: Add Interactive Parameters

Let's add some interactive parameters to our weather application:

1. Open your `AppHost/Program.cs` file (in the complete or start folder)

2. Add interactive parameters for API configuration:

```csharp
var builder = DistributedApplication.CreateBuilder(args);

// Add interactive parameters - these will prompt if not provided
var apiKey = builder.AddParameter("weather-api-key", secret: true)
    .WithDescription("API key for external weather service authentication");

var cacheTimeout = builder.AddParameter("cache-timeout")
    .WithDescription("Cache timeout in seconds for weather data")
    .WithCustomInput(p => new InteractionInput
    {
        InputType = InputType.Number,
        Label = "Cache Timeout (seconds)",
        Placeholder = "Enter timeout (30-3600)",
        Description = p.Description
    });

var environment = builder.AddParameter("deployment-environment")
    .WithDescription("Target deployment environment")
    .WithCustomInput(p => new InteractionInput
    {
        InputType = InputType.Choice,
        Label = "Environment",
        Description = p.Description,
        Options = new[]
        {
            KeyValuePair.Create("dev", "Development"),
            KeyValuePair.Create("staging", "Staging"),
            KeyValuePair.Create("prod", "Production")
        }
    });

// Use parameters in your services
var api = builder.AddProject<Projects.Api>("api")
    .WithEnvironment("WEATHER_API_KEY", apiKey)
    .WithEnvironment("CACHE_TIMEOUT", cacheTimeout)
    .WithEnvironment("ENVIRONMENT", environment);
```

3. Run your application and observe the interactive prompts in the dashboard when parameters are missing.

## 🔄 Resource Lifecycle Events

The new fluent lifecycle event API makes it easier to hook into resource lifecycle events:

```csharp
var database = builder.AddPostgres("postgres")
    .AddDatabase("weatherdb")
    .OnResourceReady(async (db, evt, ct) =>
    {
        // Seed the database when it's ready
        var connectionString = await db.ConnectionStringExpression.GetValueAsync(ct);
        // Perform database seeding logic here
        var logger = evt.Services.GetRequiredService<ILogger<Program>>();
        logger.LogInformation("Database {Name} is ready for seeding", db.Name);
    });

var api = builder.AddProject<Projects.Api>("api")
    .WithReference(database)
    .OnBeforeResourceStarted(async (resource, evt, cancellationToken) =>
    {
        // Pre-startup validation
        var logger = evt.Services.GetRequiredService<ILogger<Program>>();
        logger.LogInformation("Starting resource {Name}", resource.Name);
    })
    .OnResourceReady(async (resource, evt, cancellationToken) =>
    {
        // Resource is fully ready
        var logger = evt.Services.GetRequiredService<ILogger<Program>>();
        logger.LogInformation("Resource {Name} is ready to serve requests", resource.Name);
    });
```

## 🎨 Enhanced Dashboard Features

### Parameter and Connection String Visibility

Parameters and connection strings are now visible in the dashboard, making it easier to debug configuration issues during development.

### Console Log Text Wrapping

The dashboard now includes a toggle for text wrapping in console logs, helping with viewing long log lines.

### Hidden Resource Visibility

You can now show/hide hidden resources in the dashboard to see internal infrastructure components when needed.

## 🔗 Enhanced Endpoint URL Support

.NET Aspire 9.4 supports custom domains and `*.localhost` subdomains:

```csharp
var api = builder.AddProject<Projects.Api>("api")
    .WithEndpoint("https", e => e.TargetHost = "0.0.0.0"); // Generates both localhost and machine name URLs

// Custom subdomain support
var adminApi = builder.AddProject<Projects.AdminApi>("admin")
    .WithEndpoint("https", e => 
    {
        e.Port = 5001;
        e.TargetHost = "admin.localhost"; // Custom localhost subdomain
    });
```

##  Azure Enhancements

### Azure Key Vault Secret Management

New strongly-typed secret APIs:

```csharp
var secrets = builder.AddAzureKeyVault("secrets");

// Add secrets from parameters
var apiKey = builder.AddParameter("api-key", secret: true);
var secretRef = secrets.AddSecret("api-key", apiKey);

// Reference existing secrets
var existingSecret = secrets.GetSecret("ExistingSecretName");

// Use in services
var api = builder.AddProject<Projects.Api>("api")
    .WithEnvironment("API_KEY", secretRef)
    .WithEnvironment("EXISTING_SECRET", existingSecret);
```

### Azure Cosmos DB Enhancements

Support for hierarchical partition keys and serverless mode:

```csharp
var cosmos = builder.AddAzureCosmosDB("cosmos"); // Defaults to serverless in 9.4

var database = cosmos.AddCosmosDatabase("ecommerce");

// Hierarchical partition keys (up to 3 levels)
var productsContainer = database.AddContainer("products", 
    ["/category", "/subcategory", "/brand"]);
```

## 🤖 New AI Integrations

### GitHub Models Integration

```csharp
var model = builder.AddGitHubModel("chat-model", "gpt-4o-mini");

var chatService = builder.AddProject<Projects.ChatService>("chat")
    .WithReference(model);
```

### Azure AI Foundry Integration

```csharp
var foundry = builder.AddAzureAIFoundry("foundry");
var chat = foundry.AddDeployment("chat", "gpt-4o-mini", "1", "Microsoft");

var aiService = builder.AddProject<Projects.AIService>("ai")
    .WithReference(chat);
```

## 🔧 Database Improvements

All database providers now support the consistent `WithInitFiles()` method:

```csharp
// MongoDB
var mongo = builder.AddMongoDB("mongo")
    .WithInitFiles("./mongo-init");

// MySQL  
var mysql = builder.AddMySql("mysql", password: builder.AddParameter("mysql-password"))
    .WithInitFiles("./mysql-init");

// PostgreSQL
var postgres = builder.AddPostgres("postgres")
    .WithInitFiles("./postgres-init");
```

## Next Steps

These new features in .NET Aspire 9.4 provide powerful tools for building more sophisticated cloud-native applications. Consider how you might use:

- Interactive parameters for better developer onboarding
- Resource lifecycle events for complex initialization
- Enhanced Azure integrations for cloud deployments
- New AI integrations for intelligent applications

**Previous**: [Module #13 - Health Checks](13-healthchecks.md)
