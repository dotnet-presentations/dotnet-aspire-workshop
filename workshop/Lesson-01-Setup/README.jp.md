# マシンのセットアップ

このワークショップでは、以下のツールを使用します:

- [.NET 9 SDK](https://get.dot.net/9) または [.NET 10 Preview](https://get.dot.net/10) (オプション)
- [Docker Desktop](https://docs.docker.com/engine/install/) または [Podman](https://podman.io/getting-started/installation)
- [Visual Studio 2022](https://visualstudio.microsoft.com/vs/) または [Visual Studio Code](https://code.visualstudio.com/) と [C# Dev Kit](https://code.visualstudio.com/docs/csharp/get-started)

最良の体験を得るためには、.NET Aspire workload が用意された Visual Studio 2022 を使用することをお勧めします。ただし、C# Dev Kit と .NET Aspire workload が用意された Visual Studio Code を使用することもできます。以下に各プラットフォームのセットアップガイドを示します。

> **.NET Aspire 9.4 の新機能**: .NET 10 Preview の完全サポート！`dotnet new aspire --framework net10.0` を使用して .NET 10 をターゲットとした Aspire プロジェクトを作成できるようになりました

## Visual Studio を使用した Windows

- [Visual Studio 2022 version 17.14 以降](https://visualstudio.microsoft.com/vs/) をインストールします。
  - [無料の Visual Studio Community](https://visualstudio.microsoft.com/free-developer-offers/) を含む、どのエディションでも動作します
  - `ASP.NET と Web 開発` ワークロードを選択します。

## Mac、Linux、および Visual Studio を使用しない Windows

- 最新の [.NET 9 SDK](https://get.dot.net/9?cid=eshop) をインストールします

- [Visual Studio Code with C# Dev Kit](https://code.visualstudio.com/docs/csharp/get-started) をインストールします

> 注意: Apple Silicon (M シリーズプロセッサ) を搭載した Mac で実行する場合、grpc-tools に Rosetta 2 が必要です。

## 最新の .NET Aspire テンプレートのインストール

以下のコマンドを実行して最新の .NET Aspire テンプレートをインストールします。

```cli
dotnet new install Aspire.ProjectTemplates --force
```

## .NET Aspire CLI のインストール (オプション)

.NET Aspire 9.4 では、一般提供される Aspire CLI が導入され、合理化された開発者体験を提供します。以下の方法のいずれかを使用してインストールできます：

### クイックインストール (推奨)

```bash
# Windows (PowerShell)
irm https://aspire.dev/install.ps1 | iex

# macOS/Linux (Bash)
curl -sSL https://aspire.dev/install.sh | bash
```

### .NET グローバルツール

```cli
dotnet tool install -g Aspire.Cli
```

Aspire CLI は以下のような便利なコマンドを提供します：

- `aspire new` - 新しい Aspire プロジェクトを作成
- `aspire run` - リポジトリ内のどこからでも AppHost を見つけて実行
- `aspire add` - ホスティング統合パッケージを追加
- `aspire config` - Aspire 設定を構成
- `aspire publish` - デプロイメント成果物を生成

## インストールのテスト

インストールをテストするには、[初めての .NET Aspire プロジェクトを構築する](https://learn.microsoft.com/dotnet/aspire/get-started/build-your-first-aspire-app) を参照してください。

## ワークショップの開始ソリューションを開く

ワークショップを開始するには、Visual Studio 2022 で `start/MyWeatherHub.sln` を開きます。Visual Studio Code を使用している場合は、`start` フォルダを開き、C# Dev Kit がどのソリューションを開くかを尋ねたときに、**MyWeatherHub.sln** を選択します。

**次へ**: [モジュール #2 - Service Defaults](../Lesson-02-ServiceDefaults/README.md)
