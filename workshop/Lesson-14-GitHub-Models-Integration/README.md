# GitHub Models Integration with Aspire

## Introduction

GitHub Models provides free access to AI models directly through GitHub, making it easy to integrate AI capabilities into your applications without requiring separate API keys or cloud accounts. In this module, we'll integrate GitHub Models with our Weather Hub application to enhance the weather forecast experience with AI-powered background selection.

## 🤖 What are GitHub Models?

GitHub Models offers free access to popular AI models including:

- **GPT-4o and GPT-4o mini** for chat completions
- **Phi-3 models** for lightweight AI tasks
- **Other foundation models** for various AI scenarios

The integration uses your GitHub authentication, making it seamless for developers already using GitHub.

### 📚 Helpful Resources

Before we begin, here are some useful links to learn more about GitHub's AI offerings:

- **[GitHub Models Documentation](https://docs.github.com/en/github-models)** - Official documentation for GitHub Models
- **[GitHub Models Marketplace](https://github.com/marketplace/models)** - Browse available AI models
- **[GitHub Copilot](https://github.com/features/copilot)** - AI-powered code completion and chat
- **[Getting Started with GitHub Models](https://github.blog/2024-08-01-github-models-a-new-generation-of-ai-engineers/)** - Blog post introducing GitHub Models
- **[GitHub Models API Reference](https://docs.github.com/en/rest/models)** - API documentation for integrating with GitHub Models

## 🛠️ Setting Up GitHub Models Integration

### Prerequisites

1. A GitHub account
1. Access to GitHub Models (currently in preview)
1. Your GitHub personal access token with appropriate permissions

### Step 1: Add GitHub Models Package to AppHost

First, we need to add the GitHub Models integration package to the AppHost project using the Aspire CLI:

1. Navigate to your project directory

1. Use the Aspire CLI to add the GitHub Models hosting integration:

```bash
aspire add github-models
```

This command will automatically add the `Aspire.Hosting.GitHub.Models` package reference to your AppHost project and restore packages.

### Step 2: Add GitHub Models to AppHost

Now let's add the GitHub Models integration to our AppHost project:

1. Open your `AppHost/AppHost.cs` file
1. Add the GitHub Models integration:

```csharp
var builder = DistributedApplication.CreateBuilder(args);

...

// Add GitHub Models integration
var githubModel = builder.AddGitHubModel("chat-model", GitHubModel.OpenAI.OpenAIGPT4oMini);

...

var web = builder.AddProject<Projects.MyWeatherHub>("myweatherhub")
    .WithReference(api)
    .WithReference(weatherDb)
    .WithReference(githubModel) // Reference the GitHub model
    .WaitFor(postgres)
    .WithExternalHttpEndpoints();
```

### Step 3: Update Project Dependencies

Add the necessary NuGet packages to your MyWeatherHub project:

1. Open your `MyWeatherHub/MyWeatherHub.csproj` file
1. Add the following package references:

```xml
<PackageReference Include="Aspire.Azure.AI.Inference" Version="13.1.2-preview.1.26125.13" />
<PackageReference Include="Microsoft.Extensions.AI" Version="10.4.0" />
<PackageReference Include="Microsoft.Extensions.AI.OpenAI" Version="10.4.0" />
```

### Step 4: Configure AI Services in MyWeatherHub

Update your `MyWeatherHub/Program.cs` to configure the AI services:

```csharp
using MyWeatherHub;
using MyWeatherHub.Components;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();
builder.Services.AddHttpClient<NwsManager>(client =>
{
    client.BaseAddress = new("https+http://api");
});

// Add GitHub Models chat client
builder.AddAzureChatCompletionsClient("chat-model")
       .AddChatClient();

// Register the ForecastSummarizer service
builder.Services.AddScoped<ForecastSummarizer>();

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddMemoryCache();

builder.AddNpgsqlDbContext<MyWeatherContext>(connectionName: "weatherdb");

var app = builder.Build();

// Rest of your application configuration...
```

### Step 5: Create the ForecastSummarizer Service

Create a new file `MyWeatherHub/ForecastSummarizer.cs`:

```csharp
using Microsoft.Extensions.AI;

namespace MyWeatherHub;

public class ForecastSummarizer(IChatClient chatClient)
{
    public async Task<string> SummarizeForecastAsync(string forecasts)
    {
        var prompt = $"""
            You are a weather assistant. Summarize the following forecast 
            as one of the following conditions: Sunny, Cloudy, Rainy, Snowy.  
            Only those four values are allowed. Be as concise as possible.  
            I want a 1-word response with one of these options: Sunny, Cloudy, Rainy, Snowy.

            The forecast is: {forecasts}
            """;

        var response = await chatClient.GetResponseAsync(prompt);

        // Look for one of the four values in the response
        if (string.IsNullOrEmpty(response.Text))
        {
            return "Cloudy"; // Default fallback
        }

        var condition = response.Text switch
        {
            string s when s.Contains("Snowy", StringComparison.OrdinalIgnoreCase) => "Snowy",
            string s when s.Contains("Rainy", StringComparison.OrdinalIgnoreCase) => "Rainy", 
            string s when s.Contains("Cloudy", StringComparison.OrdinalIgnoreCase) => "Cloudy",
            string s when s.Contains("Sunny", StringComparison.OrdinalIgnoreCase) => "Sunny",
            string s when s.Contains("Clear", StringComparison.OrdinalIgnoreCase) => "Sunny",
            _ => "Cloudy" // Default fallback
        };

        return condition;
    }
}
```

### Step 6: Update the Home Component

Update your `Components/Pages/Home.razor` to use the AI-powered forecast summarization:

1. Add the ForecastSummarizer injection at the top:

```razor
@inject ForecastSummarizer Summarizer
```

1. Add a property to store the AI summary:

```csharp
@code {
    // ... existing properties ...
    
    string Summary { get; set; } = string.Empty;
    int randomBackground = new Random().Next(1, 4);
    
    // ... rest of existing code ...
}
```

1. Update the `SelectZone` method to use AI summarization:

```csharp
private async Task SelectZone(Zone zone)
{
    SelectedZone = zone;
    IsLoading = true;
    StateHasChanged();
    await Task.Delay(50);

    try
    {
        IsLoading = false;
        Forecast = await NwsManager.GetForecastByZoneAsync(zone.Key);
        Error = string.Empty;
    }
    catch (Exception ex)
    {
        IsLoading = false;
        Logger.LogError(ex, "Error getting forecast for {0}({1})", zone.Name, zone.Key);
        Forecast = null!;
        Error = $"Unable to locate weather for {SelectedZone.Name}({SelectedZone.Key})";
    }

    if (string.IsNullOrEmpty(Error))
    {
        Summary = await Summarizer.SummarizeForecastAsync(Forecast.FirstOrDefault().DetailedForecast);
    } 
}
```

1. Update the forecast display to use the AI summary for background selection:

```razor
@if (SelectedZone != null && Forecast != null)
{
    <div class="forecast-background-container" 
         style="background-image: url('img/@(Summary.ToLowerInvariant())/@(randomBackground).jpg');">
        <h3 class="weather-headline">
            Weather for @SelectedZone.Name<text>, </text> @SelectedZone.State (@SelectedZone.Key)
        </h3>
        <div class="row row-cols-1 row-cols-md-4 g-4">
            @foreach (var forecast in Forecast.Take(8))
            {
                <div class="col">
                    <div class="card forecast-card">
                        <div class="card-header">@forecast.Name</div>
                        <div class="card-body">@forecast.DetailedForecast</div>
                    </div>
                </div>
            }
        </div>
    </div>
}
```

## 🎨 Weather Background Images

We've already prepared weather-themed background images for you! The project includes an `img` folder in `MyWeatherHub/wwwroot/img/` with the following structure:

- `sunny/` - containing sunny weather background images
- `cloudy/` - containing cloudy weather background images  
- `rainy/` - containing rainy weather background images
- `snowy/` - containing snowy weather background images

Each folder contains multiple background images that the AI will randomly select from based on its weather analysis, creating a dynamic and visually appealing experience.

## 🧪 Testing the Integration

1. **Set up GitHub Models access**: Ensure your GitHub token has access to GitHub Models
1. **Run the application**: Use `dotnet run` or the Aspire dashboard
1. **Test AI integration**: Select different weather zones and observe:
   - The AI analyzing weather forecasts
   - Dynamic background selection based on AI analysis
   - The display showing which background the AI selected

## 🔧 Customization

### Customizing the AI Prompt

You can customize the AI behavior by modifying the prompt in `ForecastSummarizer.cs`:

```csharp
var prompt = $"""
    You are a weather expert analyzing forecasts for background image selection.
    Based on the forecast, determine the most appropriate background theme.
    
    Available options: Sunny, Cloudy, Rainy, Snowy
    Consider dominant weather patterns and time of day.
    
    Forecast: {forecasts}
    
    Respond with only one word from the available options.
    """;
```

## 🚀 Advanced Features

### Error Handling and Fallbacks

The implementation includes robust error handling:

- Default to "Cloudy" background if AI fails
- Graceful degradation when GitHub Models is unavailable
- Logging for debugging AI responses

### Performance Considerations

- AI calls are made only when selecting new zones
- Results could be cached for repeated zone selections
- Background image loading is optimized with CSS

## 🔍 Monitoring and Observability

The GitHub Models integration will appear in your Aspire dashboard:

- Monitor AI model usage and response times
- View connection status and health
- Debug configuration issues

## Next Steps

Now that you have GitHub Models integrated:

1. **Experiment with different models** - Try other available models for different use cases
1. **Add more AI features** - Consider adding weather recommendations or alerts
1. **Implement caching** - Cache AI responses to improve performance
1. **Add user preferences** - Let users choose between manual and AI background selection

## 🤖 Agentic Development with Aspire

In this module, you added AI as a *feature* of your app. But Aspire 13.1 also makes AI your *development partner*. While GitHub Models powers your weather backgrounds, the Aspire **agentic development** workflow lets AI assistants operate on your entire running system.

### From Copilot to Co-Developer

Traditional AI coding assistants suggest code. Aspire-connected agents go further:

| Capability | Traditional AI | Aspire-Connected Agent |
|-----------|---------------|----------------------|
| Code suggestions | ✅ | ✅ |
| Read source files | ✅ | ✅ |
| Query live resource states | ❌ | ✅ |
| Read real-time logs & traces | ❌ | ✅ |
| Run dashboard commands | ❌ | ✅ |
| Discover integrations & endpoints | ❌ | ✅ |

### How It Works

When you run `aspire mcp init` (covered in [Module #6](../Lesson-06-Telemetry/README.md)), your AI assistant connects to the Aspire MCP server. From there, it can reason about your *running* application — not just your source code.

### Real-World Agentic Scenarios

Here are practical examples using your Weather Hub application:

#### Scenario 1: Debugging a Failing Forecast

You ask your AI agent: *"The Philadelphia forecast is failing — why?"*

The agent:
1. Queries the Aspire dashboard for the `api` resource status
2. Reads recent structured logs filtered by "Philadelphia"
3. Finds the `NwsManagerDiagnostics.failedRequestCounter` spike
4. Checks the distributed trace and sees a timeout on the external weather API call
5. Reports back: *"The NWS API is timing out for zone PAZ071. The retry policy is firing but the upstream service is slow. Consider increasing the timeout or adding a circuit breaker."*

#### Scenario 2: Performance Investigation

You ask: *"How is my Redis cache performing?"*

The agent:
1. Checks the `cache_hits_total` and `cache_misses_total` metrics
2. Queries Redis Insight via the dashboard endpoint
3. Reports: *"Cache hit rate is 73%. The zones endpoint has a 100% hit rate after the first request, but forecast requests show frequent misses because the 15-minute TTL is expiring."*

#### Scenario 3: System-Wide Health Check

You ask: *"Is everything healthy?"*

The agent:
1. Lists all resources and their health status from the dashboard
2. Checks the `/health` endpoints for both `api` and `myweatherhub`
3. Verifies the PostgreSQL and Redis containers are running
4. Reports: *"All 4 resources are healthy. PostgreSQL has been up for 2 hours. Redis has 847 keys. The API has served 234 requests with a 4.2% error rate (intentional test errors)."*

### Setting Up Agentic Development

If you haven't already set up MCP in Module 6:

```bash
# Initialize MCP for your AI tools
aspire mcp init

# Start your application
aspire run
```

Then open your AI assistant and start asking questions about your running system!

### The Bigger Picture

The combination of:
- **GitHub Models** (this module) — AI as an application *feature*
- **Aspire MCP** ([Module #6](../Lesson-06-Telemetry/README.md)) — AI as a development *tool*
- **Custom Commands** ([Module #12](../Lesson-12-Custom-Commands/README.md)) — Scriptable dashboard operations the agent can invoke

...creates a powerful feedback loop where your AI assistant understands both your code *and* your running infrastructure.

### Learn More

- [Aspire Agentic Development Blog Post](https://devblogs.microsoft.com/aspire/aspire-13-1-holiday-gift/)
- [MCP for AI Coding Agents Deep Dive](https://dev.to/chris_ayers/aspire-cli-part-3-mcp-for-ai-coding-agents-5d8j)
- [Aspire Roadmap — Agent Features](https://github.com/dotnet/aspire/discussions/13608)

## Congratulations! 🎉

You've successfully integrated GitHub Models with your Aspire application! You now have AI-powered weather background selection that enhances the user experience with intelligent, dynamic visuals.

Throughout this workshop, you've learned how to build, configure, and enhance cloud-native applications using Aspire. You now have the skills to create resilient, observable, and scalable distributed applications with AI capabilities.

**Previous**: [Module #13 - Healthchecks](../Lesson-13-HealthChecks/README.md) | **Next**: [Module #15 - Docker Integration](../Lesson-15-Docker-Integration/README.md)
