# Intégration de base de données

## Introduction

Dans ce module, nous allons intégrer une base de données PostgreSQL avec notre application. Nous utiliserons Entity Framework Core (EF Core) pour interagir avec la base de données. De plus, nous configurerons PgAdmin pour gérer notre base de données PostgreSQL.

## Configuration de PostgreSQL

.NET Aspire fournit un support intégré pour PostgreSQL via le package `Aspire.Hosting.PostgreSQL`. Pour configurer PostgreSQL :

1. Installez le package NuGet requis dans votre projet AppHost :

```xml
<PackageReference Include="Aspire.Hosting.PostgreSQL" Version="9.4.2" />
```

1. Mettez à jour le Program.cs de l'AppHost pour ajouter PostgreSQL :

```csharp
var postgres = builder.AddPostgres("postgres")
    .WithDataVolume(isReadOnly: false);

var weatherDb = postgres.AddDatabase("weatherdb");
```

La configuration `WithDataVolume(isReadOnly: false)` garantit que vos données persistent entre les redémarrages de conteneur. Les données sont stockées dans un volume Docker qui existe en dehors du conteneur, lui permettant de survivre aux redémarrages de conteneur. Ceci est optionnel pour l'atelier—si vous l'omettez, l'exemple fonctionne toujours ; vous ne conservez simplement pas les données entre les exécutions.

### Nouveau dans .NET Aspire 9.4 : Initialisation de base de données améliorée

.NET Aspire 9.4 introduit la méthode améliorée `WithInitFiles()` pour tous les fournisseurs de base de données, remplaçant la méthode plus complexe `WithInitBindMount()` :

```csharp
var postgres = builder.AddPostgres("postgres")
    .WithDataVolume(isReadOnly: false)
    .WithInitFiles("./database-init");  // Initialisation simplifiée à partir de fichiers
```

Cette méthode fonctionne de manière cohérente sur tous les fournisseurs de base de données (PostgreSQL, MySQL, MongoDB, Oracle) et fournit une meilleure gestion des erreurs et une configuration simplifiée. L'utilisation de `WithInitFiles` est optionnelle pour cet atelier ; l'intégration de base de données fonctionne sans cela.

Pour assurer un démarrage d'application approprié, nous configurerons l'application web pour attendre la base de données :

```csharp
var web = builder.AddProject<Projects.MyWeatherHub>("myweatherhub")
    .WithReference(weatherDb)
    .WaitFor(postgres)  // Assure que la base de données est prête avant le démarrage de l'app
    .WithExternalHttpEndpoints();
```

## Ajout de PgAdmin pour la gestion de base de données

Pour faciliter la gestion de base de données, ajoutons PgAdmin à notre configuration :

1. Installez le package NuGet requis :

```xml
<PackageReference Include="Aspire.Hosting.PgAdmin" Version="9.4.0" />
```

1. Ajoutez PgAdmin à votre AppHost :

```csharp
var pgAdmin = builder.AddPgAdmin("pgadmin")
    .WithReference(postgres);
```

## Configuration d'Entity Framework Core

Maintenant, configurons Entity Framework Core dans notre application web pour utiliser PostgreSQL.

1. Installez les packages NuGet requis dans le projet MyWeatherHub :

```xml
<PackageReference Include="Aspire.Npgsql.EntityFrameworkCore.PostgreSQL" Version="9.4.2" />
<PackageReference Include="Microsoft.EntityFrameworkCore.Design" Version="9.0.0" />
```

1. Créez un DbContext pour votre application :

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

1. Enregistrez le DbContext dans votre Program.cs :

```csharp
builder.AddNpgsqlDbContext<WeatherDbContext>("weatherdb");
```

## Test de l'intégration

1. Exécutez votre application AppHost
1. Ouvrez le tableau de bord Aspire
1. Vérifiez que PostgreSQL et PgAdmin sont en cours d'exécution
1. Accédez à PgAdmin via le tableau de bord pour explorer votre base de données

**Suivant**: [Module #8 - Tests d'intégration](8-integration-testing.md)
