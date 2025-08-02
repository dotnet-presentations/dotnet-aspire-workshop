# Database Integration

## Introduction

In this module, we will integrate a PostgreSQL database with our application. We will use Entity Framework Core (EF Core) to interact with the database. Additionally, we will set up PgAdmin to manage our PostgreSQL database.

## Setting Up PostgreSQL

.NET Aspire provides built-in support for PostgreSQL through the `Aspire.Hosting.PostgreSQL` package. To set up PostgreSQL:

1. Install the required NuGet package in your AppHost project:

```xml
<PackageReference Include="Aspire.Hosting.PostgreSQL" Version="9.4.0" />
```

1. Update the AppHost's Program.cs to add PostgreSQL:

```csharp
var postgres = builder.AddPostgres("postgres")
    .WithDataVolume(isReadOnly: false);

var weatherDb = postgres.AddDatabase("weatherdb");
```

The `WithDataVolume(isReadOnly: false)` configuration ensures that your data persists between container restarts. The data is stored in a Docker volume that exists outside the container, making it survive container restarts.

### New in .NET Aspire 9.4: Enhanced Database Initialization

.NET Aspire 9.4 introduces the improved `WithInitFiles()` method for all database providers, replacing the more complex `WithInitBindMount()` method:

```csharp
var postgres = builder.AddPostgres("postgres")
    .WithDataVolume(isReadOnly: false)
    .WithInitFiles("./database-init");  // Simplified initialization from files
```

This method works consistently across all database providers (PostgreSQL, MySQL, MongoDB, Oracle) and provides better error handling and simplified configuration.

To ensure proper application startup, we'll configure the web application to wait for the database:

```csharp
var web = builder.AddProject<Projects.MyWeatherHub>("myweatherhub")
    .WithReference(weatherDb)
    .WaitFor(postgres)  // Ensures database is ready before app starts
    .WithExternalHttpEndpoints();
```

### Adding Container Persistence to Your Database

For development scenarios where you want your database container and data to persist across multiple application runs, you can configure the container lifetime:

```csharp
var postgres = builder.AddPostgres("postgres")
    .WithDataVolume(isReadOnly: false)
    .WithLifetime(ContainerLifetime.Persistent);  // Container persists across app restarts

var weatherDb = postgres.AddDatabase("weatherdb");
```

With `ContainerLifetime.Persistent`, the PostgreSQL container will continue running even when you stop your Aspire application. This means:

- **Faster startup times**: No need to wait for PostgreSQL to initialize on subsequent runs
- **Data persistence**: Your database data remains intact between application sessions
- **Consistent development**: The database stays in the same state you left it

> **Note**: Persistent containers are primarily useful for development scenarios. In production deployments, you'll typically use managed database services that handle persistence automatically.

