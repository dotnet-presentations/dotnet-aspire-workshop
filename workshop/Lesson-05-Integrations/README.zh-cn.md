# .NET Aspire 集成

.NET Aspire 集成是一套精心挑选的 NuGet 包，专门用于促进云原生应用程序与主要服务和平台（如 Redis、PostgreSQL 等）的集成。每个集成都通过自动配置或标准化配置模式提供基本的云原生功能。

.NET Aspire 集成有两种不同的风格：**托管集成**和 **客户端集成**。托管集成用于对 .NET Aspire 应用程序中的各种资源进行建模和配置，而客户端集成用于将配置映射到各种客户端库。

由 Microsoft 和社区创建和发布的列表不断增长，参见 [.NET Aspire integrations](https://learn.microsoft.com/dotnet/aspire/fundamentals/integrations-overview?tabs=dotnet-cli#available-integrations)。.NET Aspire 非常灵活，任何人都可以创建自己的包以与他们自己的服务集成。

让我们通过向应用程序添加集成来改进我们的应用程序。我们将添加一个集成，这将帮助我们连接到 Redis 缓存以提高我们的 API 性能。

## 添加 Redis Integration 到 App Host

要将 Redis 托管集成添加到我们的 App Host，我们需要安装 `Aspire.Hosting.Redis` NuGet 包。此包提供了在 App Host 中配置服务所需的部分。在我们的研讨会中，Redis 通过容器映像提供，当我们启动 .NET Aspire 的 App Host 时，它将自动下载 Redis 容器映像并启动 Redis 服务器。

安装 NuGet 包后，我们可以将 Redis 添加到我们的 App Host：

1. 打开 `AppHost` 项目中的 `Program.cs` 代码文件。
2. 在代码行 `var builder = DistributedApplication.CreateBuilder(args);` 之下添加如下代码：

    ```csharp
    var cache = builder.AddRedis("cache");
    ```

    这段代码使用名称 `cache` 来配置 Redis 缓存. 该名称在 `Api` 或 `MyWeatherHub` 项目中用于识别缓存.
3. 更新 App Host 中的  `api` 资源引用此缓存.

    ```csharp
    var api = builder.AddProject<Projects.Api>("api")
                     .WithReference(cache);
    ```

4. 此外，我们可以配置 Redis 管理工具 [Redis Commander](https://joeferner.github.io/redis-commander/)。作为 `Aspire.Hosting.Redis` 包的一部分，Redis Commander 可在同一集成中使用。要添加 Redis Commander，请更新添加 Redis 资源的代码，以在返回的构建器上调用 `WithRedisCommander()` 方法：

    ```csharp
    var cache = builder.AddRedis("cache")
                       .WithRedisCommander();
    ```

## 运行应用（启动 Redis）

我们没有对 `Api` 或 `MyWeatherHub` 项目进行任何更改，但我们可以看到在启动 App Host 时 Redis 缓存启动。

> [!IMPORTANT]
> 由于 Redis 在容器中运行，因此您需要确保 Docker 正在您的计算机上运行.

1. 确保 Docker Desktop 或 Podman 正在运行。
2. 启动 App Host 项目。
3. 您将在仪表板和 Docker Desktop 中看到 Redis 容器和 Redis Commander 容器的下载和启动。

    ![Redis running in dashboard and desktop](../media/redis-started.png)

## 在 API 项目中集成 Output Caching

我们可以将两种类型的缓存集成到 ASP.NET Core 应用程序中：

- **Output caching**: 一种可配置的可扩展缓存方法，用于存储整个 HTTP 响应以供将来请求使用。
- **Distributed caching**: 由多个应用服务器共享的缓存，允许您缓存特定数据。分布式缓存通常作为访问它的应用程序服务器的外部服务进行维护，可以提高 ASP.NET Core 应用程序的性能和可扩展性。

我们会将 _Output caching_ 类型的Redis 客户端集成添加到我们的 `Api` 项目中。此集成将帮助我们在 Redis 缓存中缓存 API 的响应。

1. 在 `Api` 项目中安装 `Aspire.StackExchange.Redis.OutputCaching` NuGet package 以访问 Redis APIs.
2. 打开 `Api` 项目中的 `Program.cs` 代码文件。
3. 在代码文件开始部分的代码行 `var builder = WebApplication.CreateBuilder(args);` 之后添加如下代码：

    ```csharp
    builder.AddRedisOutputCache("cache");
    ```

    > 请注意，我们使用 `cache` 名称来引用我们在 App Host 中配置的 Redis 缓存。
4. 项目中的 `NwsManager` 已经配置为使用输出缓存，但使用的是内存缓存。我们将更新它以使用 Redis 缓存。打开 `Data` 文件夹中的 `NwsManager.cs` 文件。
5. 在 `NwsManagerExtensions` 类中找到 `AddNwsManager()` 方法.
6. **DELETE** 如下代码:

    ```csharp
    // Add default output caching
    services.AddOutputCache(options =>
    {
        options.AddBasePolicy(builder => builder.Cache());
    });
    ```

    由于我们将应用程序配置为使用`Program.cs` 文件中的 Redis 缓存，因此我们不再需要添加默认输出缓存策略。

## 运行更新之后的应用

1. 启动 App Host 项目，然后通过仪表板来打开 `MyWeatherHub` 项目。
2. 点击页面上的某个城市，然后再次点击它。你会看到响应是被缓存的，通过 `Traces` 可以看到第二个请求比第一个请求要快得多。

    ![Output caching in action](../media/output-caching.png)

3. 你还可以通过 Redis Commander 查看缓存的响应。通过点击仪表板中的 `cache-commander` 端点来访问，在指标 stats 的下面，你可以看到处理的连接和命令统计。

    ![Redis Commander](../media/redis-commander.png)

4. In addition, you can see logs for the Redis cache and Redis Commander in the `Console` tab.

    ![Redis logs](../media/redis-logs.png)

## 订制 Redis 容器

.NET Aspire 集成非常灵活且可订制。默认情况下，Redis 集成使用来自 Docker Hub 的 Redis 容器镜像。但是，您可以通过在 `AddRedis()` 方法后提供映像名称和标签来使用自己的 Redis 容器映像。例如，如果您有一个自定义的 Redis 容器镜像，例如 [Garnet](https://github.com/microsoft/garnet)，您可以在 App Host 中提供镜像注册表、名称和标签，如下所示：

```csharp
var cache = builder.AddRedis("cache")
    .WithImageRegistry("ghcr.io")
    .WithImage("microsoft/garnet")
    .WithImageTag("latest");
```

> 注意: 现在有新的 [Garnet integration](https://learn.microsoft.com/dotnet/aspire/caching/stackexchange-redis-integration?pivots=garnet&tabs=dotnet-cli)  来简化这种集成，上面的示例强调了集成的灵活性。

1. 重新运行应用，你可以通过仪表板看到 Garnet 在运行，也可以在 Docker Desktop 中看到 Garnet 在运行。

    ![Garnet running in dashboard and desktop](../media/garnet-started.png)

2. 还可以 `Console` 页面中看到 Garnet 的日志信息。

    ![Garnet logs](../media/garnet-logs.png)

## 总结

在本节中，我们在 API 中添加了对于 Redis 的托管集成到 App Host 中，以及在客户端集成了 Redis 的 Output caching 支持。我们了解了响应是如何缓存在 Redis 缓存中的，以及第二个和后续请求如何比第一个请求要快得多。我们还了解了如何使用 Redis Commander 来管理 Redis 缓存。

还有更多 Aspire 集成可用于与您的服务集成。您可以找到可用集成的列表，[它们位于 .NET Aspire 文档](https://learn.microsoft.com/dotnet/aspire/fundamentals/integrations-overview?tabs=dotnet-cli#available-integrations).

下一步自然是集成数据库或将 Azure Redis 缓存用作托管解决方案。这些以及更多的集成 [在 NuGet 中可用](https://www.nuget.org/packages?q=owner%3Aaspire+tags%3Aintegration).

**下一节**: [模块 #6: Deployment](6-deployment.md)
