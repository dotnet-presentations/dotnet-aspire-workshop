# 使用 .NET Aspire 进行集成测试

## 介绍

在本模块中，我们将学习如何使用 `Aspire.Hosting.Testing` 与 `MSTest` 进行集成测试。集成测试对于确保应用程序的不同部分能够按预期协同工作至关重要。我们将创建一个单独的测试项目来测试 API 和 Web 应用程序。

## 单元测试与集成测试的区别

单元测试专注于独立测试单个组件或代码单元。它确保每个单元能够独立正常工作。相比之下，集成测试验证应用程序的不同组件能够按预期协同工作。它测试系统各个部分之间的交互，如 API、数据库和 Web 应用程序。

在使用 .NET Aspire 的分布式应用程序环境中，集成测试对于确保不同服务和组件能够正确通信和协同工作至关重要。

## 创建集成测试项目

1. 创建一个名为 `IntegrationTests` 的新测试项目。
1. 在 `IntegrationTests.csproj` 文件中添加所需包的引用：

```xml
<Project Sdk="Microsoft.NET.Sdk">
    <PropertyGroup>
        <TargetFramework>net9.0</TargetFramework>
        <ImplicitUsings>enable</ImplicitUsings>
        <Nullable>enable</Nullable>
        <IsPackable>false</IsPackable>
        <IsTestProject>true</IsTestProject>
    </PropertyGroup>

    <PropertyGroup>
        <EnableMSTestRunner>true</EnableMSTestRunner>
        <OutputType>Exe</OutputType>
    </PropertyGroup>

    <ItemGroup>
        <PackageReference Include="Aspire.Hosting.Testing" Version="9.4.2" />
        <PackageReference Include="MSTest" Version="3.10.4" />
    </ItemGroup>

    <ItemGroup>
        <ProjectReference Include="..\AppHost\AppHost.csproj" />
    </ItemGroup>

    <ItemGroup>
        <Using Include="System.Net" />
        <Using Include="Microsoft.Extensions.DependencyInjection" />
        <Using Include="Aspire.Hosting.ApplicationModel" />
        <Using Include="Aspire.Hosting.Testing" />
        <Using Include="Microsoft.VisualStudio.TestTools.UnitTesting" />
    </ItemGroup>
</Project>
```

这个项目文件对于测试项目来说是相当标准的。关键元素包括：

