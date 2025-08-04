# Docker Integration with .NET Aspire

## Introduction

.NET Aspire provides excellent support for integrating Docker containers directly into your application hosting model. This allows you to include third-party services, tools, and applications as part of your distributed application architecture. In this module, we'll integrate IT-Tools, a collection of handy online developer tools, into our Weather Hub application using Docker containers.

## 🐳 What is Docker Integration in Aspire?

Docker integration in .NET Aspire allows you to:

- **Add containerized services** to your application orchestration
- **Manage container lifecycle** through the Aspire dashboard
- **Configure networking and dependencies** between containers and your .NET services
- **Monitor container health** and logs alongside your application telemetry

This makes it easy to include databases, caches, message brokers, and utility services in your local development environment.

### 📚 Helpful Resources

Before we begin, here are some useful links to learn more about Docker and containerization:

- **[Docker Documentation](https://docs.docker.com/)** - Official Docker documentation
- **[Docker Hub](https://hub.docker.com/)** - Container image registry
- **[IT-Tools GitHub Repository](https://github.com/CorentinTh/it-tools)** - Source code for IT-Tools
- **[IT-Tools Live Demo](https://it-tools.tech/)** - Try IT-Tools online
- **[.NET Aspire Container Documentation](https://learn.microsoft.com/en-us/dotnet/aspire/fundamentals/persist-data-volumes)** - Aspire's container integration guide

## 🛠️ What is IT-Tools?

IT-Tools is a collection of handy online tools for developers, packaged as a lightweight web application. It includes:

- **Text & Code Tools**: JSON formatter, Base64 encoder/decoder, URL encoder/decoder
- **Crypto Tools**: Hash generators (MD5, SHA1, SHA256), UUID generator
- **Network Tools**: QR code generator, color picker, lorem ipsum generator
- **Development Utilities**: Regex tester, timestamp converter, and more

IT-Tools is perfect for our example because:

- It's lightweight (~23.5 MB)
- Runs on port 80 inside the container
- Provides immediate visual feedback
- Useful for actual development work

## 🛠️ Adding Docker Containers to AppHost

### Step 1: Add IT-Tools Container to AppHost

Let's add IT-Tools as a Docker container to our AppHost project:

1. Open your `AppHost/Program.cs` file
2. Add the IT-Tools container integration:

```csharp
var builder = DistributedApplication.CreateBuilder(args);

...

// Add IT-Tools Docker container
var itTools = builder.AddContainer("it-tools", "corentinth/it-tools")
    .WithHttpEndpoint(targetPort: 80)
    .WithExternalHttpEndpoints();

// Add GitHub Models integration (from previous module)
var githubModel = builder.AddGitHubModel("chat-model", "gpt-4o-mini");

...

var web = builder.AddProject<Projects.MyWeatherHub>("myweatherhub")
    .WithReference(api)
    .WithReference(weatherDb)
    .WithReference(githubModel)
    .WithReference(itTools) // Reference IT-Tools container
    .WaitFor(postgres)
    .WithExternalHttpEndpoints();
```

### Step 2: Understanding Container Configuration

Let's break down the container configuration:

- **`AddContainer("it-tools", "corentinth/it-tools")`**: Creates a container named "it-tools" using the specified Docker image
- **`WithHttpEndpoint(targetPort: 80)`**: Exposes port 80 from the container as an HTTP endpoint
- **`WithExternalHttpEndpoints()`**: Makes the container accessible from outside the Aspire application
- **`WithReference(itTools)`**: Allows other services to discover and connect to IT-Tools

### Step 3: Add a Link to IT-Tools in Your Web Application

Update your `Components/Pages/Home.razor` to include a link to IT-Tools:

1. Add a developer tools section to your home page:

```razor
@page "/"
@rendermode InteractiveServer
@inject NwsManager NwsManager
@inject ILogger<Home> Logger
@inject ForecastSummarizer Summarizer

<PageTitle>My Weather Hub</PageTitle>

<div class="hero-section">
    <h1 class="display-4">🌤️ My Weather Hub</h1>
    <p class="lead">Get weather forecasts with AI-powered backgrounds and developer tools</p>
    
    <!-- Developer Tools Section -->
    <div class="developer-tools-section mb-4">
        <h5>🛠️ Developer Tools</h5>
        <a href="http://localhost:8090" target="_blank" class="btn btn-outline-primary">
            <i class="bi bi-tools"></i> Open IT-Tools
        </a>
        <small class="text-muted d-block mt-1">Collection of handy online tools for developers</small>
    </div>
</div>

<!-- Rest of your existing content -->
<div class="zone-selection">
    <h3>Select a Weather Zone</h3>
    <div class="row row-cols-2 row-cols-md-4 row-cols-lg-6 g-3">
        @foreach (var zone in NwsManager.Zones)
        {
            <div class="col">
                <button class="btn btn-outline-secondary zone-btn w-100" 
                        @onclick="() => SelectZone(zone)"
                        disabled="@IsLoading">
                    @zone.Name, @zone.State
                </button>
            </div>
        }
    </div>
</div>

@* Rest of your existing forecast display code *@
```

## 🧪 Testing the Docker Integration

1. **Run the application**: Start your Aspire application using the dashboard or command line
2. **Verify container startup**: Check the Aspire dashboard to see IT-Tools starting up
3. **Access IT-Tools**: Click the "Open IT-Tools" link or check the Aspire dashboard for the assigned port
4. **Test the tools**: Try some of the developer tools like JSON formatter or Base64 encoder

### Expected Behavior

- IT-Tools should appear in your Aspire dashboard as a running container
- The container should show as healthy with logs indicating successful startup
- You should be able to access IT-Tools through the dynamically assigned port shown in the dashboard
- The tools should be fully functional for development tasks

## 🔧 Advanced Container Configuration

### Adding Environment Variables

You can configure containers with environment variables:

```csharp
var itTools = builder.AddContainer("it-tools", "corentinth/it-tools")
    .WithHttpEndpoint(targetPort: 80)
    .WithEnvironment("NODE_ENV", "production")
    .WithExternalHttpEndpoints();
```

### Adding Volume Mounts

For containers that need persistent storage:

```csharp
var itTools = builder.AddContainer("it-tools", "corentinth/it-tools")
    .WithHttpEndpoint(targetPort: 80)
    .WithVolume("it-tools-data", "/app/data")
    .WithExternalHttpEndpoints();
```

### Container Dependencies

Make containers wait for other services to be ready:

```csharp
var itTools = builder.AddContainer("it-tools", "corentinth/it-tools")
    .WithHttpEndpoint(targetPort: 80)
    .WaitFor(postgres) // Wait for database to be ready
    .WithExternalHttpEndpoints();
```

## 🔍 Monitoring Docker Containers

The Aspire dashboard provides excellent visibility into your Docker containers:

### Container Metrics

- **CPU and Memory usage** for each container
- **Network traffic** and port mappings
- **Container health status** and restart counts

### Container Logs

- **Real-time log streaming** from Docker containers
- **Log filtering and searching** capabilities
- **Integration with structured logging** from your .NET services

### Service Discovery

- Containers are **automatically discoverable** by other services
- **Environment variables** are injected for service URLs
- **Load balancing** and health checks are handled automatically

## 🚀 Best Practices for Docker Integration

### 1. Use Specific Image Tags

Instead of using `latest`, specify version tags for reproducible builds:

```csharp
var itTools = builder.AddContainer("it-tools", "corentinth/it-tools:nightly")
    .WithHttpEndpoint(targetPort: 80)
    .WithExternalHttpEndpoints();
```

### 2. Configure Resource Limits

Set memory and CPU limits for containers:

```csharp
var itTools = builder.AddContainer("it-tools", "corentinth/it-tools")
    .WithHttpEndpoint(targetPort: 80)
    .WithEnvironment("NODE_OPTIONS", "--max-old-space-size=512")
    .WithExternalHttpEndpoints();
```

### 3. Use Health Checks

Configure health checks for better monitoring:

```csharp
var itTools = builder.AddContainer("it-tools", "corentinth/it-tools")
    .WithHttpEndpoint(targetPort: 80)
    .WithHttpHealthCheck("/", port: 80)
    .WithExternalHttpEndpoints();
```

### 4. Group Related Services

Use consistent naming and organize related containers:

```csharp
// Development tools group
var itTools = builder.AddContainer("dev-it-tools", "corentinth/it-tools");

// Infrastructure group  
var nginx = builder.AddContainer("infra-nginx", "nginx:alpine");
```

## 🔍 Troubleshooting Docker Integration

### Common Issues

1. **Port Conflicts**: Ensure host ports don't conflict with other services
2. **Container Startup Time**: Use `WaitFor()` to handle dependencies properly
3. **Network Connectivity**: Verify containers can communicate with each other
4. **Resource Constraints**: Monitor CPU and memory usage in the dashboard

### Debugging Steps

1. **Check Aspire Dashboard**: Look for container status and logs
2. **Verify Port Mappings**: Ensure ports are correctly configured
3. **Test Container Health**: Use health check endpoints if available
4. **Review Logs**: Check both container logs and Aspire orchestration logs

## Next Steps

Now that you have Docker integration working:

1. **Explore more containers** - Add Redis, MongoDB, or other services your application needs
2. **Configure networking** - Set up container-to-container communication
3. **Add persistent storage** - Use volumes for data that needs to survive container restarts
4. **Implement health checks** - Add custom health check endpoints for better monitoring
5. **Production considerations** - Learn about container orchestration for deployment

## Congratulations! 🎉

You've successfully integrated Docker containers into your .NET Aspire application! You now understand how to:

- Add third-party services using Docker containers
- Configure port mappings and networking
- Monitor container health and logs through the Aspire dashboard
- Integrate containers with your .NET services

This powerful capability allows you to build comprehensive development environments that include all the tools and services your team needs.

**Previous**: [Module #14 - GitHub Models Integration](14-github-models-integration.md)
