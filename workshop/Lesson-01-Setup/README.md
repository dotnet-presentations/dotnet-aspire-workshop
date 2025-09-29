# Machine Setup

This workshop will be using the following tools:

- [.NET 9 SDK](https://get.dot.net/9) or [.NET 10 Preview](https://get.dot.net/10) (optional)
- [Docker Desktop](https://docs.docker.com/engine/install/) or [Podman](https://podman.io/getting-started/installation)
- [Visual Studio 2022](https://visualstudio.microsoft.com/vs/) or [Visual Studio Code](https://code.visualstudio.com/) with [C# Dev Kit](https://code.visualstudio.com/docs/csharp/get-started)

For the best experience, we recommend using Visual Studio 2022 with the .NET Aspire workload. However, you can use Visual Studio Code with the C# Dev Kit and .NET Aspire workload. Below are setup guides for each platform.

> **New in .NET Aspire 9.5**: Full support for .NET 10 Preview! You can now create Aspire projects targeting .NET 10 using `dotnet new aspire --framework net10.0`

## Windows with Visual Studio

- Install [Visual Studio 2022 version 17.14 or newer](https://visualstudio.microsoft.com/vs/).
  - Any edition will work including the [free to use Visual Studio Community](https://visualstudio.microsoft.com/free-developer-offers/)
  - Select the following `ASP.NET and web development` workload.

## Mac, Linux, & Windows without Visual Studio

- Install the latest [.NET 9 SDK](https://get.dot.net/9?cid=eshop)

- Install [Visual Studio Code with C# Dev Kit](https://code.visualstudio.com/docs/csharp/get-started)

> Note: When running on Mac with Apple Silicon (M series processor), Rosetta 2 for grpc-tools.

## Install Latest .NET Aspire Templates

Run the following command to install the latest .NET Aspire templates.

```cli
dotnet new install Aspire.ProjectTemplates --force
```

## Install the .NET Aspire CLI (Optional)

.NET Aspire 9.5 introduces the generally available Aspire CLI, providing a streamlined developer experience. You can install it using one of these methods:

### Quick Install (Recommended)

```bash
# Windows (PowerShell)
iex "& { $(irm https://aspire.dev/install.ps1) }"

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

To test your installation, see the [Build your first .NET Aspire project](https://learn.microsoft.com/dotnet/aspire/get-started/build-your-first-aspire-app) for more information.

## Open Workshop Start Solution

To start the workshop open `start/MyWeatherHub.sln` in Visual Studio 2022. If you are using Visual Studio code open the `start` folder and when prompted by the C# Dev Kit which solution to open, select **MyWeatherHub.sln**.

**Next**: [Module #2 - Service Defaults](../Lesson-02-ServiceDefaults/README.md)
