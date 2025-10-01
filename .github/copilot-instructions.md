# .NET Aspire Workshop

A comprehensive workshop for building cloud-native applications with .NET Aspire, featuring a weather application with Blazor frontend, ASP.NET Core API, Redis caching, PostgreSQL database, and AI integration.

Always reference these instructions first and fallback to search or bash commands only when you encounter unexpected information that does not match the info here.

## Working Effectively

### Prerequisites and Setup

- Install .NET 9 SDK: `curl -sSL https://dot.net/v1/dotnet-install.sh | bash /dev/stdin --version latest --channel 9.0`
- Set PATH: `export PATH="/home/runner/.dotnet:$PATH"`
- Install Docker Desktop or Podman for container resources
- Install .NET Aspire CLI: `dotnet tool install -g Aspire.Cli`
- Install .NET Aspire templates: `dotnet new install Aspire.ProjectTemplates::9.5.0 --force`

### Build Commands

- Build start solution: `cd start && dotnet build MyWeatherHub.sln` -- takes 10 seconds. NEVER CANCEL. Set timeout to 60+ seconds.
- Build complete solution: `cd complete && dotnet build MyWeatherHub.sln` -- takes 30 seconds. NEVER CANCEL. Set timeout to 120+ seconds.

### Run Applications

- **Start solution (simple)**: Run individual projects without orchestration
  - API: `cd start && dotnet run --project Api` -- listens on <http://localhost:5271>
  - Web app: `cd start && dotnet run --project MyWeatherHub` -- listens on <http://localhost:5103>
- **Complete solution (with Aspire orchestration)**: `cd complete && dotnet run --project AppHost` or `cd complete && aspire run`
  - Requires Docker running for Redis, PostgreSQL containers
  - Opens Aspire Dashboard (typically <https://localhost:17137>)
  - NEVER CANCEL: Startup takes 60+ seconds with container initialization. Set timeout to 300+ seconds.

### Test Commands

- Integration tests: `cd complete && dotnet test IntegrationTests` -- takes 45 seconds. NEVER CANCEL. Set timeout to 180+ seconds.
- **Note**: Integration tests require Docker and full Aspire environment. They may fail in CI environments without proper setup.

## Validation Scenarios

Always manually validate changes with these complete scenarios:

### Basic Weather App Workflow

1. Start the API: `cd start && dotnet run --project Api`
2. Test API endpoint: `curl http://localhost:5271/zones` -- should return JSON array of weather zones
3. Start the web app: `cd start && dotnet run --project MyWeatherHub`
4. Navigate to <http://localhost:5103> in browser
5. Click on different cities to view weather forecasts
6. Verify API calls are working by checking logs

### Complete Aspire Workflow (requires Docker)

1. Ensure Docker is running: `docker ps`
2. Start AppHost: `cd complete && dotnet run --project AppHost`
3. Open Aspire Dashboard (URL shown in console output)
4. Verify all resources are running (web, api, cache, postgres)
5. Test web application through dashboard endpoints
6. Check logs, traces, and metrics in dashboard

## Repository Structure

### Key Projects

- **start/** - Starting workshop solution
  - `MyWeatherHub/` - Blazor Server web application
  - `Api/` - ASP.NET Core Web API for weather data
- **complete/** - Completed workshop solution with Aspire orchestration
  - `AppHost/` - .NET Aspire orchestration project (main entry point)
  - `ServiceDefaults/` - Shared service configuration
  - `IntegrationTests/` - Aspire integration tests
  - `MyWeatherHub/` - Enhanced Blazor app with database integration
  - `Api/` - Enhanced API with caching and external services

### Workshop Modules

The `workshop/` folder contains 15 modules covering:

1. Setup & Installation
2. Service Defaults  
3. Dashboard & Orchestration
4. Service Discovery
5. Integrations (Redis, PostgreSQL)
6. Telemetry
7. Database Integration
8. Integration Testing
9. Deployment
10. Container Management
11. Azure Integrations
12. Custom Commands
13. Health Checks
14. GitHub Models Integration
15. Docker Integration

## Common Development Tasks

### Adding New Features

- Always start with the `start/` solution for learning
- Build and test frequently: `dotnet build && dotnet test`
- Use `aspire add` command to add new integrations to complete solution
- Test with both individual project runs and AppHost orchestration

### Debugging Issues

- Check individual project logs first
- Use Aspire Dashboard for distributed tracing when using complete solution
- API endpoints: `/zones` (get weather zones), `/forecast/{zoneId}` (get forecast)
- Verify Docker containers are running for complete solution

### Before Committing

- Build both solutions: `dotnet build start/MyWeatherHub.sln && dotnet build complete/MyWeatherHub.sln`
- Test basic functionality with start solution
- If modifying complete solution, verify AppHost can start successfully

## Technology Stack

- **.NET 9** with C# for all projects
- **Blazor Server** for web frontend
- **ASP.NET Core** for API
- **National Weather Service API** for weather data
- **.NET Aspire 9.5** for orchestration and observability
- **Redis** for caching (complete solution)
- **PostgreSQL** for database (complete solution)
- **GitHub Models** for AI integration (complete solution)
- **Docker** for containerized resources

## Critical Timing Notes

- **NEVER CANCEL**: Build operations may take 30+ seconds, especially on first run
- **NEVER CANCEL**: AppHost startup takes 60+ seconds with container pulls
- **NEVER CANCEL**: Integration tests take 45+ seconds and may timeout in CI
- **Always wait**: Docker container initialization can take 2+ minutes on first run
- Set timeouts to minimum 300 seconds for any Aspire operations involving containers

## Troubleshooting

- **AppHost fails to start**: Ensure Docker is running and no port conflicts
- **Integration tests fail**: Normal in CI environments without full Docker setup
- **API returns empty responses**: Check that zones.json exists in Api/wwwroot
- **Build errors on .NET 8**: Install .NET 9 SDK as shown in prerequisites
- **Containers not starting**: Check Docker Desktop is running and has sufficient resources

The workshop teaches cloud-native development patterns using .NET Aspire's orchestration, service discovery, telemetry, and integration capabilities in a practical weather application scenario.
