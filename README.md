# .NET Aspire Workshop

Come learn all about [.NET Aspire](https://learn.microsoft.com/dotnet/aspire/), a new cloud ready stack for building observable, production ready, distributed applications.​ .NET Aspire can be added to any application regardless of the size and scale to help you build better applications faster.​

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

This .NET Aspire workshop is part of the [Let's Learn .NET](https://aka.ms/letslearndotnet) series.  This workshop is designed to help you learn about .NET Aspire and how to use it to build cloud ready applications.  This workshop is broken down into 9 modules:

1. [Setup & Installation](./workshop/1-setup.md)
1. [Service Defaults](./workshop/2-servicedefaults.md)
1. [Developer Dashboard & Orchestration](./workshop/3-dashboard-apphost.md)
1. [Service Discovery](./workshop/4-servicediscovery.md)
1. [Integrations](./workshop/5-integrations.md)
1. [Deployment](./workshop/6-deployment.md)
1. [Telemetry Module](./workshop/7-telemetry.md)
1. [Database Module](./workshop/8-database.md)
1. [Integration Testing](./workshop/9-integration-testing.md)

A full slide deck is available for this workshop [here](./workshop/AspireWorkshop.pptx).

The starting project for this workshop is located in the `start-with-api` folder.  This project is a simple weather API that uses the National Weather Service API to get weather data and a web frontend to display the weather data powered by Blazor.

This workshop is designed to be done in a 2 hour time frame.

## Demo data

The data and service used for this tutorial comes from the United States National Weather Service (NWS) at <https://weather.gov>  We are using their OpenAPI specification to query weather forecasts.  The OpenAPI specification is [available online](https://www.weather.gov/documentation/services-web-api).  We are using only 2 methods of this API, and simplified our code to just use those methods instead of creating the entire OpenAPI client for the NWS API.
