# Integración de Base de Datos

## Introducción

En este módulo, integraremos una base de datos PostgreSQL con nuestra aplicación. Utilizaremos Entity Framework Core (EF Core) para interactuar con la base de datos. Además, configuraremos PgAdmin para administrar nuestra base de datos PostgreSQL.

## Configuración de PostgreSQL

.NET Aspire proporciona soporte integrado para PostgreSQL a través del paquete `Aspire.Hosting.PostgreSQL`. Para configurar PostgreSQL:

1. Instale el paquete NuGet requerido en su proyecto AppHost:

```xml
<PackageReference Include="Aspire.Hosting.PostgreSQL" Version="9.4.2" />
```

1. Actualice el Program.cs del AppHost para agregar PostgreSQL:

```csharp
var postgres = builder.AddPostgres("postgres")
    .WithDataVolume(isReadOnly: false);

var weatherDb = postgres.AddDatabase("weatherdb");
```

La configuración `WithDataVolume(isReadOnly: false)` garantiza que sus datos persistan entre reinicios del contenedor. Los datos se almacenan en un volumen Docker que existe fuera del contenedor, permitiendo que sobreviva a los reinicios del contenedor. Esto es opcional para el taller—si lo omite, el ejemplo aún funciona; simplemente no conservará los datos entre ejecuciones.

### Nuevo en .NET Aspire 9.4: Inicialización de base de datos mejorada

.NET Aspire 9.4 introduce el método mejorado `WithInitFiles()` para todos los proveedores de base de datos, reemplazando el método más complejo `WithInitBindMount()`:

```csharp
var postgres = builder.AddPostgres("postgres")
    .WithDataVolume(isReadOnly: false)
    .WithInitFiles("./database-init");  // Inicialización simplificada desde archivos
```

Este método funciona de manera consistente en todos los proveedores de base de datos (PostgreSQL, MySQL, MongoDB, Oracle) y proporciona mejor manejo de errores y configuración simplificada. El uso de `WithInitFiles` es opcional para este taller; la integración de base de datos funciona sin él.

Para asegurar el inicio adecuado de la aplicación, configuraremos la aplicación web para esperar la base de datos:

```csharp
var web = builder.AddProject<Projects.MyWeatherHub>("myweatherhub")
    .WithReference(weatherDb)
    .WaitFor(postgres)  // Asegura que la base de datos esté lista antes de que inicie la app
    .WithExternalHttpEndpoints();
```

## Configuración de Entity Framework Core

Ahora, configuremos Entity Framework Core en nuestra aplicación web para usar PostgreSQL.

1. Instale los paquetes NuGet requeridos en el proyecto MyWeatherHub:

```xml
<PackageReference Include="Aspire.Npgsql.EntityFrameworkCore.PostgreSQL" Version="9.4.2" />
<PackageReference Include="Microsoft.EntityFrameworkCore.Design" Version="9.0.0" />
```

1. Cree un DbContext para su aplicación:

```csharp
public class WeatherDbContext : DbContext
{
    public WeatherDbContext(DbContextOptions<WeatherDbContext> options)
        : base(options)
    {
    }

    public DbSet<WeatherForecast> WeatherForecasts { get; set; }
}

public class WeatherForecast
{
    public int Id { get; set; }
    public DateTime Date { get; set; }
    public int TemperatureC { get; set; }
    public string? Summary { get; set; }
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
```

1. Registre el DbContext en su Program.cs:

```csharp
builder.AddNpgsqlDbContext<WeatherDbContext>("weatherdb");
```

## Prueba de la integración

1. Ejecute su aplicación AppHost
1. Abra el panel de Aspire
1. Verifique que PostgreSQL y PgAdmin estén ejecutándose
1. Acceda a PgAdmin a través del panel para explorar su base de datos

**Siguiente**: [Módulo #8 - Pruebas de integración](8-integration-testing.md)
