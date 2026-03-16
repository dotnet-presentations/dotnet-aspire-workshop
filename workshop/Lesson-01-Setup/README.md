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

## Install the Aspire CLI

The Aspire CLI is the primary tool for working with Aspire projects. It provides a streamlined developer experience for creating, running, and managing your Aspire applications. Install it using one of these methods:

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

### Verify and Update

After installing, verify and update to the latest version:

```bash
aspire --version
aspire update --self
```

The Aspire CLI provides useful commands like:

- `aspire new` - Create new Aspire projects from templates
- `aspire run` - Find and run the AppHost from anywhere in your repo
- `aspire add` - Add hosting integration packages interactively
- `aspire config` - Configure Aspire settings
- `aspire update` - Update Aspire packages in your project
- `aspire do` - Execute deployment pipeline steps (build, push, deploy)

## Install Aspire Templates (Optional)

The Aspire CLI handles templates automatically when you use `aspire new`. However, if you prefer using `dotnet new` directly, install the templates:

```cli
dotnet new install Aspire.ProjectTemplates --force
```

## Test Installation

To test your installation, see the [Build your first Aspire project](https://learn.microsoft.com/dotnet/aspire/get-started/build-your-first-aspire-app) for more information.

## Open Workshop Start Solution

To start the workshop open `start/MyWeatherHub.sln` in Visual Studio 2026. If you are using Visual Studio code open the `start` folder and when prompted by the C# Dev Kit which solution to open, select **MyWeatherHub.sln**.

**Next**: [Module #2 - Service Defaults](../Lesson-02-ServiceDefaults/README.md)
