# 服务默认值 Service Defaults（又名智能默认值 ）

## 介绍

.NET Aspire 为 .NET 应用程序中常用的服务提供了一组智能默认值。这些默认值旨在帮助您快速入门，并在不同类型的应用程序中提供一致的体验。其中包括：

- 遥测 Telemetry: Metrics, Tracing, Logging
- 弹性 Resiliency
- 健康检测 Health Checks
- 服务发现 Service Discovery

## 创建 Service Defaults 项目

### Visual Studio & Visual Studio Code

1. 在当前解决方案中添加名为 `ServiceDefaults` 的新项目:
   - 在解决方案上右键，在右键菜单中选择 `Add` > `New Project`.
   - 选择 `.NET Aspire Service Defaults` 项目模板.
   - 命名项目为 `ServiceDefaults`.
   - 点击 `Next` > `Create`.

    *Visual Studio*
    ![Visual Studio dialog to add a service defaults project](../media/vs-add-servicedefaults.png)

    *Visual Studio Code*
    ![Visual Studio Code dialog to add a service defaults project](../media/vsc-add-servicedefaults.png)

### 命令行

1. 在解决方案文件夹中，使用 `dotnet new aspire-servicedefaults` 命令创建新项目:

   ```bash
   dotnet new aspire-servicedefaults -n ServiceDefaults
   ```

## 配置 Service Defaults

1. 在 `Api` 和 `MyWeatherHub` 项目中添加对 `ServiceDefaults` 项目的引用:
   - 在 `Api` 项目上右键，然后在弹出菜单中选择 `Add` > `Reference`.
     - 选择 `ServiceDefaults` 项目，然后点击 `OK`.
   - 在 `MyWeatherHub` 项目上右键，然后在弹出菜单中选择 `Add` > `Reference`.
     - 选择 `ServiceDefaults` 项目，然后点击 `OK`.

   > 高级提示: 在 Visual Studio 2022 中, 你可以通过拖拽项目到其它项目上来添加引用.

2. 在 `Api` 和 `MyWeatherHub` 两个项目中, 更新它们的 `Program.cs` 文件，在紧接其中的 `var builder = WebApplication.CreateBuilder(args);` 行之后，添加如下的代码行：

   ```csharp
   builder.AddServiceDefaults();
   ```

3. 在 `Api` 和 `MyWeatherHub` 两个项目中，, 更新它们的 `Program.cs` 文件，在紧接其中的 `var app = builder.Build();` 行之后，添加如下的代码行：

   ```csharp
   app.MapDefaultEndpoints();
   ```

## 运行

1. 在 Visual Studio 或 Visual Studio Code 中，使用 multiple-project launch configuration 来运行项目:
   - Visual Studio: 在 `MyWeatherHub` 解决方案右键，然后在属性 properties 页面. 选择 `Api` 和 `MyWeatherHub` 项目作为启动项目, 选择 `OK`.
     - ![Visual Studio solution properties](../media/vs-multiproject.png)
     - 点击 `Start` 来启动和调试这两个项目.
   - Visual Studio Code: 使用 `Run and Debug` 面板来运行 `Api` 和 `MyWeatherHub` 项目。我们提供了包含所需配置的 `launch.json` 文件来支持运行这两个项目。
2. 通过导航到以下 URL 来测试应用程序:
   - [https://localhost:7032/openapi/v1.json](https://localhost:7032/openapi/v1.json) - API（OpenAPI 文档）
   - [https://localhost:7274/](https://localhost:7274/) - MyWeatherHub
3. 您应该会看到 API 的 OpenAPI 文档（JSON）和 MyWeatherHub 主页。
4. 您还可以通过导航到 [https://localhost:7032/health](https://localhost:7032/health).
5. 您还可以通过导航到 [https://localhost:7274/health](https://localhost:7274/health).
6. 查看终端中的日志以查看运行状况检查和其他遥测数据，例如 Polly 的弹性：

   ```bash
   Polly: Information: Execution attempt. Source: '-standard//Standard-Retry', Operation Key: '', Result: '200', Handled: 'False', Attempt: '0', Execution Time: '13.0649'
   ```

7. 点击 5 个不同的城市，将抛出一个 “随机” 错误。您将看到 Polly 重试策略正在运行。

   ```bash
   Polly: Warning: Execution attempt. Source: '-standard//Standard-Retry', Operation Key: '', Result: '500', Handled: 'True', Attempt: '0', Execution Time: '9732.8258'
   Polly: Warning: Resilience event occurred. EventName: 'OnRetry', Source: '-standard//Standard-Retry', Operation Key: '', Result: '500'
   System.Net.Http.HttpClient.NwsManager.ClientHandler: Information: Sending HTTP request GET http://localhost:5271/forecast/AKZ318
   ```

**下一节**: [模块 #3 - Dashboard & App Host](../Lesson-03-Dashboard-AppHost/README.md)