- 对 [Aspire.Hosting.Testing](https://www.nuget.org/packages/Aspire.Hosting.Testing) NuGet 包的 `PackageReference`，它提供了测试 .NET Aspire 应用程序所需的类型和 API。
- 对 AppHost 项目的 `ProjectReference`，这使测试项目能够访问目标分布式应用程序定义。
- `EnableMSTestRunner` 和 `OutputType` 设置，用于配置测试项目使用原生 MSTest 运行器运行。

> 注意：本研讨会中使用任何 MSTest 3.x 版本都可以。如果您的环境提供了更新的 3.x 版本，您可以使用它。

1. 为集成测试创建测试类：

`IntegrationTests.cs` 文件测试 API 和 Web 应用程序功能：

```csharp
namespace MyWeatherHub.Tests;

[TestClass]
public class IntegrationTests
{
    [TestMethod]
    public async Task TestApiGetZones()
    {
        // Arrange
        var appHost = await DistributedApplicationTestingBuilder
            .CreateAsync<Projects.AppHost>();

        appHost.Services.ConfigureHttpClientDefaults(clientBuilder =>
        {
            clientBuilder.AddStandardResilienceHandler();
        });

        await using var app = await appHost.BuildAsync();

        var resourceNotificationService = app.Services
            .GetRequiredService<ResourceNotificationService>();

        await app.StartAsync();

        // Act
        var httpClient = app.CreateHttpClient("api");

        await resourceNotificationService.WaitForResourceAsync(
                "api",
                KnownResourceStates.Running
            )
            .WaitAsync(TimeSpan.FromSeconds(30));

        var response = await httpClient.GetAsync("/zones");

        // Assert
        response.EnsureSuccessStatusCode();
        var zones = await response.Content.ReadFromJsonAsync<Zone[]>();
        Assert.IsNotNull(zones);
        Assert.IsTrue(zones.Length > 0);
    }

    [TestMethod]
    public async Task TestWebAppHomePage()
    {
        // Arrange
        var appHost = await DistributedApplicationTestingBuilder
            .CreateAsync<Projects.AppHost>();

        appHost.Services.ConfigureHttpClientDefaults(clientBuilder =>
        {
            clientBuilder.AddStandardResilienceHandler();
        });

        await using var app = await appHost.BuildAsync();

        var resourceNotificationService = app.Services
            .GetRequiredService<ResourceNotificationService>();

        await app.StartAsync();

        // Act
        var httpClient = app.CreateHttpClient("myweatherhub");

        await resourceNotificationService.WaitForResourceAsync(
                "myweatherhub",
                KnownResourceStates.Running
            )
            .WaitAsync(TimeSpan.FromSeconds(30));

        var response = await httpClient.GetAsync("/");

        // Assert
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();
        Assert.IsTrue(content.Contains("MyWeatherHub"));
    }
}

public record Zone(string Key, string Name, string State);
```

这个测试类演示了如何测试分布式应用程序。让我们看看这些测试在做什么：

- 两个测试都遵循类似的模式，使用 `DistributedApplicationTestingBuilder.CreateAsync<Projects.AppHost>()` 异步创建应用程序主机的实例。
- `appHost` 配置了标准 HTTP 恢复处理器，为更强健的 HTTP 通信提供重试策略和断路器。
- 测试调用 `appHost.BuildAsync()` 构建应用程序，然后从 DI 容器检索 `ResourceNotificationService`。
- 使用 `app.StartAsync()` 启动应用程序后，为被测试的资源（"api" 或 "myweatherhub"）专门创建 `HttpClient`。
- 测试等待目标资源达到 "Running" 状态后再继续，确保服务准备好接受请求。
- 最后，向特定端点发出 HTTP 请求，断言验证响应。

在第一个测试中，我们验证 API 的 `/zones` 端点返回有效的区域数据集合。在第二个测试中，我们检查 Web 应用程序的主页是否成功加载并包含预期内容。

`EnvVarTests.cs` 文件验证环境变量解析：

```csharp
namespace MyWeatherHub.Tests;

[TestClass]
public class EnvVarTests
{
    [TestMethod]
    public async Task WebResourceEnvVarsResolveToApiService()
    {
        // Arrange
        var appHost = await DistributedApplicationTestingBuilder
            .CreateAsync<Projects.AppHost>();

        var frontend = (IResourceWithEnvironment)appHost.Resources
            .Single(static r => r.Name == "myweatherhub");

        // Act
        var envVars = await frontend.GetEnvironmentVariableValuesAsync(
            DistributedApplicationOperation.Publish);

        // Assert
        CollectionAssert.Contains(envVars,
            new KeyValuePair<string, string>(
                key: "services__api__https__0",
                value: "{api.bindings.https.url}"));
    }
}
```

这个测试专注于验证服务发现配置：

- 它使用 `DistributedApplicationTestingBuilder.CreateAsync<Projects.AppHost>()` 创建应用程序主机的实例。
- 不启动应用程序，而是直接检索表示 Web 前端（"myweatherhub"）的 `IResourceWithEnvironment` 实例。
- 它使用 `DistributedApplicationOperation.Publish` 参数调用 `GetEnvironmentVariableValuesAsync()` 来获取将发布到资源的环境变量。
- 最后，它断言 Web 前端具有解析为 API 服务 URL 的环境变量，确认服务发现配置正确。

这个测试特别有价值，因为它验证了应用程序的服务通过环境变量正确连接，这是 .NET Aspire 在分布式应用程序中处理服务发现的方式。

> 注意：如果您在完整解决方案中看到空的 `WeatherBackgroundTests.cs` 文件，它是未来后台作业测试的占位符，在本研讨会中可以忽略。

## 运行集成测试

### 使用命令行

1. 打开终端并导航到 `complete` 文件夹。
1. 使用 `dotnet test` 命令运行集成测试：

```bash
dotnet test IntegrationTests/IntegrationTests.csproj
```

### 使用 Visual Studio 测试资源管理器

1. 在 Visual Studio 中打开解决方案
1. 通过转到查看 > 测试资源管理器打开测试资源管理器（或按 Ctrl+E, T）
1. 在测试资源管理器窗口中，您将看到解决方案中的所有测试

![Visual Studio 测试资源管理器](../media/vs-test-explorer.png)

1. 您可以：
   - 通过单击顶部的"全部运行"按钮来运行所有测试
   - 通过右键单击特定测试并选择"运行"来运行特定测试
   - 通过单击"运行失败的测试"按钮来仅运行失败的测试
   - 通过右键单击并选择"调试"来在调试模式下运行测试
   - 在测试资源管理器窗口中查看测试结果和输出

测试将验证：

- 环境变量配置正确
- API 端点工作正常
- Web 应用程序按预期运行

运行这些测试时，默认情况下所有资源日志都会重定向到 `DistributedApplication`。这种日志重定向支持您想要断言资源正确记录日志的场景，尽管我们在这些特定测试中没有这样做。

## 附加测试工具

Playwright 是用于端到端测试的强大工具。它允许您自动化浏览器交互并验证应用程序从用户角度按预期工作。Playwright 支持多个浏览器，包括 Chromium、Firefox 和 WebKit。

### 用例

Playwright 可用于执行 Web 应用程序的端到端测试。它可以模拟用户交互，如单击按钮、填写表单和在页面之间导航。这确保您的应用程序在真实场景中正确运行。

### 高级概念

- **浏览器自动化**：Playwright 可以启动和控制浏览器来执行自动化测试。
- **跨浏览器测试**：Playwright 支持跨不同浏览器的测试以确保兼容性。
- **无头模式**：Playwright 可以在无头模式下运行测试，这意味着浏览器在后台运行而没有图形用户界面。
- **断言**：Playwright 提供内置断言来验证元素是否存在、可见并具有预期属性。

有关 Playwright 的更多信息，请参阅[官方文档](https://playwright.dev/dotnet/)。

## 结论

在本模块中，我们介绍了使用 `Aspire.Hosting.Testing` 与 `MSTest` 进行集成测试。我们创建了一个单独的测试项目来测试 API 和 Web 应用程序，遵循类似于 ASP.NET Core 中 `WebApplicationFactory` 方法的模式，但适用于分布式应用程序。

我们的测试验证了分布式应用程序的三个关键方面：

1. API 功能（测试端点返回预期数据）
1. Web 应用程序功能（测试 UI 正确渲染）
1. 服务发现机制（测试服务可以找到并相互通信）

要深入了解使用 .NET Aspire 进行测试（包括视频演练），请查看博客文章[Getting started with testing and .NET Aspire](https://devblogs.microsoft.com/dotnet/getting-started-with-testing-and-dotnet-aspire/)。

现在，让我们了解使用 .NET Aspire 时的部署选项。

**下一步**：[模块 #9：部署](../Lesson-09-Deployment/README.md)
