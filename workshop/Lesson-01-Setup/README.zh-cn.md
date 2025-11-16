# 环境设置

本研讨会需要使用如下开发工具:

- [.NET 9 SDK](https://get.dot.net/9) 或者 [.NET 10 Preview](https://get.dot.net/10) (可选)
- [Docker Desktop](https://docs.docker.com/engine/install/) 或者 [Podman](https://podman.io/getting-started/installation)
- [Visual Studio 2022](https://visualstudio.microsoft.com/vs/) 或者 [Visual Studio Code](https://code.visualstudio.com/) 和 [C# Dev Kit](https://code.visualstudio.com/docs/csharp/get-started)

为了获得最佳体验，我们推荐使用将 Visual Studio 2022 与 .NET Aspire 工作负载一起使用。但是，您也可以将 Visual Studio Code 与 C# Dev Kit 和 .NET Aspire 工作负载一起使用。以下是每个平台的设置指南。

> **.NET Aspire 9.4 中的新功能**：完全支持 .NET 10 Preview！您现在可以使用 `dotnet new aspire --framework net10.0` 创建针对 .NET 10 的 Aspire 项目

## Windows 平台上使用 Visual Studio

- 安装 [Visual Studio 2022 version 17.14 或更新的版本](https://visualstudio.microsoft.com/vs/).
  - 所有此版本的发行版都可以，包括 [免费 Visual Studio 社区版](https://visualstudio.microsoft.com/free-developer-offers/)
  - 选择 `ASP.NET and web development` 工作负载.

> 注意: .NET Aspire 8.0 要求额外安装 .NET Aspire 工作负载. [对于 .NET 9, 该工作负载不再需要](https://learn.microsoft.com/en-us/dotnet/aspire/get-started/upgrade-to-aspire-9?pivots=visual-studio) ，它可以被卸载掉。

## Mac, Linux, & Windows 平台下不使用 Visual Studio

- 安装最新版 [.NET 9 SDK](https://get.dot.net/9?cid=eshop)

- 安装 [Visual Studio Code with C# Dev Kit](https://code.visualstudio.com/docs/csharp/get-started)

> 注意: 在装有 Apple Silicon（M 系列处理器）的 Mac 上运行时，还需要 Rosetta 2 for grpc-tools。

## 安装最新的 .NET Aspire 模板

运行以下命令安装最新的 .NET Aspire 模板。

```cli
dotnet new install Aspire.ProjectTemplates --force
```

## 安装 .NET Aspire CLI (可选)

.NET Aspire 9.4 引入了正式可用的 Aspire CLI，提供精简的开发者体验。您可以使用以下方法之一进行安装：

### 快速安装 (推荐)

```bash
# Windows (PowerShell)
irm https://aspire.dev/install.ps1 | iex

# macOS/Linux (Bash)
curl -sSL https://aspire.dev/install.sh | bash
```

### .NET 全局工具

```cli
dotnet tool install -g Aspire.Cli
```

Aspire CLI 提供有用的命令，如：

- `aspire new` - 创建新的 Aspire 项目
- `aspire run` - 从存储库中的任何位置查找并运行 AppHost
- `aspire add` - 添加托管集成包
- `aspire config` - 配置 Aspire 设置
- `aspire publish` - 生成部署工件

## 测试安装

为了测试安装, 请查阅 [构建您的第一个 .NET Aspire 项目](https://learn.microsoft.com/dotnet/aspire/get-started/build-your-first-aspire-app) 以获得更详尽信息.

## 打开研讨会的起始解决方案

在 Visual Studio 2022 中打开 `start/MyWeatherHub.sln` 解决方案，开始研讨会。如果您在使用 Visual Studio code，打开 `start` 文件夹，当 C# Dev Kit 提示打开哪个解决方案时，选择 **MyWeatherHub.sln**。

**下一节**: [模块 #2 - Service Defaults](../Lesson-02-ServiceDefaults/README.md)
