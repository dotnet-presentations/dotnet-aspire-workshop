# Tableau de bord et orchestration avec l'hôte de l'application .NET Aspire

.NET Aspire fournit des API pour exprimer les ressources et les dépendances au sein de votre application distribuée. En plus de ces API, il existe des outils qui permettent certains scénarios convaincants. L'orchestrateur est destiné à des fins de développement local.

Avant de continuer, considérez quelques termes courants utilisés dans .NET Aspire :

* *Modèle d'application (App model)* : un ensemble de ressources qui composent votre application distribuée (DistributedApplication). Pour une définition plus formelle, consultez Définir le modèle d’application.
* *Projet hôte/Orchestreur d'application* : projet .NET qui orchestre le modèle d'application, nommé avec le suffixe *.AppHost (par convention).
* *Ressource* : une ressource représente une partie d'une application, qu'il s'agisse d'un projet .NET, d'un conteneur ou d'un exécutable, ou d'une autre ressource telle qu'une base de données, un cache ou un service cloud (tel qu'un service de stockage).
* *Référence* : Une référence définit une connexion entre des ressources, exprimée sous forme de dépendance à l'aide de l'API WithReference. Pour plus d’informations, consultez Ressources de référence.

## Créer un projet hôte d'application

### Visual Studio et Visual Studio Code

1. Ajoutez un nouveau projet à la solution appelé `AppHost`:

* Faites un clic droit sur la solution et sélectionnez `Add` > `New project`.
* Sélectionnez le modèle de projet `.NET Aspire App Host`.
* Nommez le projet `AppHost`.
* Cliquez sur `Next` > `Create`.

  *Visual Studio*
  ![Boîte de dialogue Visual Studio pour ajouter un projet hôte d'application](.../media/vs-add-apphost.png)

  *Code de studio visuel*
  ![Boîte de dialogue Visual Studio Code pour ajouter un projet hôte d'application](.../media/vsc-add-apphost.png)

### Ligne de commande

1. Créez un nouveau projet à l'aide de la commande `dotnet new aspire-apphost` :

  ```bash
  dotnet nouveau aspire-apphost -n AppHost
  ```

## Configurer les paramètres par défaut du service

1. Ajoutez une référence aux projets `Api` et `MyWeatherHub` dans le nouveau projet `AppHost` :

* Faites un clic droit sur le projet `AppHost` et sélectionnez `Ajouter` > `Référence`.
* Vérifiez les projets `Api` et `MyWeatherHub` et cliquez sur `OK`.

  > Astuce de pro : dans Visual Studio 2022, vous pouvez faire glisser et déposer le projet sur un autre projet pour ajouter une référence.

1. Lorsque ces références sont ajoutées, les générateurs de sources génèrent automatiquement le code nécessaire pour référencer les projets dans l'App Host.

## Orchestrer l'application

1. Dans le projet `AppHost`, mettez à jour le fichier `Program.cs` en ajoutant la ligne suivante immédiatement après la ligne `var builder = DistributedApplication.CreateBuilder(args);` :

  ```csharp
  var api = builder.AddProject<Projects.Api>("api");

  var web = builder.AddProject<Projects.MyWeatherHub>("myweatherhub");
  ```

## Exécutez l'application

1. Définissez le projet `AppHost` comme projet de démarrage dans Visual Studio en cliquant avec le bouton droit sur `AppHost` et en cliquant sur `Définir le projet par défaut`.
1. Si vous utilisez Visual Studio Code, ouvrez le fichier `launch.json` et remplacez tout le contenu par ce qui suit :

 ```json
 {
        "version": "0.2.0",
        "configurations": [
            {
                "name": "Run AppHost",
                "type": "dotnet",
                "request": "launch",
                "projectPath": "${workspaceFolder}\\AppHost\\AppHost.csproj"
            }
        ]
    }
 ```

1. Exécutez App Host à l'aide du panneau `Exécuter et déboguer` dans Visual Studio Code ou Visual Studio.
1. Le tableau de bord .NET Aspire s'ouvrira dans votre navigateur par défaut et affichera les ressources et dépendances de votre application.

  ![Tableau de bord .NET Aspire](.../media/dashboard.png)

1. Ouvrez le tableau de bord météo en cliquant sur l'un des points de terminaison de `MyWeatherHub` qui sera [https://localhost:7274](https://localhost:7274).
1. Notez que les projets `Api` et `MyWeatherHub` s'exécutent dans le même processus et peuvent communiquer entre eux de la même manière qu'avant en utilisant les paramètres de configuration.
1. Cliquez sur le bouton `Afficher les journaux` pour voir les journaux des projets `Api` et `MyWeatherHub`.
1. Sélectionnez l'onglet `Traces` et sélectionnez `Vue` sur une trace où l'API est appelée.

  ![Tableau de bord .NET Aspire](.../media/dashboard-trace.png)]

1. Explorez l'onglet `Métriques` pour voir les métriques des projets `Api` et `MyWeatherHub`.

  ![Tableau de bord .NET Aspire](.../media/dashboard-metrics.png)

## Créer une erreur

1. Ouvrez l'onglet `Structuré` sur le tableau de bord.
1. Réglez le `Niveau` sur `Erreur` et notez qu'aucune erreur n'apparaît.
1. Sur le site Web `MyWeatherApp`, cliquez sur plusieurs villes différentes pour générer des erreurs. Habituellement, 5 villes différentes génèrent une erreur.
1. Après avoir généré les erreurs, l'onglet `Structuré` se mettra automatiquement à jour sur le tableau de bord et remarquera que les erreurs sont affichées.

  ![Tableau de bord .NET Aspire](.../media/dashboard-error.png)

1. Cliquez sur `Trace` ou `Détails` pour voir le message d'erreur et la trace de la pile.
