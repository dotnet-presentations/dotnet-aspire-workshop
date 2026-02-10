# Configuration et installation

Cet atelier utilisera les outils suivants:

- [SDK .NET 9](https://get.dot.net/9) ou [.NET 10 Preview](https://get.dot.net/10) (optionnel)
- [Docker Desktop](https://docs.docker.com/engine/install/) ou [Podman](https://podman.io/getting-started/installation)
- [Visual Studio 2022](https://visualstudio.microsoft.com/vs/) ou [Visual Studio Code](https://code.visualstudio.com/) avec [C# Dev Kit](https://code.visualstudio.com/docs/csharp/get-started)

Pour une expérience optimale, nous vous recommandons d'utiliser Visual Studio 2022 avec le workload .NET Aspire. Toutefois, vous pouvez utiliser Visual Studio Code avec le C# Dev Kit et le workload .NET Aspire. Vous trouverez ci-dessous des guides de configuration pour chaque plate-forme.

> **Nouveau dans .NET Aspire 9.4** : Support complet pour .NET 10 Preview ! Vous pouvez maintenant créer des projets Aspire ciblant .NET 10 en utilisant `dotnet new aspire --framework net10.0`

## Windows avec Visual Studio

- Installer [Visual Studio 2022 version 17.14 ou plus récent](https://visualstudio.microsoft.com/vs/).
  - Toutes les éditions fonctionnent, y compris la [Visual Studio Community gratuite](https://visualstudio.microsoft.com/free-developer-offers/)
  - Sélectionnez le workload `ASP.NET and web development`.

## Mac, Linux, & Windows sans Visual Studio

- Installer le dernier [SDK .NET 9](https://get.dot.net/9?cid=eshop)

- Installer [Visual Studio Code avec C# Dev Kit](https://code.visualstudio.com/docs/csharp/get-started)

> Remarque: Lors de l'exécution sur Mac avec Apple Silicon (processeur série M), Rosetta 2 pour grpc-tools.

## Installer les derniers modèles .NET Aspire

Exécutez la commande suivante pour installer les modèles .NET Aspire les plus récents.

```cli
dotnet new install Aspire.ProjectTemplates --force
```

## Installer l'interface de ligne de commande .NET Aspire (Optionnel)

.NET Aspire 9.4 introduit l'interface de ligne de commande Aspire généralement disponible, offrant une expérience de développement rationalisée. Vous pouvez l'installer en utilisant l'une de ces méthodes :

### Installation rapide (Recommandée)

```bash
# Windows (PowerShell)
irm https://aspire.dev/install.ps1 | iex

# macOS/Linux (Bash)
curl -sSL https://aspire.dev/install.sh | bash
```

### Outil global .NET

```cli
dotnet tool install -g Aspire.Cli
```

L'interface de ligne de commande Aspire fournit des commandes utiles comme :

- `aspire new` - Créer de nouveaux projets Aspire
- `aspire run` - Trouver et exécuter l'AppHost depuis n'importe où dans votre dépôt
- `aspire add` - Ajouter des packages d'intégration d'hébergement
- `aspire config` - Configurer les paramètres Aspire
- `aspire publish` - Générer des artefacts de déploiement

## Tester l’installation

Pour tester votre installation, consultez [Build your first .NET Aspire project](https://learn.microsoft.com/dotnet/aspire/get-started/build-your-first-aspire-app) pour plus d'informations.

## Ouvrir la solution de démarrage de l'atelier

Pour commencer l'atelier, ouvrez `start/MyWeatherHub.sln` dans Visual Studio 2022. Si vous utilisez Visual Studio Code, ouvrez le dossier `start` et lorsque le C# Dev Kit vous demande quelle solution ouvrir, sélectionnez **MyWeatherHub.sln**.

**Suivant**: [Module #2 - Service Defaults](../Lesson-02-ServiceDefaults/README.md)
