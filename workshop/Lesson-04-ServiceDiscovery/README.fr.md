# Découverte de services

.NET Aspire inclut des fonctionnalités permettant de configurer la découverte de services au moment du développement et des tests. La fonctionnalité de découverte de services fonctionne en fournissant une configuration au format attendu par le résolveur de point de terminaison basé sur la configuration du projet .NET Aspire AppHost aux projets de service individuels ajoutés au modèle d'application.

## Configuration de la découverte de services

Actuellement, `MyWeatherHub` utilise une configuration statique pour se connecter à `Api`. Ce n’est pas idéal pour plusieurs raisons, notamment :

- Le numéro de port du service `Api` peut changer.
- L'adresse IP du service `Api` peut changer.
- Plusieurs paramètres de configuration devront être définis pour les paramètres http et https.
- À mesure que nous ajoutons plus de services, la configuration deviendrait plus complexe.

Pour résoudre ces problèmes, nous utiliserons la fonctionnalité de découverte de services fournie par le projet .NET Aspire AppHost. Cela permettra au service `MyWeatherHub` de découvrir le service `Api` au moment de l'exécution.

1. Ouvrez le fichier `Program.cs` dans le projet `AppHost`.
1. Auparavant, nous avons ajouté l'orchestration pour inclure plusieurs projets en utilisant la méthode `builder.AddProject`. Cela a renvoyé un `IResourceBuild` qui peut être utilisé pour référencer des projets. Référençons le projet `Api` dans le projet `MyWeatherHub` en mettant à jour le code :

  ```csharp
  var api = builder.AddProject<Projects.Api>("api");

  var web = builder.AddProject<Projects.MyWeatherHub>("myweatherhub")
  .AvecRéférence(api)
  .WithExternalHttpEndpoints();
  ```

1. La méthode `WithReference` est utilisée pour référencer le projet `Api`. Cela permettra au projet `MyWeatherHub` de découvrir le projet `Api` au moment de l'exécution.
1. Si vous choisissez ultérieurement de déployer cette application, vous aurez besoin de l'appel à `WithExternalHttpEndpoints` pour vous assurer qu'elle est publique pour le monde extérieur.

## Activation de la découverte de services dans MyWeatherHub

Lorsque nous avons ajouté `ServiceDefaults` aux projets, nous les avons automatiquement inscrits dans le système de découverte de services. Cela signifie que le projet `MyWeatherHub` est déjà configuré pour utiliser la découverte de services.

Certains services exposent plusieurs points de terminaison nommés. Les points de terminaison nommés peuvent être résolus en spécifiant le nom du point de terminaison dans la partie hôte de l'URI de la requête HTTP, en suivant le format `scheme://_endpointName.serviceName`. Par exemple, si un service nommé `basket` expose un point de terminaison nommé `dashboard`, alors l'URI `scheme+http://_dashboard.basket` peut être utilisé pour spécifier ce point de terminaison, par exemple :

```csharp
builder.Services.AddHttpClient<BasketServiceClient>(
  client statique => client.BaseAddress = new("https+http://basket"));

builder.Services.AddHttpClient<BasketServiceDashboardClient>(
  client statique => client.BaseAddress = new("https+http://_dashboard.basket"));
```

Dans l'exemple ci-dessus, `BasketServiceClient` utilisera le point de terminaison par défaut du service `basket`, tandis que `BasketServiceDashboardClient` utilisera le point de terminaison `dashboard` du service `basket`. Maintenant, mettons à jour le projet `MyWeatherHub` pour utiliser la découverte de services pour nous connecter au service `Api`.

Cela peut être accompli en mettant à jour les paramètres de configuration `WeatherEndpoint` existants dans `appsettings.json`. Ceci est pratique lorsque vous activez .NET Aspire dans une application déployée existante, car vous pouvez continuer à utiliser vos paramètres de configuration existants.

1. Ouvrez le fichier `appsettings.json` dans le projet `MyWeatherHub`.

1. Modifiez les paramètres de configuration `WeatherEndpoint` pour utiliser la découverte de services :

  ```json
  "WeatherEndpoint": "https+http://api"
  ```

1. Le paramètre de configuration `WeatherEndpoint` utilise désormais la découverte de services pour se connecter au service `Api`.

En option, nous pouvons mettre à jour l'URL pour ne pas utiliser les paramètres de configuration `WeatherEndpoint`.

1. Ouvrez le fichier `Program.cs` dans le projet `MyWeatherHub`.
1. Mettez à jour les paramètres de configuration `WeatherEndpoint` pour utiliser la découverte de services :

  ```csharp
  builder.Services.AddHttpClient<NwsManager>(
  client statique => client.BaseAddress = new("https+http://api"));
  ```

## Exécutez l'application

1. Exécutez l'application en appuyant sur `F5` ou en sélectionnant l'option `Start Debugging`.
1. Ouvrez `MyWeatheApp` en sélectionnant le point de terminaison dans le tableau de bord.
1. Notez que l'application `MyWeatherHub` fonctionne toujours et utilise désormais la découverte de services pour se connecter au service `Api`.
1. Dans le tableau de bord, cliquez sur `Details` pour le projet `MyWeatherHub`. Cela fera apparaître tous les paramètres configurés par .NET Aspire lors de l'exécution de l'application à partir de l'hôte de l'application.
1. Cliquez sur l'icône en forme d'œil pour révéler les valeurs et faites défiler vers le bas où vous verrez `services__api_http_0` et `services__api_https_0` configurés avec les valeurs correctes du service `Api`.

  ![Paramètres de découverte de services dans le tableau de bord](../media/dashboard-servicediscovery.png)

## Conclusion

Ce n'était que le début de ce que nous pouvons faire avec la découverte de services et .NET Aspire. À mesure que notre application se développe et que nous ajoutons davantage de services, nous pouvons continuer à utiliser la découverte de services pour connecter les services au moment de l'exécution. Cela nous permettra de faciliter
