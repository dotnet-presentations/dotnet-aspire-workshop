# .NET Aspireでの統合テスト

## はじめに

このモジュールでは、`MSTest`と`Aspire.Hosting.Testing`を使用した統合テストについて説明します。統合テストは、アプリケーションの異なる部分が期待通りに連携して動作することを確保するために重要です。APIとWebアプリケーションの両方をテストするために、別のテストプロジェクトを作成します。

## 単体テストと統合テストの違い

単体テストは、個々のコンポーネントやコードの単位を分離してテストすることに焦点を当てています。各単位が単独で正しく機能することを確保します。対照的に、統合テストはアプリケーションの異なるコンポーネントが期待通りに連携して動作することを検証します。API、データベース、Webアプリケーションなど、システムの様々な部分間の相互作用をテストします。

.NET Aspireを使用した分散アプリケーションのコンテキストでは、異なるサービスとコンポーネントが正しく通信し、連携して機能することを確保するために統合テストが不可欠です。

## 統合テストプロジェクトの作成

1. `IntegrationTests`という名前の新しいテストプロジェクトを作成します。
1. `IntegrationTests.csproj`ファイルに必要なパッケージへの参照を追加します：

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

このプロジェクトファイルは、テストプロジェクトとしては非常に標準的です。主要な要素は以下の通りです：