> **Advanced Container Features**: .NET Aspire also supports advanced container configuration like `WithExplicitStart()` for better startup coordination, and `WithContainerFiles()` for mounting initialization scripts. These features provide fine-grained control over container behavior when needed for complex development scenarios. To learn more about these advanced features, see [Persist data using volumes](https://learn.microsoft.com/dotnet/aspire/fundamentals/persist-data-volumes) and [Container resource lifecycle](https://learn.microsoft.com/dotnet/aspire/fundamentals/app-host-overview#container-resource-lifecycle) in the official documentation.

## Integrating EF Core with PostgreSQL

1. Install the required NuGet packages in your web application:

```xml
<PackageReference Include="Aspire.Npgsql.EntityFrameworkCore.PostgreSQL" Version="9.4.0" />
```

1. Create your DbContext class:

```csharp
public class MyWeatherContext : DbContext
{
    public MyWeatherContext(DbContextOptions<MyWeatherContext> options)
        : base(options)
    {
    }

    public DbSet<Zone> FavoriteZones => Set<Zone>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Zone>()
            .HasKey(z => z.Key);
    }
}
```

1. Register the DbContext in your application's Program.cs:

```csharp
builder.AddNpgsqlDbContext<MyWeatherContext>(connectionName: "weatherdb");
```

Note that .NET Aspire handles the connection string configuration automatically. The connection name "weatherdb" matches the database name we created in the AppHost project.

1. Set up database initialization:

```csharp
if (app.Environment.IsDevelopment())
{
    using (var scope = app.Services.CreateScope())
    {
        var context = scope.ServiceProvider.GetRequiredService<MyWeatherContext>();
        await context.Database.EnsureCreatedAsync();
    }
}
```

For development environments, we use `EnsureCreatedAsync()` to automatically create the database schema. In a production environment, you should use proper database migrations instead.

## Updating the Web App

Now we'll update the web application to support favoriting weather zones and filtering them. Let's make these changes step by step:

1. Make sure to add these Entity Framework using statements at the top of `Home.razor` if they're not already present:

```csharp
@using Microsoft.EntityFrameworkCore
@inject MyWeatherContext DbContext
```

1. Add these new properties to the `@code` block to support the favorites functionality:

```csharp
bool ShowOnlyFavorites { get; set; }
List<Zone> FavoriteZones { get; set; } = new List<Zone>();
```

1. Update the `OnInitializedAsync` method to load favorites from the database. Find the existing method and replace it with:

```csharp
protected override async Task OnInitializedAsync()
{
    AllZones = (await NwsManager.GetZonesAsync()).ToArray();
    FavoriteZones = await MyWeatherContext.FavoriteZones.ToListAsync();
}
```

1. Finally, add the `ToggleFavorite` method to handle saving favorites to the database. Add this method to the `@code` block:

```csharp
private async Task ToggleFavorite(Zone zone)
{
    if (FavoriteZones.Contains(zone))
    {
        FavoriteZones.Remove(zone);
        MyWeatherContext.FavoriteZones.Remove(zone);
    }
    else
    {
        FavoriteZones.Add(zone);
        MyWeatherContext.FavoriteZones.Add(zone);
    }
    await DbContext.SaveChangesAsync();
}
```

1. In the `@code` block of `Home.razor`, locate the `zones` property and replace it with this updated version that includes the favorites filter:

```csharp
IQueryable<Zone> zones
{
    get
    {
        var results = AllZones.AsQueryable();

        if (ShowOnlyFavorites)
        {
            results = results.Where(z => FavoriteZones.Contains(z));
        }

        results = string.IsNullOrEmpty(StateFilter) ? results
                : results.Where(z => z.State == StateFilter.ToUpper());

        results = string.IsNullOrEmpty(NameFilter) ? results
                : results.Where(z => z.Name.Contains(NameFilter, StringComparison.InvariantCultureIgnoreCase));

        return results.OrderBy(z => z.Name);
    }
}
```

1. First, add a checkbox to filter the zones list. In `Home.razor`, add this code just before the `<QuickGrid>` element:

```csharp
<div class="form-check mb-3">
    <input class="form-check-input" type="checkbox" @bind="ShowOnlyFavorites" id="showFavorites">
    <label class="form-check-label" for="showFavorites">
        Show only favorites
    </label>
</div>
```

1. Next, add a new column to show the favorite status. Add this column definition inside the `<QuickGrid>` element, after the existing State column:

```csharp
<TemplateColumn Title="Favorite">
    <ChildContent>
        <button @onclick="@(() => ToggleFavorite(context))">
            @if (FavoriteZones.Contains(context))
            {
                <span>&#9733;</span> <!-- Starred -->
            }
            else
            {
                <span>&#9734;</span> <!-- Unstarred -->
            }
        </button>
    </ChildContent>
</TemplateColumn>
```

## Testing Your Changes

Now let's verify that your changes are working correctly by testing the favorites functionality and database persistence:

1. Start the application:
   - In Visual Studio: Right-click the AppHost project and select "Set as Startup Project", then press F5
   - In VS Code: Open the Run and Debug panel (Ctrl+Shift+D), select "Run AppHost" from the dropdown, and click Run

1. Open your browser to the My Weather Hub application:
   - Navigate to <https://localhost:7274>
   - Verify you see the new "Show only favorites" checkbox above the grid
   - Check that each row in the grid now has a star icon (☆) in the Favorite column

1. Test the favorites functionality:
   - Use the Name filter to find "Philadelphia"
   - Click the empty star (☆) next to Philadelphia - it should fill in (★)
   - Find and favorite a few more cities (try "Manhattan" and "Los Angeles County")
   - Check the "Show only favorites" checkbox
   - Verify that the grid now only shows your favorited cities
   - Uncheck "Show only favorites" to see all cities again
   - Try unfavoriting a city by clicking its filled star (★)

1. Verify the persistence:
   - Close your browser window
   - Stop the application in your IDE (click the stop button or press Shift+F5)
   - Restart the AppHost project
   - Navigate back to <https://localhost:7274>
   - Verify that:
     - Your favorited cities still show filled stars (★)
     - Checking "Show only favorites" still filters to just your saved cities
     - The star toggles still work for adding/removing favorites

If you want to reset and start fresh:

1. Stop the application completely
1. Open Docker Desktop
1. Navigate to the Volumes section
1. Find and delete the PostgreSQL volume
1. Restart the application - it will create a fresh database automatically

## Other Data Options

In addition to PostgreSQL, .NET Aspire provides first-class support for several other database systems:

### [Azure SQL/SQL Server](https://learn.microsoft.com/en-us/dotnet/aspire/database/sql-server-entity-framework-integration)

SQL Server integration in .NET Aspire includes automatic container provisioning for development, connection string management, and health checks. It supports both local SQL Server containers and Azure SQL Database in production. The integration handles connection resiliency automatically and includes telemetry for monitoring database operations.

### [MySQL](https://learn.microsoft.com/en-us/dotnet/aspire/database/mysql-entity-framework-integration)

The MySQL integration for .NET Aspire provides similar capabilities to PostgreSQL, including containerized development environments and production-ready configurations. It includes built-in connection retries and health monitoring, making it suitable for both development and production scenarios.

### [MongoDB](https://learn.microsoft.com/en-us/dotnet/aspire/database/mongodb-integration)

For NoSQL scenarios, Aspire's MongoDB integration offers connection management, health checks, and telemetry. It supports both standalone MongoDB instances and replica sets, with automatic container provisioning for local development. The integration handles connection string management and includes retry policies specifically tuned for MongoDB operations.

### SQLite

While SQLite doesn't require containerization, Aspire provides consistent configuration patterns and health checks. It's particularly useful for development and testing scenarios, offering the same familiar development experience as other database providers while being completely self-contained.

## Community Toolkit Database Features

The .NET Aspire Community Toolkit extends database capabilities with additional tooling:

### [SQL Database Projects](https://learn.microsoft.com/en-us/dotnet/aspire/community-toolkit/hosting-sql-database-projects)

The SQL Database Projects integration enables you to include your database schema as part of your source code. It automatically builds and deploys your database schema during development, ensuring your database structure is version controlled and consistently deployed. This is particularly useful for teams that want to maintain their database schema alongside their application code.

### [Data API Builder](https://learn.microsoft.com/en-us/dotnet/aspire/community-toolkit/hosting-data-api-builder)

Data API Builder (DAB) automatically generates REST and GraphQL endpoints from your database schema. This integration allows you to quickly expose your data through modern APIs without writing additional code. It includes features like:

- Automatic REST and GraphQL endpoint generation
- Built-in authentication and authorization
- Custom policy support
- Real-time updates via GraphQL subscriptions
- Database schema-driven API design

## Conclusion

In this module, we added PostgreSQL database support to our application using .NET Aspire's database integration features. We used Entity Framework Core for data access and configured our application to work with both local development and cloud-hosted databases.

The natural next step would be to add tests to verify the database integration works correctly. 

Head over to [Module #8: Integration Testing](./8-integration-testing.md) to learn how to write integration tests for your .NET Aspire application.


**Next**: [Module #8: Integration Testing](./8-integration-testing.md)