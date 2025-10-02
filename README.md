# .NET Aspire Workshop

Come learn all about [.NET Aspire](https://learn.microsoft.com/dotnet/aspire/), a cloud ready stack for building observable, production ready, distributed applications.​ .NET Aspire can be added to any application regardless of the size and scale to help you build better applications faster.​

This workshop uses **.NET Aspire 9.4** and is designed for **.NET 9** (**.NET 8** is also supported).

.NET Aspire streamlines app development with:

- **Orchestration**: Use C# and familiar APIs to model your distributed application without a line of YAML. Easily add popular databases, messaging systems, and cloud services, connect them to your projects, and run locally with a single click.
- **Service Discovery**: Automatic injection of the right connection strings or network configurations and service discovery information to simplify the developer experience.
- **Integrations**: Built-in integrations for common cloud services like databases, queues, and storage. Configured for logging, health checks, telemetry, and more.
- **Dashboard**: See live OpenTelemetry data with no configuration required. Launched by default on run, .NET Aspire's developer dashboard shows logs, environment variables, distributed traces, metrics and more to quickly verify app behavior.
- **Deployment**: Easily produce a manifest of all the configuration your application resources require to run in production. Optionally, quickly and easily deploy to Azure Container Apps or Kubernetes using Aspire-aware tools.
- **So Much More**: .NET Aspire is packed full of features that developers will love and help you be more productive.

Learn more about .NET Aspire with the following resources:

- [Documentation](https://learn.microsoft.com/dotnet/aspire)
- [Microsoft Learn Training Path](https://learn.microsoft.com/training/paths/dotnet-aspire/)
- [.NET Aspire Videos](https://aka.ms/aspire/videos)
- [eShop Reference Sample App](https://github.com/dotnet/eshop)
- [.NET Aspire Samples](https://learn.microsoft.com/samples/browse/?expanded=dotnet&products=dotnet-aspire)
- [.NET Aspire FAQ](https://learn.microsoft.com/dotnet/aspire/reference/aspire-faq)

## Localization

This workshop materials are available in the following languages:

- [English](./README.md)
- [简体中文](./README.zh-cn.md)
- [한국어](./README.ko.md)
- [日本語](./README.jp.md)
- [Español](./README.es.md)
- [Français](./README.fr.md)
- [Português (PT-BR)](./README.pt-br.md)

You can also watch the Let's Learn .NET Aspire live stream events for the following languages:

- [English](https://www.youtube.com/watch?v=8i3FaHChh20)
- [한국어](https://www.youtube.com/watch?v=rTpNgMaVM6g)
- [日本語](https://www.youtube.com/watch?v=Cm7mqHZJIgc)
- [Español](https://www.youtube.com/watch?v=dd1Mc5bQZSo)
- [Français](https://www.youtube.com/watch?v=jJiqqVPDN4w)
- [Português (PT-BR)](https://www.youtube.com/watch?v=PUCU9ZOOgQ8)
- [Tiếng Việt](https://www.youtube.com/watch?v=48CWnYfTZhk)

## Workshop

This .NET Aspire workshop is part of the [Let's Learn .NET](https://aka.ms/letslearndotnet) series. This workshop is designed to help you learn about .NET Aspire and how to use it to build cloud ready applications.

### Prerequisites

Before starting this workshop, ensure you have:

- [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0) (recommended) or [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [.NET 10 SDK](https://get.dot.net/10) (not required) for some feature previews you may want to try
- [Visual Studio 2022](https://visualstudio.microsoft.com/vs/) or [Visual Studio Code](https://code.visualstudio.com/) with the C# extension
- [Docker Desktop](https://www.docker.com/products/docker-desktop/) (for containerized resources)

### Workshop Modules

This workshop is broken down into 15 modules (estimated completion time: 4-6 hours):

1. [Setup & Installation](./workshop/Lesson-01-Setup/README.md)
1. [Service Defaults](./workshop/Lesson-02-ServiceDefaults/README.md)
1. [Developer Dashboard & Orchestration](./workshop/Lesson-03-Dashboard-AppHost/README.md)
1. [Service Discovery](./workshop/Lesson-04-ServiceDiscovery/README.md)
1. [Integrations](./workshop/Lesson-05-Integrations/README.md)
1. [Telemetry Module](./workshop/Lesson-06-Telemetry/README.md)
1. [Database Module](./workshop/Lesson-07-Database/README.md)
1. [Integration Testing](./workshop/Lesson-08-Integration-Testing/README.md)
1. [Deployment](./workshop/Lesson-09-Deployment/README.md)
1. [Container Management](./workshop/Lesson-10-Container-Management/README.md)
1. [Azure Integrations](./workshop/Lesson-11-Azure-Integrations/README.md)
1. [Custom Commands](./workshop/Lesson-12-Custom-Commands/README.md)
1. [Health Checks](./workshop/Lesson-13-HealthChecks/README.md)
1. [GitHub Models Integration](./workshop/Lesson-14-GitHub-Models-Integration/README.md)
1. [Docker Integration](./workshop/Lesson-15-Docker-Integration/README.md)

A full [slide deck](./workshop/AspireWorkshop.pptx) is available for this workshop.

### Getting Started

The starting project for this workshop is located in the `start` folder. This project is a simple weather API that uses the National Weather Service API to get weather data and a web frontend to display the weather data powered by Blazor.

To begin the workshop:

1. Navigate to the `start` folder
2. Open the solution file `MyWeatherHub.sln`
3. Follow the instructions in [Module 1: Setup & Installation](./workshop/1-setup.md)

## Demo data

The data and service used for this tutorial comes from the United States National Weather Service (NWS) at <https://weather.gov>  We are using their OpenAPI specification to query weather forecasts.  The OpenAPI specification is [available online](https://www.weather.gov/documentation/services-web-api).  We are using only 2 methods of this API, and simplified our code to just use those methods instead of creating the entire OpenAPI client for the NWS API.
