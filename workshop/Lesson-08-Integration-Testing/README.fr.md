# Tests d'Intégration avec .NET Aspire

## Introduction

Dans ce module, nous couvrirons les tests d'intégration en utilisant `Aspire.Hosting.Testing` avec `MSTest`. Les tests d'intégration sont cruciaux pour s'assurer que différentes parties de votre application fonctionnent ensemble comme attendu. Nous créerons un projet de test séparé pour tester à la fois l'API et l'application web.

## Différence Entre les Tests Unitaires et les Tests d'Intégration

Les tests unitaires se concentrent sur le test de composants individuels ou d'unités de code de manière isolée. Ils s'assurent que chaque unité fonctionne correctement de manière autonome. En contraste, les tests d'intégration vérifient que différents composants de l'application fonctionnent ensemble comme attendu. Ils testent les interactions entre diverses parties du système, telles que les APIs, les bases de données et les applications web.

Dans le contexte des applications distribuées avec .NET Aspire, les tests d'intégration sont essentiels pour s'assurer que les différents services et composants communiquent et fonctionnent correctement ensemble.

## Création du Projet de Tests d'Intégration

1. Créez un nouveau projet de test nommé `IntegrationTests`.
1. Ajoutez des références aux packages requis dans le fichier `IntegrationTests.csproj` :

```xml
<Project Sdk="Microsoft.NET.Sdk">
    <PropertyGroup>
        <TargetFramework>net9.0</TargetFramework>
        <ImplicitUsings>enable</ImplicitUsings>
        <Nullable>enable</Nullable>
        <IsPackable>false</IsPackable>
        <IsTestProject>true</IsTestProject>
    </PropertyGroup>

    <PropertyGroup>
        <EnableMSTestRunner>true</EnableMSTestRunner>
        <OutputType>Exe</OutputType>
    </PropertyGroup>

    <ItemGroup>
        <PackageReference Include="Aspire.Hosting.Testing" Version="9.4.2" />
        <PackageReference Include="MSTest" Version="3.10.4" />
    </ItemGroup>

    <ItemGroup>
        <ProjectReference Include="..\AppHost\AppHost.csproj" />
    </ItemGroup>

    <ItemGroup>
        <Using Include="System.Net" />
        <Using Include="Microsoft.Extensions.DependencyInjection" />
        <Using Include="Aspire.Hosting.ApplicationModel" />
        <Using Include="Aspire.Hosting.Testing" />
        <Using Include="Microsoft.VisualStudio.TestTools.UnitTesting" />
    </ItemGroup>
</Project>
```

Ce fichier projet est assez standard pour un projet de test. Les éléments clés sont :