- [Aspire.Hosting.Testing](https://www.nuget.org/packages/Aspire.Hosting.Testing) NuGetパッケージへの`PackageReference`。これは.NET Aspireアプリケーションをテストするために必要な型とAPIを提供します。
- AppHostプロジェクトへの`ProjectReference`。これにより、テストプロジェクトがターゲット分散アプリケーション定義にアクセスできます。
- `EnableMSTestRunner`と`OutputType`の設定。これらはテストプロジェクトがネイティブMSTestランナーで実行されるように構成します。

> 注：このワークショップではMSTest 3.xのどのバージョンでも問題ありません。お使いの環境でより新しい3.xが提供されている場合は、それを使用できます。

1. 統合テスト用のテストクラスを作成します：

`IntegrationTests.cs`ファイルはAPIとWebアプリケーションの機能をテストします：

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

このテストクラスは、分散アプリケーションをテストする方法を示しています。これらのテストが何を行っているかを見てみましょう：

- 両方のテストは類似のパターンに従い、`DistributedApplicationTestingBuilder.CreateAsync<Projects.AppHost>()`を使用してアプリケーションホストのインスタンスを非同期的に作成します。
- `appHost`は標準HTTPレジリエンスハンドラーで構成され、より堅牢なHTTP通信のためのリトライポリシーとサーキットブレーカーを提供します。
- テストは`appHost.BuildAsync()`を呼び出してアプリケーションをビルドし、その後DIコンテナから`ResourceNotificationService`を取得します。
- `app.StartAsync()`でアプリケーションを開始した後、テストされるリソース（"api"または"myweatherhub"）専用の`HttpClient`が作成されます。
- テストは対象リソースが"Running"状態に達するまで待機してから処理を続行し、サービスがリクエストを受け入れる準備ができていることを確保します。
- 最後に、特定のエンドポイントにHTTPリクエストが送信され、アサーションがレスポンスを検証します。

最初のテストでは、APIの`/zones`エンドポイントが有効なゾーンデータのコレクションを返すことを検証します。2番目のテストでは、Webアプリケーションのホームページが正常に読み込まれ、期待されるコンテンツが含まれていることを確認します。

`EnvVarTests.cs`ファイルは環境変数の解決を検証します：

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

このテストはサービス発見設定の検証に焦点を当てています：

- `DistributedApplicationTestingBuilder.CreateAsync<Projects.AppHost>()`を使用してアプリケーションホストのインスタンスを作成します。
- アプリケーションを開始する代わりに、Webフロントエンド（"myweatherhub"）を表す`IResourceWithEnvironment`インスタンスを直接取得します。
- `DistributedApplicationOperation.Publish`引数を使用して`GetEnvironmentVariableValuesAsync()`を呼び出し、リソースに公開される環境変数を取得します。
- 最後に、WebフロントエンドがAPIサービスのURLに解決される環境変数を持っていることをアサートし、サービス発見が正しく構成されていることを確認します。

このテストは、アプリケーションのサービスが環境変数を通じて正しく接続されていることを検証するため特に価値があります。これは.NET Aspireが分散アプリケーションでサービス発見を処理する方法です。

> 注：完成版のソリューションに空の`WeatherBackgroundTests.cs`ファイルがある場合、これは将来のバックグラウンドジョブテスト用のプレースホルダーであり、このワークショップでは無視できます。

## 統合テストの実行

### コマンドラインの使用

1. ターミナルを開き、`complete`フォルダに移動します。
1. `dotnet test`コマンドを使用して統合テストを実行します：

```bash
dotnet test IntegrationTests/IntegrationTests.csproj
```

### Visual Studioテストエクスプローラーの使用

1. Visual Studioでソリューションを開きます
1. ビュー > テストエクスプローラーに移動して（またはCtrl+E、Tを押して）テストエクスプローラーを開きます
1. テストエクスプローラーウィンドウで、ソリューション内のすべてのテストが表示されます

![Visual Studioテストエクスプローラー](../media/vs-test-explorer.png)

1. 以下のことができます：
   - 上部の「すべて実行」ボタンをクリックしてすべてのテストを実行
   - 特定のテストを右クリックして「実行」を選択して実行
   - 「失敗したテストを実行」ボタンをクリックして失敗したテストのみを実行
   - 右クリックして「デバッグ」を選択してデバッグモードでテストを実行
   - テストエクスプローラーウィンドウでテスト結果と出力を表示

テストは以下を検証します：

- 環境変数が正しく構成されている
- APIエンドポイントが正しく動作している
- Webアプリケーションが期待通りに機能している

これらのテストを実行すると、すべてのリソースログはデフォルトで`DistributedApplication`にリダイレクトされます。このログリダイレクトにより、リソースが正しくログを出力していることをアサートするシナリオが可能になりますが、これらの特定のテストではそれを行っていません。

## 追加のテストツール

Playwrightはエンドツーエンドテストのための強力なツールです。ブラウザの相互作用を自動化し、アプリケーションがユーザーの視点から期待通りに動作することを検証できます。PlaywrightはChromium、Firefox、WebKitを含む複数のブラウザをサポートしています。

### 使用ケース

PlaywrightはWebアプリケーションのエンドツーエンドテストを実行するために使用できます。ボタンのクリック、フォームの入力、ページ間のナビゲーションなど、ユーザーの相互作用をシミュレートできます。これにより、アプリケーションが実際のシナリオで正しく動作することが保証されます。

### 高レベル概念

- **ブラウザ自動化**：Playwrightはブラウザを起動して制御し、自動化されたテストを実行できます。
- **クロスブラウザテスト**：Playwrightは異なるブラウザでのテストをサポートし、互換性を確保します。
- **ヘッドレスモード**：Playwrightはヘッドレスモードでテストを実行できます。これは、ブラウザがグラフィカルユーザーインターフェースなしでバックグラウンドで実行されることを意味します。
- **アサーション**：Playwrightは、要素が存在し、表示され、期待されるプロパティを持っていることを検証するための組み込みアサーションを提供します。

Playwrightの詳細については、[公式ドキュメント](https://playwright.dev/dotnet/)を参照してください。

## まとめ

このモジュールでは、`MSTest`と`Aspire.Hosting.Testing`を使用した統合テストについて説明しました。APIとWebアプリケーションの両方をテストするために別のテストプロジェクトを作成し、ASP.NET Coreの`WebApplicationFactory`アプローチに似たパターンに従いましたが、分散アプリケーション用に適応させました。

私たちのテストは分散アプリケーションの3つの重要な側面を検証しました：

1. API機能（エンドポイントが期待されるデータを返すことをテスト）
1. Webアプリケーション機能（UIが正しくレンダリングされることをテスト）
1. サービス発見メカニズム（サービスが互いを見つけて通信できることをテスト）

.NET Aspireでのテストについて、ビデオウォークスルーを含む詳しい情報については、ブログ投稿[Getting started with testing and .NET Aspire](https://devblogs.microsoft.com/dotnet/getting-started-with-testing-and-dotnet-aspire/)をご覧ください。

それでは、.NET Aspireを使用する際のデプロイメントオプションについて学びましょう。

**次へ**：[モジュール#9：デプロイメント](../Lesson-09-Deployment/README.md)
