# 服务发现 Service Discovery

.NET Aspire 包括在开发和测试阶段配置服务发现的功能。该特性允许按其名称引用其他资源，并在运行时解析具体的地址详细信息。服务发现功能的工作原理是，以基于配置的端点来解析程序所需的格式提供配置，所支持范围从 .NET Aspire App Host 项目到添加到应用程序模型的各个服务项目。

## 服务发现配置 Service Discovery Configuration

现在，项目 `MyWeatherHub` 还是使用定义在项目中的 *appsettings.json* 配置文件中的静态配置来连接到 `Api` 服务。对于多种场景来说，这并不理想：

- `Api` 服务的端口可能发生变化
- `Api` 服务的 IP 地址可能发生变化
- 需要针对 http 和 https 协议设置定义多个配置
- 随着我们增加更多的服务，配置将变得更加复杂

为了解决这些问题，我们将使用 .NET Aspire App Host 项目提供的服务发现功能。这将支持 `MyWeatherHub` 服务在运行时发现 `Api` 服务。

1. 打开 `AppHost` 项目中的 `Program.cs` 文件
2. 在前面为了支持编排，我们已经使用 `builder.AddProject` 方法包含了多个项目，它返回一个 `IResourceBuilder<TResource>` 结果，可以被用来引用其它资源。让我们更新代码来使得 `MyWeatherHub` 项目引用 `Api` 项目:

    ```csharp
    var api = builder.AddProject<Projects.Api>("api");

    var web = builder.AddProject<Projects.MyWeatherHub>("myweatherhub")
        .WithReference(api)
        .WithExternalHttpEndpoints();
    ```

3. 其中的 `WithReference` 方法用来引用到 `Api` 项目。这将注入必要的配置到项目中，以支持 `MyWeatherHub` 项目在运行时通过它的资源名称 `api` 发现 `Api` 项目。
4. 如果以后你需要发布该应用，你还需要调用 `WithExternalHttpEndpoints` 方法来确保它对外开放。

## 在 MyWeatherHub 项目中启用服务发现

当我们将 `ServiceDefaults` 项目添加到项目之后，我们会自动将它们注册到服务发现系统中。这意味着 `MyWeatherHub` 项目已经被配置为使用服务发现。

有些服务会暴露多个命名端点，在默认情况下，URI 中的 scheme 部分被用于引用正在解析的端点名称，例如，对于 URI 为 `https://basket` 来说，它将被解析为命名为在 `basket` 服务之上的 `https` 端点。在 Aspire 的默认情况下，项目资源根据它们的  *launchSettings.json*  文件的内容定义其端点，所以，大多数的项目默认接受 `https` 和 `http` 命名的端点。引用 URI 的 scheme 部分可以包含多个通过 `+` 字符分隔的名称，并按照优先次序排列。例如，对于 `https+http://basket` 来说，将会尝试首先解析 `basket` 服务的 `https` 命名端点，如果没有找到，它将解析 `http` 端点。

对于命名端点与预期方案不匹配的情况，也可以通过在请求 URI 的主机部分的第一个子域部分中指定端点名称来显式解析它们，当第一部分以下划线 (`_`) 为前缀时，格式为 'scheme://_endpointName.serviceName' 。例如，如果名为 `basket` 的服务公开了名为 `dashboard` 的 HTTPS 端点，则可以使用 URI 为 `https+http://_dashboard.basket` 来指定此端点，例如：

```csharp
builder.Services.AddHttpClient<BasketServiceClient>(
    static client => client.BaseAddress = new("https+http://basket"));

builder.Services.AddHttpClient<BasketServiceDashboardClient>(
    static client => client.BaseAddress = new("https+http://_dashboard.basket"));
```

在上面的示例中， `BasketServiceClient` 将为 `basket` 服务使用 `https` 和 `http` 端点, 而对于 `BasketServiceDashboardClient` 来说，它将为 `basket` 服务使用 `dashboard` 端点, 而不管是通过 HTTPS 还是 HTTP schemes, 取决于哪一个可用。

现在，让我们更新 `MyWeatherHub` 项目来使用服务发现连接到 `Api` 服务。这可以通过更新现有的位于 `appsettings.json` 配置文件中的 `WeatherEndpoint` 配置来实现。在现有已部署的应用程序中启用 .NET Aspire 非常方便，因为您可以继续使用现有的配置设置。

1. 打开 `MyWeatherHub` 项目中的 `appsettings.json` 配置文件。
2. 更新 `WeatherEndpoint` 配置设置以使用服务发现:

    ```json
    "WeatherEndpoint": "https+http://api"
    ```

3. 现在 `WeatherEndpoint` 配置设置已经在使用服务发现连接到 `Api` 服务了

或者，我们不使用 `WeatherEndpoint` 配置设置，而是通过更新使用的 URL 来实现。直接在 ：

1. 打开 `MyWeatherHub` 项目中的 `Program.cs` 代码文件。
2. 将使用 `WeatherEndpoint` 配置设置更改为使用基于 URL 的服务发现:

    ```csharp
    builder.Services.AddHttpClient<NwsManager>(
        static client => client.BaseAddress = new("https+http://api"));
    ```

## 运行应用

1. 通过 `F5` 快捷键或者选择 `Start Debugging`  运行应用程序。
2. 在仪表板中通过选择端点来打开 `MyWeatherApp` .
3. 注意到 `MyWeatherHub` 应用同以前一样工作，现在它使用服务发现连接到 `Api` 服务.
4. 在仪表板中点击 `MyWeatherHub` 项目的 `Details`. 这将调出 .NET Aspire 在从 App Host 运行应用程序时配置的所有设置。
5. 点击其中的眼睛图标来查看配置值，卷绕到底部，你将看到对于 `Api` 服务的 `services__api__http__0` 和 `services__api__https__0` 配置了正确的值。

    ![Service discovery settings in the dashboard](../media/dashboard-servicediscovery.png)

## 结论

这只是我们可以使用服务发现和 .NET Aspire 完成的开始。随着我们的应用程序增长和添加更多服务，我们可以继续使用服务发现在运行时连接服务。这将使我们能够轻松扩展我们的应用程序，并使其对环境变化更具弹性。

## 进一步学习

您可以通过 [.NET Aspire Service Discovery](https://learn.microsoft.com/dotnet/aspire/service-discovery/overview) 文档进一步学习服务发现的高级用法和配置。

**下一节**: [模块 #5: Integrations](../Lesson-05-Integrations/README.md)
