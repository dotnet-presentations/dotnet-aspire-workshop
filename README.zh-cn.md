# .NET Aspire 研讨会

学习关于 [.NET Aspire](https://learn.microsoft.com/dotnet/aspire/) 的所有信息，它是用来构建可观察的、生产就绪的分布式应用程序的云就绪技术栈。.NET Aspire 可以支持任何类型的应用程序，无论其大小和规模如何，以帮助您更快地构建更好的应用程序。

本研讨会使用 **.NET Aspire 9.4**，专为 **.NET 9** 设计（也支持 **.NET 8**）。

.NET Aspire 通过以下方式简化了应用程序的开发:

- **编排**: 通过使用 C# 程序设计语言和其熟悉的 API 接口来对你的应用程序进行建模，而无需一行的 YAML 定义文件。轻松添加常用的数据库、消息系统和云服务，将它们连接到您的项目，然后单击一下即可在本地运行
- **服务发现**: 自动注入正确的连接串、网络配置和服务发现信息，以简化开发人员体验。
- **集成**: 常见云服务（如数据库、消息队列和存储）的内置集成。针对日志、运行状况检查、遥测等进行配置。
- **控制面板**: 无需配置即可查看实时 OpenTelemetry 数据。.NET Aspire 的开发人员仪表板默认在运行时启动，显示日志、环境变量、分布式跟踪、指标等，以快速验证应用程序行为。
- **部署**: 轻松生成应用程序在生产环境中运行所需的所有配置的资源清单。（可选）使用 Aspire 感知工具快速轻松地部署到 Azure 容器应用或 Kubernetes。
- **更多**: .NET Aspire 包含开发人员会喜欢的所有功能，并帮助您提高工作效率。

通过以下资源了解有关 .NET Aspire 的更多信息：

- [文档](https://learn.microsoft.com/dotnet/aspire)
- [Microsoft Learn Training Path](https://learn.microsoft.com/training/paths/dotnet-aspire/)
- [.NET Aspire 视频](https://aka.ms/aspire/videos)
- [eShop 参考示例应用程序](https://github.com/dotnet/eshop)
- [.NET Aspire 示例](https://learn.microsoft.com/samples/browse/?expanded=dotnet&products=dotnet-aspire)
- [.NET Aspire FAQ](https://learn.microsoft.com/dotnet/aspire/reference/aspire-faq)

## 本地化

本研讨会提供以下语言的学习材料:

- [English](./README.md)
- [简体中文](./README.zh-cn.md)
- [한국어](./README.ko.md)
- [日本語](./README.jp.md)
- [Español](./README.es.md)
- [Français](./README.fr.md)
- [Português (PT-BR)](./README.pt-br.md)

您还可以观看以下语言的 Let's Learn .NET Aspire 直播活动：

- [English](https://www.youtube.com/watch?v=8i3FaHChh20)
- [한국어](https://www.youtube.com/watch?v=rTpNgMaVM6g)
- [日本語](https://www.youtube.com/watch?v=Cm7mqHZJIgc)
- [Español](https://www.youtube.com/watch?v=dd1Mc5bQZSo)
- [Français](https://www.youtube.com/watch?v=jJiqqVPDN4w)
- [Português (PT-BR)](https://www.youtube.com/watch?v=PUCU9ZOOgQ8)
- [Tiếng Việt](https://www.youtube.com/watch?v=48CWnYfTZhk)

## 研讨会

此 .NET Aspire 研讨会是 [Let's Learn .NET](https://aka.ms/letslearndotnet) 系列的一部分。此研讨会旨在帮助您了解 .NET Aspire 以及如何使用它来构建云就绪应用程序。

### 先决条件

开始此研讨会之前，请确保您具有：

- [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)（推荐）或 [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Visual Studio 2022](https://visualstudio.microsoft.com/vs/) 或带有 C# 扩展的 [Visual Studio Code](https://code.visualstudio.com/)
- [Docker Desktop](https://www.docker.com/products/docker-desktop/)（用于容器化资源）

### 研讨会模块

本研讨会分为 15 个模块（预计完成时间：4-6 小时）：

1. [设置和安装](./workshop/Lesson-01-Setup/README.md)
1. [Service Defaults](./workshop/Lesson-02-ServiceDefaults/README.md)
1. [开发者仪表板和编排](./workshop/Lesson-03-Dashboard-AppHost/README.md)
1. [服务发现](./workshop/Lesson-04-ServiceDiscovery/README.md)
1. [集成](./workshop/Lesson-05-Integrations/README.md)
1. [遥测模块](./workshop/Lesson-06-Telemetry/README.md)
1. [数据库模块](./workshop/Lesson-07-Database/README.md)
1. [集成测试](./workshop/Lesson-08-Integration-Testing/README.md)
1. [部署](./workshop/Lesson-09-Deployment/README.md)
1. [容器管理](./workshop/Lesson-10-Container-Management/README.md)
1. [Azure 集成](./workshop/Lesson-11-Azure-Integrations/README.md)
1. [自定义命令](./workshop/Lesson-12-Custom-Commands/README.md)
1. [健康检查](./workshop/Lesson-13-HealthChecks/README.md)
1. [GitHub Models 集成](./workshop/Lesson-14-GitHub-Models-Integration/README.md)
1. [Docker 集成](./workshop/Lesson-15-Docker-Integration/README.md)

此研讨会的完整[幻灯片](./workshop/AspireWorkshop.pptx)可供使用。

### 入门指南

此研讨会的起始项目位于 `start` 文件夹中。此项目是一个简单的气象 API，它使用国家气象局 API 来获取天气数据，并使用基于 Blazor 的 Web 前端来显示气象数据。

开始研讨会：

1. 导航到 `start` 文件夹
2. 打开解决方案文件 `MyWeatherHub.sln`
3. 按照[模块 1：设置和安装](./workshop/Lesson-01-Setup/README.md)中的说明操作

## 演示数据

本教程使用的数据和服务来自美国国家气象局 (NWS) <https://weather.gov>。我们使用他们的 OpenAPI 规范来查询天气预报。OpenAPI 规范可[在线获取](https://www.weather.gov/documentation/services-web-api)。我们只使用了这个 API 的 2 个方法，并简化了我们的代码，只使用这些方法，而不是为 NWS API 创建整个 OpenAPI 客户端。
