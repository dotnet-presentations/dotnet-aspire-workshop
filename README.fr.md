# Apprenons .NET Aspire

Venez apprendre tout à propos de [.NET Aspire](https://learn.microsoft.com/dotnet/aspire/), une pile prête pour le cloud pour créer des applications distribuées observables, prêtes pour la production.​ .NET Aspire peut être ajouté à n'importe quelle application, quelles que soient sa taille et sa portée, pour vous aider à créer de meilleures applications plus rapidement.​

Cet atelier utilise **.NET Aspire 9.4** et est conçu pour **.NET 9** (**.NET 8** est également pris en charge).

.NET Aspire simplifie le développement d'applications avec :

- **Orchestration** : Orchestration intégrée avec un moteur de workflow simple mais puissant. Utilisez C# et des API familières sans une seule ligne de YAML. Ajoutez facilement des services cloud populaires, connectez-les à vos projets et exécutez-les localement en un seul clic.
- **Découverte de services** : Injection automatique des chaînes de connexion ou des configurations réseau appropriées et des informations de découverte de services pour simplifier l'expérience du développeur.
- **Composants** : Composants intégrés pour les services cloud courants tels que les bases de données, les files d'attente et le stockage. Intégré à la journalisation, aux contrôles de santé, à la télémétrie, etc.
- **Tableau de bord** : Consultez les données OpenTelemetry en direct sans aucune configuration requise. Lancé par défaut lors de l'exécution, le tableau de bord du développeur de .NET Aspire affiche les journaux, les variables d'environnement, les traces distribuées, les métriques et bien plus encore pour vérifier rapidement le comportement de l'application.
- **Déploiement** : Gère l'injection des bonnes chaînes de connexion ou configurations réseau et des informations de découverte de services pour simplifier l'expérience du développeur.
- **Bien plus encore** : .NET Aspire regorge de fonctionnalités que les développeurs adoreront et vous aideront à être plus productif.

Apprenez-en davantage sur .NET Aspire avec les ressources suivantes :

- [Documentation](https://learn.microsoft.com/dotnet/aspire)
- [Parcours de formation Microsoft Learn](https://learn.microsoft.com/en-us/training/paths/dotnet-aspire/)
- [Vidéos .NET Aspire](https://aka.ms/aspire/videos)
- [Exemple d'application de boutique en ligne](https://github.com/dotnet/eshop)
- [Plusieurs exemple .NET Aspire](https://learn.microsoft.com/samples/browse/?expanded=dotnet&products=dotnet-aspire)
- [FAQ .NET Aspire](https://learn.microsoft.com/dotnet/aspire/reference/aspire-faq)

## Localisation

Le matériel de cet atelier est disponible dans les langues suivantes :

- [Anglais](./README.md)
- [한국어](./README.ko.md)
- [日本語](./README.jp.md)
- [Espagnol](./README.es.md)
- [Français](./README.fr.md)
- [Português (PT-BR)](./README.pt-br.md)

Vous pouvez également regarder les événements de en direct (ou sur demande si l'évènement est passé) **Let's Learn .NET Aspire** pour les langues suivantes :

- [anglais](https://www.youtube.com/watch?v=8i3FaHChh20)
- [한국어](https://www.youtube.com/watch?v=rTpNgMaVM6g)
- [日本語](https://www.youtube.com/watch?v=Cm7mqHZJIgc)
- [Espagnol](https://www.youtube.com/watch?v=dd1Mc5bQZSo)
- [Français](https://www.youtube.com/watch?v=jJiqqVPDN4w)
- [Português (PT-BR)](https://www.youtube.com/watch?v=PUCU9ZOOgQ8)
- [Tiếng Việt](https://www.youtube.com/watch?v=48CWnYfTZhk)

## Atelier (Workshop)

Cet atelier .NET Aspire fait partie de la série [Let's Learn .NET](https://aka.ms/letslearndotnet). Cet atelier est conçu pour vous aider à en savoir plus sur .NET Aspire et comment l'utiliser pour créer des applications prêtes pour le cloud.

### Prérequis

Avant de commencer cet atelier, assurez-vous d'avoir :

- [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0) (recommandé) ou [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Visual Studio 2022](https://visualstudio.microsoft.com/vs/) ou [Visual Studio Code](https://code.visualstudio.com/) avec l'extension C#
- [Docker Desktop](https://www.docker.com/products/docker-desktop/) (pour les ressources conteneurisées)

### Modules de l'atelier

Cet atelier se décompose en 15 modules (temps de réalisation estimé : 4-6 heures) :

1. [Configuration et installation](./workshop/Lesson-01-Setup/README.md)
1. [Paramètres par défaut](./workshop/Lesson-02-ServiceDefaults/README.md)
1. [Tableau de bord et orchestration](./workshop/Lesson-03-Dashboard-AppHost/README.md)
1. [Découverte de services](./workshop/Lesson-04-ServiceDiscovery/README.md)
1. [Intégrations](./workshop/Lesson-05-Integrations/README.md)
1. [Module de télémétrie](./workshop/Lesson-06-Telemetry/README.md)
1. [Module de base de données](./workshop/Lesson-07-Database/README.md)
1. [Tests d'intégration](./workshop/Lesson-08-Integration-Testing/README.md)
1. [Déploiement](./workshop/Lesson-09-Deployment/README.md)
1. [Gestion des conteneurs](./workshop/Lesson-10-Container-Management/README.md)
1. [Intégrations Azure](./workshop/Lesson-11-Azure-Integrations/README.md)
1. [Commandes personnalisées](./workshop/Lesson-12-Custom-Commands/README.md)
1. [Vérifications de santé](./workshop/Lesson-13-HealthChecks/README.md)
1. [Intégration GitHub Models](./workshop/Lesson-14-GitHub-Models-Integration/README.md)
1. [Intégration Docker](./workshop/Lesson-15-Docker-Integration/README.md)

Un [diaporama](./workshop/AspireWorkshop.pptx) complet est disponible pour cet atelier.

### Commencer

Le projet de départ de cet atelier se trouve dans le dossier `start`. Ce projet est une API météo simple qui utilise l'API du National Weather Service pour obtenir des données météorologiques et une interface Web pour afficher les données météorologiques alimentées par Blazor.

Pour commencer l'atelier :

1. Naviguez vers le dossier `start`
2. Ouvrez le fichier de solution `MyWeatherHub.sln`
3. Suivez les instructions dans [Module 1 : Configuration et installation](./workshop/Lesson-01-Setup/README.md)

## Données de démonstration

Les données et le service utilisés pour ce didacticiel proviennent du National Weather Service (NWS) des États-Unis à <https://weather.gov>. Nous utilisons leur spécification OpenAPI pour interroger les prévisions météorologiques. La spécification OpenAPI est [disponible en ligne](https://www.weather.gov/documentation/services-web-api). Nous n'utilisons que 2 méthodes de cette API et avons simplifié notre code pour utiliser uniquement ces méthodes au lieu de créer l'intégralité du client OpenAPI pour l'API NWS.
