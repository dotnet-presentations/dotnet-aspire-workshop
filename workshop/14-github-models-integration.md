# GitHub Models Integration with .NET Aspire

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
2. Access to GitHub Models (currently in preview)
3. Your GitHub personal access token with appropriate permissions

### Step 1: Add GitHub Models Package to AppHost

First, we need to add the GitHub Models integration package to the AppHost project using the Aspire CLI:

1. Navigate to your project directory

2. Use the Aspire CLI to add the GitHub Models hosting integration:

```bash
aspire add github-models
```

This command will automatically add the `Aspire.Hosting.GitHub.Models` package reference to your AppHost project and restore packages.

### Step 2: Add GitHub Models to AppHost

Now let's add the GitHub Models integration to our AppHost project:

1. Open your `AppHost/Program.cs` file
2. Add the GitHub Models integration:

```csharp
var builder = DistributedApplication.CreateBuilder(args);

...

// Add GitHub Models integration
var githubModel = builder.AddGitHubModel("chat-model", "gpt-4o-mini");

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
2. Add the following package references:

```xml
<PackageReference Include="Microsoft.Extensions.AI" Version="9.0.1-preview.1.24570.5" />
<PackageReference Include="Microsoft.Extensions.AI.OpenAI" Version="9.0.1-preview.1.24570.5" />
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
builder.AddChatClient("chat-model");

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

        var response = await chatClient.CompleteAsync(prompt);

        // Look for one of the four values in the response
        if (string.IsNullOrEmpty(response.Message.Text))
        {
            return "Cloudy"; // Default fallback
        }

        var condition = response.Message.Text switch
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

2. Add a property to store the AI summary:

```csharp
@code {
    // ... existing properties ...
    
    string AiSummary { get; set; } = "Cloudy"; // Default background
    int randomBackground = new Random().Next(1, 4);
    
    // ... rest of existing code ...
}
```

3. Update the `SelectZone` method to use AI summarization:

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
        
        // Use AI to summarize the forecast for background selection
        if (Forecast?.Any() == true)
        {
            AiSummary = await Summarizer.SummarizeForecastAsync(
                Forecast.FirstOrDefault()?.DetailedForecast ?? "");
        }
    }
    catch (Exception ex)
    {
        IsLoading = false;
        Logger.LogError(ex, "Error getting forecast for {0}({1})", zone.Name, zone.Key);
        Forecast = null!;
        Error = $"Unable to locate weather for {SelectedZone.Name}({SelectedZone.Key})";
        AiSummary = "Cloudy"; // Default on error
    }
}
```

4. Update the forecast display to use the AI summary for background selection:

```razor
@if (SelectedZone != null && Forecast != null)
{
    <div class="forecast-background-container" 
         style="background-image: url('img/@(AiSummary.ToLowerInvariant())/@(randomBackground).jpg');">
        <h3 class="weather-headline">
            Weather for @SelectedZone.Name, @SelectedZone.State (@SelectedZone.Key)
            <small class="text-muted">- AI suggests: @AiSummary background</small>
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
2. **Run the application**: Use `dotnet run` or the Aspire dashboard
3. **Test AI integration**: Select different weather zones and observe:
   - The AI analyzing weather forecasts
   - Dynamic background selection based on AI analysis
   - The display showing which background the AI selected

## 🔧 Configuration and Customization

### Environment Variables

Configure GitHub Models through environment variables or user secrets:

```json
{
  "ConnectionStrings": {
    "chat-model": "Endpoint=https://models.inference.ai.azure.com;Key=your-github-token"
  }
}
```

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
2. **Add more AI features** - Consider adding weather recommendations or alerts
3. **Implement caching** - Cache AI responses to improve performance
4. **Add user preferences** - Let users choose between manual and AI background selection

## Congratulations! 🎉

You've completed the .NET Aspire Workshop! Throughout these modules, you've learned how to build, configure, and manage cloud-native applications using .NET Aspire. You now have the skills to create resilient, observable, and scalable distributed applications.

**Previous**: [Module #13 - Healthchecks](13-healthchecks.md)

