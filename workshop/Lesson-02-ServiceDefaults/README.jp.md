# Service Defaults / Smart Defaults

## はじめに

.NET Aspire は、.NET アプリケーションで一般的に使用されるサービスのスマートデフォルトを提供します。これらのデフォルトは、迅速に開始し、異なる種類のアプリケーションで一貫した体験を提供するよう設計されています。これには以下が含まれます：

- 可観測性: メトリクス、トレーシング、ロギング
- 回復性
- ヘルスチェック
- サービス ディスカバリー

## Service defaults プロジェクトの作成

### Visual Studio & Visual Studio Code

1. `ServiceDefaults` という名前の新しいプロジェクトをソリューションに追加します：

- ソリューションを右クリックし、追加 > 新しいプロジェクトを選択します。
- `.NET Aspire Service Defaults` プロジェクトテンプレートを選択します。
- プロジェクトに `ServiceDefaults` という名前を付けます。
- `次へ` > `作成` をクリックします。

 *Visual Studio*
 ![Visual Studio での service defaults プロジェクトの追加ダイアログ](.../media/vs-add-servicedefaults.png)

 *Visual Studio Code*
 ![Visual Studio Code での service defaults プロジェクトの追加ダイアログ](.../media/vsc-add-servicedefaults.png)

### コマンドライン

1. `dotnet new aspire-servicedefaults` コマンドを使用して新しいプロジェクトを作成します：

 ```bash
 dotnet new aspire-servicedefaults -n ServiceDefaults
 ```

## Service Defaults の設定

1. `Api` および `MyWeatherHub` プロジェクトに `ServiceDefaults` プロジェクトへの参照を追加します：

- `Api` プロジェクトを右クリックし、`追加` > `参照` を選択します。
- `ServiceDefaults` プロジェクトをチェックし、`OK` をクリックします。
- `MyWeatherHub` プロジェクトを右クリックし、`追加` > `参照` を選択します。
- `ServiceDefaults` プロジェクトをチェックし、`OK` をクリックします。

 > プロのヒント: Visual Studio 2022では、プロジェクトを別のプロジェクトにドラッグ＆ドロップして参照を追加できます。

1. `Api` および `MyWeatherHub` プロジェクトの両方で、`Program.cs` ファイルを更新し、以下の行を `var builder = WebApplication.CreateBuilder(args);` 行の直後に追加します：

 ```csharp
 builder.AddServiceDefaults();
 ```

1. `Api` および `MyWeatherHub` プロジェクトの両方で、`Program.cs` ファイルを更新し、`var app = builder.Build();` 行の直後に以下の行を追加します：

 ```csharp
 app.MapDefaultEndpoints();
 ```

## アプリケーションの実行

1. Visual Studio または Visual Studio Code でマルチプロジェクト起動構成を使用してアプリケーションを実行します。

- Visual Studio: `MyWeatherHub` ソリューションを右クリックしてプロパティに移動し、`Api` と `MyWeatherHub` をスタートアッププロジェクトとして選択し、`OK` をクリックします。
- ![Visual Studio ソリューション プロパティ](.../media/vs-multiproject.png)
- `開始` をクリックして、両方のプロジェクトを開始およびデバッグします。
- Visual Studio Code: `Run and Debug` パネルを使用して `Api` および `MyWeatherHub` プロジェクトを実行します。必要な構成が含まれている `launch.json` ファイルを提供しています。

1. 次の URL に移動してアプリケーションをテストします：

- [https://localhost:7032/openapi/v1.json](https://localhost:7032/openapi/v1.json) - API（OpenAPI ドキュメント）
- [https://localhost:7274/](https://localhost:7274/) - MyWeatherHub

1. API の OpenAPI ドキュメント（JSON）と MyWeatherHub のホームページが表示されるはずです。
1. 次のURLに移動してAPIのヘルスチェックを表示できます：[https://localhost:7032/health](https://localhost:7032/health)
1. 次のURLに移動してMyWeatherHubのヘルスチェックを表示できます：[https://localhost:7274/health](https://localhost:7274/health)
1. ターミナルでログを表示して、ヘルスチェックや Polly を使用したレジリエンスなどのテレメトリデータを確認できます：

 ```bash
 Polly: Information: Execution attempt. Source: '-standard//Standard-Retry', Operation Key: '', Result: '200', Handled: 'False', Attempt: '0', Execution Time: '13.0649'
 ```

1. 5つの異なる都市をクリックすると、「ランダム」エラーが発生します。これにより、Polly のリトライポリシーが機能しているのを確認できます。

 ```bash
 Polly: Warning: Execution attempt. Source: '-standard//Standard-Retry', Operation Key: '', Result: '500', Handled: 'True', Attempt: '0', Execution Time: '9732.8258'
 Polly: Warning: Resilience event occurred. EventName: 'OnRetry', Source: '-standard//Standard-Retry', Operation Key: '', Result: '500'
 System.Net.Http.HttpClient.NwsManager.ClientHandler: Information: Sending HTTP request GET http://localhost:5271/forecast/AKZ318
 ```