- Une `PackageReference` au package NuGet [Aspire.Hosting.Testing](https://www.nuget.org/packages/Aspire.Hosting.Testing), qui fournit les types et APIs nécessaires pour tester les applications .NET Aspire.
- Une `ProjectReference` au projet AppHost, qui donne au projet de test l'accès à la définition de l'application distribuée cible.
- Les paramètres `EnableMSTestRunner` et `OutputType` pour configurer le projet de test à s'exécuter avec le runner MSTest natif.

> Note : N'importe quelle version MSTest 3.x convient pour cet atelier. Si votre environnement fournit une 3.x plus récente, vous pouvez l'utiliser.

1. Créez des classes de test pour les tests d'intégration :

Le fichier `IntegrationTests.cs` teste la fonctionnalité de l'API et de l'application web :

```csharp
namespace MyWeatherHub.Tests;

[TestClass]
public class IntegrationTests
{
    [TestMethod]
    public async Task TestApiGetZones()
    {
        // Arrange
        var appHost = await DistributedApplicationTestingBuilder
            .CreateAsync<Projects.AppHost>();

        appHost.Services.ConfigureHttpClientDefaults(clientBuilder =>
        {
            clientBuilder.AddStandardResilienceHandler();
        });

        await using var app = await appHost.BuildAsync();

        var resourceNotificationService = app.Services
            .GetRequiredService<ResourceNotificationService>();

        await app.StartAsync();

        // Act
        var httpClient = app.CreateHttpClient("api");

        await resourceNotificationService.WaitForResourceAsync(
                "api",
                KnownResourceStates.Running
            )
            .WaitAsync(TimeSpan.FromSeconds(30));

        var response = await httpClient.GetAsync("/zones");

        // Assert
        response.EnsureSuccessStatusCode();
        var zones = await response.Content.ReadFromJsonAsync<Zone[]>();
        Assert.IsNotNull(zones);
        Assert.IsTrue(zones.Length > 0);
    }

    [TestMethod]
    public async Task TestWebAppHomePage()
    {
        // Arrange
        var appHost = await DistributedApplicationTestingBuilder
            .CreateAsync<Projects.AppHost>();

        appHost.Services.ConfigureHttpClientDefaults(clientBuilder =>
        {
            clientBuilder.AddStandardResilienceHandler();
        });

        await using var app = await appHost.BuildAsync();

        var resourceNotificationService = app.Services
            .GetRequiredService<ResourceNotificationService>();

        await app.StartAsync();

        // Act
        var httpClient = app.CreateHttpClient("myweatherhub");

        await resourceNotificationService.WaitForResourceAsync(
                "myweatherhub",
                KnownResourceStates.Running
            )
            .WaitAsync(TimeSpan.FromSeconds(30));

        var response = await httpClient.GetAsync("/");

        // Assert
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();
        Assert.IsTrue(content.Contains("MyWeatherHub"));
    }
}

public record Zone(string Key, string Name, string State);
```

Cette classe de test démontre comment tester votre application distribuée. Examinons ce que ces tests font :

- Les deux tests suivent un modèle similaire, utilisant `DistributedApplicationTestingBuilder.CreateAsync<Projects.AppHost>()` pour créer de manière asynchrone une instance de votre hôte d'application.
- L'`appHost` est configuré avec des gestionnaires de résilience HTTP standard, qui fournissent des politiques de retry et des circuit breakers pour une communication HTTP plus robuste.
- Le test appelle `appHost.BuildAsync()` pour construire l'application puis récupère le `ResourceNotificationService` du conteneur DI.
- Après avoir démarré l'application avec `app.StartAsync()`, un `HttpClient` est créé spécifiquement pour la ressource testée (soit "api" soit "myweatherhub").
- Le test attend que la ressource cible atteigne l'état "Running" avant de procéder, s'assurant que le service est prêt à accepter des requêtes.
- Finalement, des requêtes HTTP sont faites à des endpoints spécifiques, et des assertions vérifient les réponses.

Dans le premier test, nous vérifions que l'endpoint `/zones` de l'API retourne une collection valide de données de zone. Dans le second test, nous vérifions que la page d'accueil de l'application web se charge avec succès et contient le contenu attendu.

Le fichier `EnvVarTests.cs` vérifie la résolution des variables d'environnement :

```csharp
namespace MyWeatherHub.Tests;

[TestClass]
public class EnvVarTests
{
    [TestMethod]
    public async Task WebResourceEnvVarsResolveToApiService()
    {
        // Arrange
        var appHost = await DistributedApplicationTestingBuilder
            .CreateAsync<Projects.AppHost>();

        var frontend = (IResourceWithEnvironment)appHost.Resources
            .Single(static r => r.Name == "myweatherhub");

        // Act
        var envVars = await frontend.GetEnvironmentVariableValuesAsync(
            DistributedApplicationOperation.Publish);

        // Assert
        CollectionAssert.Contains(envVars,
            new KeyValuePair<string, string>(
                key: "services__api__https__0",
                value: "{api.bindings.https.url}"));
    }
}
```

Ce test se concentre sur la vérification de la configuration de découverte de service :

- Il utilise `DistributedApplicationTestingBuilder.CreateAsync<Projects.AppHost>()` pour créer une instance de l'hôte d'application.
- Au lieu de démarrer l'application, il récupère directement une instance `IResourceWithEnvironment` représentant le frontend web ("myweatherhub").
- Il appelle `GetEnvironmentVariableValuesAsync()` avec l'argument `DistributedApplicationOperation.Publish` pour obtenir les variables d'environnement qui seraient publiées à la ressource.
- Finalement, il affirme que le frontend web a une variable d'environnement qui se résout à l'URL du service API, confirmant que la découverte de service est correctement configurée.

Ce test est particulièrement précieux car il vérifie que les services de votre application sont correctement connectés via les variables d'environnement, ce qui est la façon dont .NET Aspire gère la découverte de service dans les applications distribuées.

> Note : Si vous voyez un fichier `WeatherBackgroundTests.cs` dans la solution complète qui est vide, c'est un placeholder pour de futurs tests de tâches en arrière-plan et peut être ignoré pour cet atelier.

## Exécution des Tests d'Intégration

### Utilisation de la Ligne de Commande

1. Ouvrez un terminal et naviguez vers le dossier `complete`.
1. Exécutez les tests d'intégration en utilisant la commande `dotnet test` :

```bash
dotnet test IntegrationTests/IntegrationTests.csproj
```

### Utilisation de l'Explorateur de Tests de Visual Studio

1. Ouvrez la solution dans Visual Studio
1. Ouvrez l'Explorateur de Tests en allant à Affichage > Explorateur de Tests (ou appuyez sur Ctrl+E, T)
1. Dans la fenêtre de l'Explorateur de Tests, vous verrez tous les tests de votre solution

![Explorateur de Tests Visual Studio](../media/vs-test-explorer.png)

1. Vous pouvez :
   - Exécuter tous les tests en cliquant sur le bouton "Exécuter Tout" en haut
   - Exécuter un test spécifique en faisant un clic droit dessus et en sélectionnant "Exécuter"
   - Exécuter seulement les tests échoués en cliquant sur le bouton "Exécuter les Tests Échoués"
   - Exécuter les tests en mode debug en faisant un clic droit et en sélectionnant "Déboguer"
   - Voir les résultats des tests et la sortie dans la fenêtre de l'Explorateur de Tests

Les tests vérifieront que :

- Les variables d'environnement sont correctement configurées
- Les endpoints de l'API fonctionnent correctement
- L'application web fonctionne comme attendu

Lors de l'exécution de ces tests, tous les logs de ressources sont redirigés vers la `DistributedApplication` par défaut. Cette redirection de logs permet des scénarios où vous voulez affirmer qu'une ressource se connecte correctement, bien que nous ne le fassions pas dans ces tests particuliers.

## Outils de Test Supplémentaires

Playwright est un outil puissant pour les tests de bout en bout. Il vous permet d'automatiser les interactions du navigateur et de vérifier que votre application fonctionne comme attendu du point de vue de l'utilisateur. Playwright supporte plusieurs navigateurs, incluant Chromium, Firefox et WebKit.

### Cas d'Usage

Playwright peut être utilisé pour effectuer des tests de bout en bout de votre application web. Il peut simuler les interactions utilisateur, comme cliquer sur des boutons, remplir des formulaires et naviguer entre les pages. Ceci assure que votre application se comporte correctement dans des scénarios du monde réel.

### Concepts de Haut Niveau

- **Automatisation de Navigateur** : Playwright peut lancer et contrôler des navigateurs pour effectuer des tests automatisés.
- **Tests Multi-navigateurs** : Playwright supporte les tests à travers différents navigateurs pour assurer la compatibilité.
- **Mode Sans Tête** : Playwright peut exécuter des tests en mode sans tête, ce qui signifie que le navigateur s'exécute en arrière-plan sans interface utilisateur graphique.
- **Assertions** : Playwright fournit des assertions intégrées pour vérifier que les éléments sont présents, visibles et ont les propriétés attendues.

Pour plus d'informations sur Playwright, consultez la [documentation officielle](https://playwright.dev/dotnet/).

## Conclusion

Dans ce module, nous avons couvert les tests d'intégration en utilisant `Aspire.Hosting.Testing` avec `MSTest`. Nous avons créé un projet de test séparé pour tester à la fois l'API et l'application web, en suivant des modèles similaires à l'approche `WebApplicationFactory` dans ASP.NET Core mais adaptés pour les applications distribuées.

Nos tests ont vérifié trois aspects critiques de l'application distribuée :

1. La fonctionnalité de l'API (testant que les endpoints retournent les données attendues)
1. La fonctionnalité de l'application web (testant que l'UI se rend correctement)
1. Le mécanisme de découverte de service (testant que les services peuvent se trouver et communiquer entre eux)

Pour une plongée plus approfondie dans les tests avec .NET Aspire, incluant une vidéo explicative, consultez l'article de blog [Getting started with testing and .NET Aspire](https://devblogs.microsoft.com/dotnet/getting-started-with-testing-and-dotnet-aspire/).

Maintenant, apprenons sur les options de déploiement lors de l'utilisation de .NET Aspire.

**Suivant** : [Module #9 : Déploiement](../Lesson-09-Deployment/README.md)
