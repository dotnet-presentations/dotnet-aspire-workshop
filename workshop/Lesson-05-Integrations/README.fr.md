# Intégrations .NET Aspire

Les composants .NET Aspire sont une suite organisée de packages NuGet spécifiquement sélectionnés pour faciliter l'intégration d'applications cloud natives avec des services et plates-formes de premier plan, notamment, mais sans s'y limiter, Redis et PostgreSQL. Chaque composant fournit des fonctionnalités cloud natives essentielles via un provisionnement automatique ou des modèles de configuration standardisés. Les composants .NET Aspire peuvent être utilisés sans le projet `AppHost` (l'orchestrateur), mais ils sont conçus pour fonctionner de manière optimale avec l’app host .NET Aspire.

Les composants .NET Aspire ne doivent pas être confondus avec les packages d'hébergement .NET Aspire, car ils répondent à des objectifs différents. Les packages d'hébergement sont utilisés pour modéliser et configurer diverses ressources dans une application .NET Aspire, tandis que les composants sont utilisés pour mapper la configuration à diverses bibliothèques clientes.

Il existe une liste sans cesse croissante de [composants .NET Aspire](https://learn.microsoft.com/dotnet/aspire/fundamentals/components-overview?tabs=dotnet-cli#available-components) créés et livrés par Microsoft et le communauté. .NET Aspire est flexible et n'importe qui peut créer son propre composant à intégrer à ses propres services.

Améliorons notre application en y ajoutant un composant. Nous ajouterons un composant qui nous aidera à nous connecter à un cache Redis pour améliorer les performances de notre API.

## Ajouter un composant Redis à l'hôte de l'application

Il existe deux types de mise en cache que nous pourrions intégrer dans notre application, notamment :

- **Output caching** : une méthode de mise en cache configurable et extensible pour stocker des réponses HTTP entières pour les requêtes futures.
- **Distributed caching** : cache partagé par plusieurs serveurs d'applications qui vous permet de mettre en cache des éléments de données spécifiques. Un cache distribué est généralement conservé en tant que service externe pour les serveurs d'applications qui y accèdent et peut améliorer les performances et l'évolutivité d'une application ASP.NET Core.

Nous intégrerons le composant _Output caching_ à notre hôte d'application. Ce composant nous aidera à mettre en cache la réponse de notre API dans le cache Redis.

Pour ajouter le composant Redis à notre hôte d'application, nous devons installer le package NuGet `Aspire.Hosting.Redis`. Ce package fournit les composants nécessaires pour configurer le service dans App Host. Redis est fourni via une image de conteneur dans cet atelier, et lorsque nous démarrons l'hôte de l'application .NET Aspire, il téléchargera automatiquement l'image du conteneur Redis et démarrera le serveur Redis.

Avec NuGet installé, nous pouvons le configurer.

1. Ouvrez le fichier `Program.cs` dans le projet `AppHost`.
1. Ajoutez le code suivant sous `var builder = DistributedApplication.CreateBuilder(args);`

 ```csharp
 var cache = builder.AddRedis("cache")
 ```

   Ici, nous avons configuré le cache Redis avec le nom `cache`. Ce nom est utilisé pour identifier le cache dans `Api` ou `MyWeatherHub`.

1. Modifiez l'API dans l'hôte de l'application avec une référence au cache.

 ```csharp
 var api = builder.AddProject<Projects.Api>("api")
   .WithReference(cache);
 ```

1. De plus, nous pourrions configurer [Redis Commander](https://joeferner.github.io/redis-commander/), un outil de gestion Redis. Dans le cadre du package `Aspire.Hosting.Redis`, Redis Commander est disponible dans le même composant. Pour ajouter Redis Commander, ajoutez le code suivant sous à la configuration Redis nouvellement ajoutée.

 ```csharp
 var cache = builder.AddRedis("cache")
   .WithRedisCommander();
 ```

## Exécutez l'application (démarrer Redis)

Nous n'avons apporté aucune modification aux projets `Api` ou `MyWeatherHub`, mais nous pouvons voir le cache Redis démarrer lorsque nous démarrons l'App Host.

> [!IMPORTANT]
> Étant donné que Redis s'exécute dans un conteneur, vous devrez vous assurer que Docker est exécuté sur votre machine.

1. Démarrez Docker Desktop ou Podman
1. Démarrez le projet App Host.
1. Vous verrez le téléchargement et le démarrage du conteneur Redis et du conteneur Redis Commander dans le tableau de bord et dans Docker Desktop.

  ![Redis s'exécutant dans le tableau de bord et le bureau](.../media/redis-started.png)

## Intégrer la mise en cache des sorties dans l'API

1. Installez le package NuGet `Aspire.StackExchange.Redis.OutputCaching` dans le projet `Api` pour accéder aux API Redis.
1. Ouvrez le fichier `Program.cs` dans le projet `Api`.
1. Ajoutez le code suivant sous `var builder = WebApplication.CreateBuilder(args);` en haut du fichier :

 ```csharp
 builder.AddRedisOutputCache("cache");
 ```

 > Notez que nous utilisons le nom `cache` pour référencer le cache Redis que nous avons configuré dans l'App Host.

1. Le `NwsManager` a déjà été configuré pour utiliser la mise en cache de sortie, mais avec un cache mémoire. Nous le mettrons à jour pour utiliser le cache Redis. Ouvrez le fichier `NwsManager.cs` dans le dossier `Data`.
1. Dans la classe `NwsManagerExtensions`, vous trouverez une méthode `AddNwsManager`.
1. **SUPPRIMER** le code suivant :

 ```csharp
 // Add default output caching
 services.AddOutputCache(options =>
 {
  options.AddBasePolicy(builder => builder.Cache());
 });
 ```

 Étant donné que nous avons configuré l'application pour utiliser le cache Redis dans le fichier `Program.cs`, nous n'avons plus besoin d'ajouter la politique de mise en cache de sortie par défaut.

## Exécutez l'application (tester la mise en cache de sortie)

1. Démarrez le projet App Host et ouvrez le projet `MyWeatherHub` depuis le tableau de bord
1. Cliquez sur une ville puis cliquez à nouveau dessus. Vous verrez que la réponse est mise en cache et que la deuxième requête est beaucoup plus rapide que la première sous l'onglet `Traces`.

 ![Mise en cache de sortie en action](.../media/output-caching.png)

1. Vous pouvez également voir la réponse mise en cache dans Redis Commander. Ouvrez Redis Commander en cliquant sur le point de terminaison `Redis Commander` dans le tableau de bord. Sous les statistiques, vous verrez les connexions et les commandes traitées.

 ![Redis Commander](.../media/redis-commander.png)

1. De plus, vous pouvez voir les journaux du cache Redis et de Redis Commander dans l'onglet `Console`.

 ![Journaux Redis](.../media/redis-logs.png)

## Conteneurs Redis personnalisés

Les composants .NET Aspire sont flexibles et personnalisables. Par défaut, le composant Redis utilise une image de conteneur Redis de Docker Hub. Cependant, vous pouvez utiliser votre propre image de conteneur Redis en fournissant le nom de l'image et la balise après la méthode `AddRedis`. Par exemple, si vous disposez d'une image de conteneur Redis personnalisée telle que [Garnet](https://github.com/microsoft/garnet), vous pouvez fournir le nom de l'image et la balise dans l'hôte d'application comme suit :

```csharp
var cache = builder.AddRedis("cache")
 .WithImage("ghcr.io/microsoft/garnet")
 .WithImageTag("latest")
 .WithRedisCommander();
```

1. Exécutez l'application et vous verrez maintenant Garnet s'exécuter dans le tableau de bord et dans Docker Desktop.

  ![Garnet s'exécutant dans le tableau de bord et le bureau](.../media/garnet-started.png)

1. Vous pouvez également voir les journaux de Garnet dans l'onglet `Console`.

  ![Journaux Garnet](.../media/garnet-logs.png)

## Résumé

Dans cette section, nous avons ajouté un composant Redis à App Host et intégré la mise en cache de sortie dans l'API. Nous avons vu comment la réponse était mise en cache dans le cache Redis et comment la deuxième requête était beaucoup plus rapide que la première. Nous avons également vu comment utiliser Redis Commander pour gérer le cache Redis.

Il existe de nombreux autres composants disponibles que vous pouvez utiliser pour intégrer vos services. Vous pouvez trouver la liste des composants disponibles [dans la documentation .NET Aspire](https://learn.microsoft.com/dotnet/aspire/fundamentals/components-overview?tabs=dotnet-cli#available-components).

Une prochaine étape naturelle consisterait à intégrer une base de données ou à exploiter Azure Redis Cache en tant que solution hébergée. Les composants pour ceux-ci et bien d’autres sont disponibles sur NuGet.
