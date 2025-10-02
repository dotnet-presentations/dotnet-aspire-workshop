# Paramètres par défaut (Smart Defaults)

## Introduction

.NET Aspire fournit un ensemble de valeurs par défaut pour les services couramment utilisés dans les applications .NET. Ces valeurs par défaut sont conçues pour vous aider à démarrer rapidement et à offrir une expérience cohérente sur différents types d'applications. Ceci comprend:

- Télémétrie : métriques, traçage, journalisation
- Résilience
- Contrôles de santé
- Découverte des services

## Créer le projet "Service Defaults"

### Visual Studio & Visual Studio Code

1. Ajoutez un nouveau projet à la solution appelé `ServiceDefaults`:

- Faites un clic droit sur la solution et sélectionnez `Add` > `New Project`.
- Sélectionnez le modèle de projet `.NET Aspire Service Defaults`.
- Nommé le projet `ServiceDefaults`.
- Cliquez `Next` > `Create`.

 *Visual Studio*
 ![Boîte de dialogue Visual Studio pour ajouter un projet de valeurs par défaut du service](.../media/vs-add-servicedefaults.png)

 *Visual Studio Code*
 ![Boîte de dialogue Visual Studio Code pour ajouter un projet de valeurs par défaut du service](.../media/vsc-add-servicedefaults.png)

### Ligne de commande

1. Créez un nouveau projet à l'aide de la commande`dotnet new aspire-servicedefaults`:

 ```bash
 dotnet new aspire-servicedefaults -n ServiceDefaults
 ```

## Configurer les paramètres par défaut du service

1. Ajoutez une référence au projet `ServiceDefaults` dans les projets `Api` et `MyWeatherHub` :

   - Faites un clic droit sur le projet `Api` et sélectionnez `Add` > `Reference`.
     - Vérifiez le projet `ServiceDefaults` et cliquez sur `OK`.
   - Faites un clic droit sur le projet `MyWeatherHub` et sélectionnez `Add` > `Reference`.
     - Vérifiez le projet `ServiceDefaults` et cliquez sur `OK`.

 > Conseil de pro: dans Visual Studio 2022, vous pouvez faire glisser et déposer le projet sur un autre projet pour ajouter une référence.

1. Dans les projets `Api` et `MyWeatherHub`, modifiez leurs fichiers `Program.cs`, en ajoutant la ligne suivante immédiatement après leur ligne `var builder = WebApplication.CreateBuilder(args);` :

 ```csharp
 builder.AddServiceDefaults();
 ```

1. Dans les projets `Api` et `MyWeatherHub`, modifiez leurs fichiers `Program.cs`, en ajoutant la ligne suivante immédiatement après leur ligne `var app = builder.Build();` :

 ```csharp
 app.MapDefaultEndpoints();
 ```

## Exécutez l'application

1. Exécutez l'application à l'aide d'une configuration de lancement multi-projets dans Visual Studio ou Visual Studio Code.

- Visual Studio : Faites un clic droit sur la solution `MyWeatherHub` et accédez aux propriétés. Sélectionnez « Api » et « MyWeatherHub » comme projets de démarrage, sélectionnez « OK ».
  - ![Propriétés de la solution Visual Studio](.../media/vs-multiproject.png)
  - Cliquez sur « Start » pour démarrer et déboguer les deux projets.
- Visual Studio Code : exécutez les projets `Api` et `MyWeatherHub` à l'aide du panneau `Exécuter et déboguer`. Nous avons fourni un fichier « launch.json » avec les configurations nécessaires pour exécuter les deux.

1. Test the application by navigating to the following URLs:

- [https://localhost:7032/openapi/v1.json](https://localhost:7032/openapi/v1.json) - API (OpenAPI document)
- [https://localhost:7274/](https://localhost:7274/) - MyWeatherHub

1. You should see the OpenAPI document (JSON) for the API and the MyWeatherHub home page.
1. You can also view the health checks for the API by navigating to [https://localhost:7032/health](https://localhost:7032/health).
1. You can also view the health checks for the MyWeatherHub by navigating to [https://localhost:7274/health](https://localhost:7274/health).
1. View the logs in the terminal to see the health checks and other telemetry data such as resiliency with Polly:

1. Vous devriez voir le document OpenAPI (JSON) de l'API et la page d'accueil de MyWeatherHub.
1. Vous pouvez également afficher les Contrôles de santé de l'API en accédant à [https://localhost:7032/health](https://localhost:7032/health).
1. Vous pouvez également afficher les Contrôles de santé de MyWeatherHub en accédant à [https://localhost:7274/health](https://localhost:7274/health).
1. Consultez les journaux (logs) dans le terminal pour voir les contrôles de santé et d'autres données de télémétrie telles que la résilience avec Polly :

 ```bash
 Polly: Information: Execution attempt. Source: '-standard//Standard-Retry', Operation Key: '', Result: '200', Handled: 'False', Attempt: '0', Execution Time: '13.0649'
 ```

1. Cliquez sur 5 villes différentes et une erreur "random" sera générée. Vous verrez la politique de nouvelle tentative de Polly en action.

 ```bash
 Polly: Warning: Execution attempt. Source: '-standard//Standard-Retry', Operation Key: '', Result: '500', Handled: 'True', Attempt: '0', Execution Time: '9732.8258'
 Polly: Warning: Resilience event occurred. EventName: 'OnRetry', Source: '-standard//Standard-Retry', Operation Key: '', Result: '500'
 System.Net.Http.HttpClient.NwsManager.ClientHandler: Information: Sending HTTP request GET http://localhost:5271/forecast/AKZ318
 ```
