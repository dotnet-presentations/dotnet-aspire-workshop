# サービスディスカバリー

.NET Aspire には、開発およびテスト時にサービスディスカバリを設定するための機能が含まれています。サービスディスカバリ機能は、.NET Aspire AppHost プロジェクトからアプリケーションモデルに追加された個々のサービスプロジェクトに対して、構成ベースのエンドポイントリゾルバから期待される形式で構成を提供することで機能します。

## サービスディスカバリの設定

現在、`MyWeatherHub` は `Api` に接続するために静的な構成を使用しています。これには以下のような理由で問題があります：

- `Api` サービスのポート番号が変更される可能性があります。
- `Api` サービスの IP アドレスが変更される可能性があります。
- HTTP および HTTPS の設定に対して複数の構成設定が必要になります。
- サービスが追加されると、構成がより複雑になります。

これらの問題に対処するために、.NET Aspire AppHost プロジェクトが提供するサービスディスカバリ機能を使用します。これにより、`MyWeatherHub` サービスが実行時に `Api` サービスをディスカバリできるようになります。

1. `AppHost` プロジェクトの `Program.cs` ファイルを開きます。
1. 以前、`builder.AddProject` メソッドを使用していくつかのプロジェクトを含むオーケストレーションを追加しました。これにより、プロジェクトを参照するために使用できる `IResourceBuild` が返されます。次のコードを更新して、`MyWeatherHub` プロジェクトで `Api` プロジェクトを参照します：

 ```csharp
 var api = builder.AddProject<Projects.Api>("api");

 var web = builder.AddProject<Projects.MyWeatherHub>("myweatherhub")
  .WithReference(api)
  .WithExternalHttpEndpoints();
 ```

1. `WithReference` メソッドは `Api` プロジェクトを参照するために使用されます。これにより、`MyWeatherHub` プロジェクトは実行時にApiプロジェクトをディスカバリできるようになります。
1. 後でこのアプリをデプロイする場合、`WithExternalHttpEndpoints` の呼び出しが必要になります。これにより、外部からアクセスできるようになります。

## MyWeatherHub でのサービスディスカバリの有効化

サービスデフォルトをプロジェクトに追加したときに、自動的にサービスディスカバリシステムに登録されました。これにより、`MyWeatherHub` プロジェクトは既にサービスディスカバリを使用するように構成されています。

一部のサービスは複数の名前付きエンドポイントを公開します。名前付きエンドポイントは、HTTP リクエスト URI のホスト部分にエンドポイント名を指定することで解決できます。形式は `scheme://_endpointName.serviceName` です。例えば、"basket" というサービスが "dashboard" という名前のエンドポイントを公開している場合、URI `scheme+http://_dashboard.basket` を使用してこのエンドポイントを指定できます。例：

```csharp
builder.Services.AddHttpClient<BasketServiceClient>(
 static client => client.BaseAddress = new("https+http://basket"));

builder.Services.AddHttpClient<BasketServiceDashboardClient>(
 static client => client.BaseAddress = new("https+http://_dashboard.basket"));
```

上記の例では、`BasketServiceClient` は `basket` サービスのデフォルトエンドポイントを使用し、`BasketServiceDashboardClient` は `basket` サービスの `dashboard` エンドポイントを使用します。次に、`MyWeatherHub` プロジェクトを更新して、サービスディスカバリを使用して `Api` サービスに接続します。

これは、既存の `WeatherEndpoint` 構成設定 (`appsettings.json`) を更新することで実現できます。これにより、既存のデプロイ済みアプリケーションで .NET Aspire を有効にするときに、既存の構成設定を引き続き使用できます。

1. `MyWeatherHub` プロジェクトの `appsettings.json` ファイルを開きます。

1. `WeatherEndpoint` 構成設定をサービスディスカバリを使用するように更新します：

 ```json
 "WeatherEndpoint": "https+http://api"
 ```

1. `WeatherEndpoint` 構成設定は、サービスディスカバリを使用して `Api` サービスに接続するようになりました。

オプションで、`WeatherEndpoint` 構成設定を使用しないようにURLを更新できます。

1. `MyWeatherHub` プロジェクトの `Program.cs` ファイルを開きます。
1. `WeatherEndpoint` 構成設定をサービスディスカバリを使用するように更新します：

 ```csharp
 builder.Services.AddHttpClient<NwsManager>(
  static client => client.BaseAddress = new("https+http://api"));
 ```

## アプリケーションの実行

1. `F5 キー` を押すか、`デバッグ開始` オプションを選択してアプリケーションを実行します。
1. ダッシュボードのエンドポイントを選択して `MyWeatherApp` を開きます。
1. `MyWeatherHub` アプリがまだ動作しており、サービスディスカバリを使用して `Api` サービスに接続していることを確認します。
1. ダッシュボードで `MyWeatherHub` プロジェクトの `詳細` をクリックします。これにより、App Host からアプリを実行する際に .NET Aspire が構成したすべての設定が表示されます。
1. 目のアイコンをクリックして値を表示し、`services__api_http_0` および `services__api_https_0` が `Api` サービスの正しい値で構成されていることを確認します。

 ![ダッシュボードのサービスディスカバリ設定](.../media/dashboard-servicediscovery.png)

## 結論

これは、サービスディスカバリと .NET Aspire を使用してできることの始まりに過ぎません。アプリケーションが成長し、さらに多くのサービスが追加されるにつれて、サービスディスカバリを使用して実行時にサービスを接続し続けることができます。これにより、アプリケーションを簡単にスケールアップし、環境の変化に対してよりレジリエントになります。

## 詳細情報

サービスディスカバリの高度な使用法と構成については、[.NET Aspire Service Discovery](https://learn.microsoft.com/dotnet/aspire/service-discovery/overview) ドキュメントをご覧ください。
