# 将 .NET Aspire 应用部署到 Azure Container Apps

.NET Aspire 针对在容器化环境中运行的应用程序进行了优化。[Azure Container Apps](https://learn.microsoft.com/azure/container-apps/overview) 是一个完全托管的环境，使您能够在无服务器平台上运行微服务和容器化应用程序。本文将指导您创建新的 .NET Aspire 解决方案，并使用 Visual Studio 和 Azure Developer CLI (`azd`) 将其部署到 Microsoft Azure Container Apps。

在此示例中，我们假设您正在部署前面部分中的 MyWeatherHub 应用程序。您可以使用您构建的代码，也可以使用 **complete** 目录中的代码。但是，对于任何 .NET Aspire 应用程序，一般步骤都是相同的。

## 使用 Visual Studio 部署应用

1. 在解决方案资源管理器中，右键单击 **AppHost** 项目并选择 **发布** 以打开 **发布** 对话框。

    > 发布 .NET Aspire 需要当前版本的 `azd` CLI。这应该与 .NET Aspire 工作负载一起安装，但如果您收到 CLI 未安装或不是最新版本的通知，可以按照本教程下一部分的说明进行安装。

1. 选择 **Azure Container Apps for .NET Aspire** 作为发布目标。

    ![发布对话框工作流的屏幕截图。](../media/vs-deploy.png)

1. 在 **AzDev Environment** 步骤中，选择您所需的 **订阅** 和 **位置** 值，然后输入 **环境名称**，如 _aspire-weather_。环境名称决定 Azure Container Apps 环境资源的命名。
1. 选择 **完成** 以创建环境，然后选择 **关闭** 以退出对话框工作流并查看部署环境摘要。
1. 选择 **发布** 以在 Azure 上预配和部署资源。

    > 此过程可能需要几分钟才能完成。Visual Studio 在输出日志中提供有关部署进度的状态更新，您可以通过观看这些更新了解发布的工作原理！您将看到该过程涉及创建资源组、Azure Container Registry、Log Analytics 工作区和 Azure Container Apps 环境。然后将应用部署到 Azure Container Apps 环境。

1. 发布完成后，Visual Studio 在环境屏幕底部显示资源 URL。使用这些链接查看各种已部署的资源。选择 **webfrontend** URL 以打开浏览器到已部署的应用。

    ![已完成的发布过程和已部署资源的屏幕截图。](../media/vs-publish-complete.png)

## 使用 Azure Developer CLI (azd) 部署应用

### 安装 Azure Developer CLI

安装 `azd` 的过程因操作系统而异，但可通过 `winget`、`brew`、`apt` 或直接通过 `curl` 广泛使用。要安装 `azd`，请参阅[安装 Azure Developer CLI](https://learn.microsoft.com/azure/developer/azure-developer-cli/install-azd)。

### .NET Aspire 9.4 中的新功能：aspire deploy 命令

.NET Aspire 9.4 引入了 `aspire deploy` 命令（预览/功能标志），它扩展了发布功能以主动部署到目标环境。此命令提供具有自定义预/后部署逻辑的增强部署工作流。

要启用此功能：

```bash
aspire config set features.deployCommandEnabled true
```

然后您可以使用：

```bash
aspire deploy
```

此命令提供增强的进度报告、更好的错误消息，并支持用于复杂部署场景的自定义部署钩子。

### 初始化模板

> 先决条件：
>
> - 确保您已登录：运行 `azd login` 并选择正确的 Azure 订阅。
> - 从包含 AppHost 的文件夹运行以下命令（对于此存储库，如果您要部署完成的示例，通常是 `complete` 文件夹）。

1. 打开新的终端窗口并 `cd` 到您的 .NET Aspire 项目的根目录。
1. 执行 `azd init` 命令以使用 `azd` 初始化您的项目，这将检查本地目录结构并确定应用类型。

    ```console
    azd init
    ```

    有关 `azd init` 命令的更多信息，请参阅 [azd init](https://learn.microsoft.com/azure/developer/azure-developer-cli/reference#azd-init)。
1. 如果这是您第一次初始化应用，`azd` 会提示您输入环境名称：

    ```console
    初始化要在 Azure 上运行的应用 (azd init)
    
    ? 输入新的环境名称： [? 获取帮助]
    ```

    输入所需的环境名称以继续。有关使用 `azd` 管理环境的更多信息，请参阅 [azd env](https://learn.microsoft.com/azure/developer/azure-developer-cli/reference#azd-env)。
1. 当 `azd` 为您提供两个应用初始化选项时，选择 **使用当前目录中的代码**。

    ```console
    ? 您希望如何初始化您的应用？  [使用箭头移动，键入以筛选]
    > 使用当前目录中的代码
      选择模板
    ```

1. 扫描目录后，`azd` 提示您确认它找到了正确的 .NET Aspire _AppHost_ 项目。选择 **确认并继续初始化我的应用** 选项。

    ```console
    检测到的服务：
    
      .NET (Aspire)
      检测位置：D:\source\repos\letslearn-dotnet-aspire\complete\AppHost\AppHost.csproj
    
    azd 将生成在 Azure 上使用 Azure Container Apps 托管应用所需的文件。
    
    ? 选择一个选项  [使用箭头移动，键入以筛选]
    > 确认并继续初始化我的应用
      取消并退出
    ```

1. `azd` 显示 .NET Aspire 解决方案中的每个项目，并提示您识别要部署的项目，这些项目的 HTTP 入口向所有互联网流量公开。仅选择 `myweatherhub`（使用 ↓ 和空格键），因为您希望 API (`api`) 对 Azure Container Apps 环境保持私有且不公开可用。

    ```console
    ? 选择一个选项 确认并继续初始化我的应用
    默认情况下，服务只能从运行它的 Azure Container Apps 环境内部访问。在此处选择服务还将允许从 Internet 访问它。
    ? 选择要向 Internet 公开的服务  [使用箭头移动，空格选择，<右>全选，<左>全不选，键入以筛选]
          [ ]  api
        > [x]  myweatherhub
    ```

1. 最后，指定环境名称，该名称用于命名在 Azure 中预配的资源并管理不同的环境，如 `dev` 和 `prod`。

    ```console
    生成文件以在 Azure 上运行您的应用：
    
      (✓) 完成：生成 ./azure.yaml
      (✓) 完成：生成 ./next-steps.md
    
    成功：您的应用已为云做好准备！
    您可以通过在此目录中运行 azd up 命令来预配和部署您的应用到 Azure。有关配置应用的更多信息，请参阅 ./next-steps.md
    ```

`azd` 生成多个文件并将它们放入工作目录。这些文件是：

- _azure.yaml_：描述应用的服务，如 .NET Aspire AppHost 项目，并将它们映射到 Azure 资源。
- _.azure/config.json_：配置文件，告知 `azd` 当前活动环境是什么。
- _.azure/aspireazddev/.env_：包含特定于环境的覆盖。
- _.azure/aspireazddev/config.json_：配置文件，告知 `azd` 在此环境中哪些服务应该有公共终结点。

### 部署应用

一旦 `azd` 初始化，预配和部署过程就可以作为单个命令执行，[azd up](https://learn.microsoft.com/azure/developer/azure-developer-cli/reference#azd-up)。

```console
默认情况下，服务只能从运行它的 Azure Container Apps 环境内部访问。在此处选择服务还将允许从 Internet 访问它。
? 选择要向 Internet 公开的服务 webfrontend
? 选择要使用的 Azure 订阅：  1. <您的订阅>
? 选择要使用的 Azure 位置： 1. <您的位置>

打包服务 (azd package)


成功：您的应用程序在不到一秒的时间内为 Azure 打包。

预配 Azure 资源 (azd provision)
预配 Azure 资源可能需要一些时间。

订阅：<您的订阅>
位置：<您的位置>

  您可以在 Azure 门户中查看详细进度：
<部署链接>

  (✓) 完成：资源组：<您的资源组>
  (✓) 完成：Container Registry：<ID>
  (✓) 完成：Log Analytics 工作区：<ID>
  (✓) 完成：Container Apps 环境：<ID>
  (✓) 完成：Container App：<ID>

成功：您的应用程序在 1 分 13 秒内在 Azure 中预配。
您可以在 Azure 门户中的资源组 <您的资源组> 下查看创建的资源：
<资源组概述链接>

部署服务 (azd deploy)

  (✓) 完成：部署服务 api
  - 终结点：<仅内部>

  (✓) 完成：部署服务 myweatherhub
  - 终结点：<您的唯一 myweatherhub 应用>.azurecontainerapps.io/


成功：您的应用程序在 1 分 39 秒内部署到 Azure。
您可以在 Azure 门户中的资源组 <您的资源组> 下查看创建的资源：
<资源组概述链接>

成功：您的 up 工作流在 3 分 50 秒内完成了在 Azure 中的预配和部署。
```

首先，项目将在 `azd package` 阶段打包到容器中，然后是 `azd provision` 阶段，在此阶段将预配应用所需的所有 Azure 资源。

一旦 `provision` 完成，将发生 `azd deploy`。在此阶段，项目被推送为容器到 Azure Container Registry 实例，然后用于创建将托管代码的 Azure Container Apps 的新修订版本。

此时，应用已部署和配置，您可以打开 Azure 门户并浏览资源。

## 清理资源

当您不再需要创建的 Azure 资源时，运行以下 Azure CLI 命令删除资源组。删除资源组也会删除其中包含的资源。

```console
az group delete --name <您的资源组名称>
```

**下一步**：[模块 #10：高级容器管理](../Lesson-10-Container-Management/README.md)
