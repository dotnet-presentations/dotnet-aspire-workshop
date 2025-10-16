# Database Integration

## Introduction

In this module, we will integrate a PostgreSQL database with our application. We will use Entity Framework Core (EF Core) to interact with the database. Additionally, we will set up PgAdmin to manage our PostgreSQL database.

## Setting Up PostgreSQL

.NET Aspire provides built-in support for PostgreSQL through the `Aspire.Hosting.PostgreSQL` package. To set up PostgreSQL:

1. Install the required NuGet package in your AppHost project:

```xml
<PackageReference Include="Aspire.Hosting.PostgreSQL" Version="9.4.2" />
```

1. Update the AppHost's Program.cs to add PostgreSQL:

```csharp
var postgres = builder.AddPostgres("postgres")
    .WithDataVolume(isReadOnly: false);

var weatherDb = postgres.AddDatabase("weatherdb");
```

The `WithDataVolume(isReadOnly: false)` configuration ensures that your data persists between container restarts. The data is stored in a Docker volume that exists outside the container, making it survive container restarts. This is optional for the workshop—if you omit it, the sample still runs; you just won't keep data between runs.

### Enhanced Database Initialization

Aspire provides the method `WithInitFiles()` for all database providers, replacing the more complex `WithInitBindMount()` method:

```csharp
var postgres = builder.AddPostgres("postgres")
    .WithDataVolume(isReadOnly: false)
    .WithInitFiles("./database-init");  // Simplified initialization from files
```

This method works consistently across all database providers (PostgreSQL, MySQL, MongoDB, Oracle) and provides better error handling and simplified configuration. Using `WithInitFiles` is optional for this workshop; the database integration works without it.

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

With `ContainerLifetime.Persistent`, the PostgreSQL container will continue running even when you stop your Aspire application. This is optional and not required to complete the module. If enabled, it means:

- **Faster startup times**: No need to wait for PostgreSQL to initialize on subsequent runs
- **Data persistence**: Your database data remains intact between application sessions
- **Consistent development**: The database stays in the same state you left it

> [!NOTE]
> Persistent containers are mainly for local development convenience. In production you'll typically rely on a managed database service (Azure Database for PostgreSQL, Azure Cosmos DB for PostgreSQL, etc.) or external infrastructure that already guarantees durability.
>
> Need finer control? Aspire also supports:
>
> - `WithExplicitStart()` — manually coordinate start order
> - `WithContainerFiles()` — inject init scripts and assets
> - `WithInitFiles()` — simplified cross-database initialization
>
>
> Learn more: [Persist data using volumes](https://learn.microsoft.com/dotnet/aspire/fundamentals/persist-data-volumes) · [Container resource lifecycle](https://learn.microsoft.com/dotnet/aspire/fundamentals/app-host-overview#container-resource-lifecycle)

## Integrating EF Core with PostgreSQL

1. Install the required NuGet packages in your web application:

```xml
<PackageReference Include="Aspire.Npgsql.EntityFrameworkCore.PostgreSQL" Version="9.4.2" />
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

We've provided a pre-built `WeatherZoneGrid` component that encapsulates all the database interaction functionality. This approach demonstrates best practices for component design in Blazor applications and keeps your `Home.razor` file clean and maintainable.

> **🎯 Workshop Approach**: Instead of manually adding extensive grid and database code to `Home.razor`, you'll use the provided `WeatherZoneGrid` component. This demonstrates how to create reusable, maintainable Blazor components with database integration.

The `WeatherZoneGrid` component is already available in your project and includes:
- Sortable, filterable zone grid with pagination
- "Show only favorites" checkbox functionality  
- Star icons for favoriting/unfavoriting zones
- Database persistence of favorite zones using Entity Framework
- Search functionality by zone name and state
- Isolated CSS styling

> QuickGrid Version Note: The project already references the latest stable 9.0.x version of `Microsoft.AspNetCore.Components.QuickGrid` with all necessary configurations.



### Using the WeatherZoneGrid Component

To integrate database functionality into your application, you just need to make a few simple changes to `Home.razor`:

1. **Add the Entity Framework using statement** at the top with the other `@using` statements:
   ```csharp
   @using Microsoft.EntityFrameworkCore
   ```

2. **Replace the entire existing grid section** in `Home.razor`. Find this section:
   ```razor
   <div class="col-md-6">
       <QuickGrid Items="zones" TGridItem="Zone" Pagination="pagination">
           <!-- ... existing grid content ... -->
       </QuickGrid>
       <Paginator State="@pagination"></Paginator>
   </div>
   ```
   
   And replace it with:
   ```razor
   <div class="col-md-6">
       <WeatherZoneGrid AllZones="AllZones" 
                        SelectedZone="SelectedZone" 
                        OnZoneSelected="SelectZone" 
                        DbContext="DbContext" />
   </div>
   ```

That's it! The `WeatherZoneGrid` component handles all the complex grid functionality and database interactions for you.

This approach provides several benefits:
- **Reusable**: The grid component can be used in other parts of the application
- **Maintainable**: All grid-related logic is contained in one component
- **Testable**: The component can be tested independently
- **Clean separation**: The Home page focuses on weather display, not grid management

### What You've Accomplished

By using the provided `WeatherZoneGrid` component, you have:
1. ✅ Added Entity Framework using statement to `Home.razor`
2. ✅ Replaced the basic grid with a database-integrated component
3. ✅ Gained favorites functionality with database persistence
4. ✅ Maintained clean separation of concerns in your code

The new component handles:
- Displaying zones in a sortable, filterable grid
- "Show only favorites" checkbox functionality
- Star icons for favoriting/unfavoriting zones
- Database persistence of favorite zones
- Search functionality by name and state

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

> Note: The `Zone` type is a `record`, so equality is by value. When the UI checks `FavoriteZones.Contains(context)`, it's comparing by the record's values (like Key/Name/State), which is the intended behavior for favorites.

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

Head over to [Module #8: Integration Testing](../Lesson-08-Integration-Testing/README.md) to learn how to write integration tests for your .NET Aspire application.

**Next**: [Module #8: Integration Testing](../Lesson-08-Integration-Testing/README.md)
