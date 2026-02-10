# Machine Setup

This workshop will be using the following tools:

- [.NET 10 SDK](https://get.dot.net/10) or [.NET 9](https://get.dot.net/9)
- [Docker Desktop](https://docs.docker.com/engine/install/) or [Podman](https://podman.io/getting-started/installation)
- [Visual Studio 2026](https://visualstudio.microsoft.com/vs/) or [Visual Studio Code](https://code.visualstudio.com/) with [C# Dev Kit](https://code.visualstudio.com/docs/csharp/get-started)
- Aspire CLI - this command line tool allows you to update and interact with the Aspire features of your application system without needing to directly use .NET tools

For the best experience, we recommend using Visual Studio 2026 with the ASP.NET Core workload. However, you can use Visual Studio Code with the C# Dev Kit and Aspire CLI. Below are setup guides for each platform.

## Windows with Visual Studio

- Install [Visual Studio 2026](https://visualstudio.microsoft.com/vs/).
  - Any edition will work including the [free to use Visual Studio Community](https://visualstudio.microsoft.com/free-developer-offers/)
  - Select the following `ASP.NET and web development` workload.

## Mac, Linux, & Windows without Visual Studio

- Install the latest [.NET 10 SDK](https://get.dot.net/10?cid=eshop)

- Install [Visual Studio Code with C# Dev Kit](https://code.visualstudio.com/docs/csharp/get-started)

> Note: When running on Mac with Apple Silicon (M series processor), Rosetta 2 for grpc-tools.

## Install Latest Aspire Templates

Run the following command to install the latest Aspire templates.

```cli
dotnet new install Aspire.ProjectTemplates --force
```

## Install the Aspire CLI

Let's install the Aspire CLI, which provides a streamlined developer experience. You can install it using one of these methods:

### Quick Install (Recommended)

```bash
# Windows (PowerShell)
irm https://aspire.dev/install.ps1 | iex

# macOS/Linux (Bash)
curl -sSL https://aspire.dev/install.sh | bash
```

### .NET Global Tool

```cli
dotnet tool install -g Aspire.Cli
```

The Aspire CLI provides useful commands like:

- `aspire new` - Create new Aspire projects
- `aspire run` - Find and run the AppHost from anywhere in your repo
- `aspire add` - Add hosting integration packages
- `aspire config` - Configure Aspire settings
- `aspire publish` - Generate deployment artifacts

## Test Installation

To test your installation, see the [Build your first Aspire project](https://learn.microsoft.com/dotnet/aspire/get-started/build-your-first-aspire-app) for more information.

## Open Workshop Start Solution

To start the workshop open `start/MyWeatherHub.sln` in Visual Studio 2026. If you are using Visual Studio code open the `start` folder and when prompted by the C# Dev Kit which solution to open, select **MyWeatherHub.sln**.

**Next**: [Module #2 - Service Defaults](../Lesson-02-ServiceDefaults/README.md)
