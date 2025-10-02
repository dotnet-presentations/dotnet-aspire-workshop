# .NET Aspire App Host を使用したダッシュボードとオーケストレーター

.NET Aspire は、分散アプリケーション内のリソースと依存関係を表現するための API を提供します。これらの API に加え、いくつかの有力なシナリオを可能にするツールもあります。オーケストレータはローカル開発目的のために設計されています。

続ける前に、.NET Aspire で使用される一般的な用語について考えてみましょう：

* *アプリモデル*: 分散アプリケーションを構成するリソースのコレクション (DistributedApplication) 。より正式な定義については、アプリモデルの定義を参照してください。
* *アプリホスト/オーケストレータープロジェクト*: アプリモデルをオーケストレーションする .NET プロジェクトで、通常、*.AppHost という接尾辞が付けられます。
* *リソース*: アプリケーションの一部を表すリソース。これは、.NET プロジェクト、コンテナ、実行可能ファイル、データベース、キャッシュ、クラウドサービス (ストレージサービスなど) などを含むことができます。
* *リファレンス*: WithReference API を使用して依存関係として表現される、リソース間の接続を定義する参照。詳細については、リソース参照を参照してください。

## アプリホスト プロジェクトの作成

### Visual Studio & Visual Studio Code

1. `AppHost` という新しいプロジェクトをソリューションに追加します：

* ソリューションを右クリックして、`追加` > `新しいプロジェクト` を選択します。
* `.NET Aspire App Host` プロジェクトテンプレートを選択します。
* プロジェクトに `AppHost` という名前を付けます。
* `次へ` > `作成` をクリックします。

 *Visual Studio*
 ![アプリホスト プロジェクトを追加するための Visual Studio ダイアログ](.../media/vs-add-apphost.png)

 *Visual Studio Code*
 ![アプリホスト プロジェクトを追加するための Visual Studio Code ダイアログ](.../media/vsc-add-apphost.png)

### コマンドライン

1. `dotnet new aspire-apphost` コマンドを使用して新しいプロジェクトを作成します：

 ```bash
 dotnet new aspire-apphost -n AppHost
 ```

## サービスデフォルトの設定

1. 新しい `AppHost` プロジェクトに `Api` および `MyWeatherHub` プロジェクトへの参照を追加します：

* `AppHost` プロジェクトを右クリックして、`追加` > `参照` を選択します。
* `Api` および `MyWeatherHub` プロジェクトをチェックして、`OK` をクリックします。

 > プロのヒント: Visual Studio 2022 では、プロジェクトを別のプロジェクトにドラッグ＆ドロップして参照を追加できます。

1. これらの参照が追加されると、ソースジェネレータが必要なコードを自動的に生成して、App Host でプロジェクトを参照します。

## アプリケーションのオーケストレーション

1. `AppHost` プロジェクトで、`Program.cs` ファイルを更新し、以下の行を `var builder = DistributedApplication.CreateBuilder(args);` 行の直後に追加します：

 ```csharp
 var api = builder.AddProject<Projects.Api>("api");

 var web = builder.AddProject<Projects.MyWeatherHub>("myweatherhub");
 ```

## アプリケーションの実行

1. Visual Studio で `AppHost` プロジェクトをスタートアッププロジェクトとして設定します。`AppHost` を右クリックして、`既定のプロジェクト` に設定をクリックします。
1. Visual Studio Code を使用している場合は、`launch.json` を開き、次の内容に置き換えます：

 ```json
 {
        "version": "0.2.0",
        "configurations": [
            {
                "name": "Run AppHost",
                "type": "dotnet",
                "request": "launch",
                "projectPath": "${workspaceFolder}\\AppHost\\AppHost.csproj"
            }
        ]
    }
 ```

1. Visual Studio Code または Visual Studio の `Run and Debug` パネルを使用して App Host を実行します。
1. デフォルトのブラウザで .NET Aspire ダッシュボードが開き、アプリケーションのリソースと依存関係が表示されます。

 ![.NET Aspire ダッシュボード](.../media/dashboard.png)

1. `MyWeatherHub` のエンドポイントである [https://localhost:7274](https://localhost:7274) をクリックし、天気ダッシュボードを開きます。
1. `Api` および `MyWeatherHub` プロジェクトが同じプロセス内で実行されており、設定を使用して以前と同じように通信できることに注意してください。
1. `View Logs` ボタンをクリックして、`Api` および `MyWeatherHub` プロジェクトのログを表示します。
1. `Traces` タブを選択し、API が呼び出されているトレースの `View` を選択します。

 ![.NET Aspire ダッシュボード](.../media/dashboard-trace.png)]

1. `Metrics` タブを探索して、`Api` および `MyWeatherHub` プロジェクトのメトリクスを確認します。

 ![.NET Aspire ダッシュボード](.../media/dashboard-metrics.png)

## エラーの作成

1. ダッシュボードで `Structured` タブを開きます。
1. `Level` を `Error` に設定し、エラーが表示されないことを確認します。
1. `MyWeatherApp` ウェブサイトで異なるいくつかの都市をクリックしてエラーを生成します。通常、5つの異なる都市をクリックするとエラーが発生します。
1. エラーを生成した後、ダッシュボードの `Structured` タブが自動的に更新され、エラーが表示されることを確認します。

 ![.NET Aspire ダッシュボード](.../media/dashboard-error.png)

1. `Trace` または `Details` をクリックして、エラーメッセージとスタックトレースを確認します。
