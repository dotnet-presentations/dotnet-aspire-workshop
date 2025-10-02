# .NET Aspire 統合

.NET Aspire コンポーネントは、Redis や PostgreSQL などの主要なサービスやプラットフォームとのクラウドネイティブアプリケーションの統合を容易にするために特別に選ばれた NuGet パッケージのキュレーションされたスイートです。各コンポーネントは、自動プロビジョニングまたは標準化された構成パターンを通じて、クラウドネイティブの基本的な機能を提供します。.NET Aspire コンポーネントはアプホスト (オーケストレータ) プロジェクトなしでも使用できますが、.NET Aspire AppHost と一緒に使用するのが最適です。

.NET Aspire コンポーネントは、.NET Aspire ホスティングパッケージと混同しないでください。これらは異なる目的を持ちます。ホスティングパッケージは、.NET Aspire アプリ内のさまざまなリソースをモデリングおよび構成するために使用され、コンポーネントはさまざまなクライアントライブラリに構成をマッピングするために使用されます。

Microsoft とコミュニティによって作成および提供されている  [.NET Aspire コンポーネント](https://learn.microsoft.com/dotnet/aspire/fundamentals/components-overview?tabs=dotnet-cli#available-components) のリストはますます増えています。.NET Aspire は柔軟であり、誰でも自分のサービスと統合するための独自のコンポーネントを作成できます。

アプリケーションを改善するためにコンポーネントを追加しましょう。API のパフォーマンスを向上させるために Redis キャッシュに接続するためのコンポーネントを追加します。

## App Host に Redis コンポーネントを追加

アプリケーションに統合できるキャッシングには、以下の 2 種類があります：

- **出力キャッシング**: 将来のリクエストのために完全な HTTP レスポンスを保存するための、構成可能で拡張可能なキャッシング方法。
- **分散キャッシング**: 複数のアプリサーバーで共有されるキャッシュで、特定のデータをキャッシュできます。分散キャッシュは通常、アクセスするアプリサーバーの外部サービスとして維持され、ASP.NET Core アプリのパフォーマンスとスケーラビリティを向上できます。

_出力キャッシング_ コンポーネントを App Host に統合します。このコンポーネントは、API のレスポンスを Redis キャッシュに保存するのに役立ちます。

Redis コンポーネントを App Host に追加するには、`Aspire.Hosting.Redis` NuGet パッケージをインストールする必要があります。このパッケージは、AppHost でサービスを構成するために必要なコンポーネントを提供します。このワークショップでは、Redis はコンテナ イメージを介して提供されており、.NET Aspire App Host を開始すると、自動的に Redis コンテナイメージをダウンロードし、Redis サーバーを開始します。

NuGet をインストールしたら、それを構成できます。

1. `AppHost` プロジェクトの `Program.cs` ファイルを開きます。
1. `var builder = DistributedApplication.CreateBuilder(args);` の下に次のコードを追加します：

 ```csharp
 var cache = builder.AddRedis("cache")
 ```

 ここでは、`cache` という名前で Redis キャッシュを構成しています。この名前は、`Api` や `MyWeatherHub` でキャッシュを識別するために使用されます。

1. App Host 内の `api` を更新してキャッシュへの参照を追加します。

 ```csharp
 var api = builder.AddProject<Projects.Api>("api")
   .WithReference(cache);
 ```

1. さらに、Redis 管理ツールである [Redis Commander](https://joeferner.github.io/redis-commander/) を構成できます。`Aspire.Hosting.Redis` パッケージの一部として、Redis Commander は同じコンポーネント内で利用可能です。Redis 構成に次のコードを追加して Redis Commander を追加します。

 ```csharp
 var cache = builder.AddRedis("cache")
   .WithRedisCommander();
 ```

## アプリケーションの実行（Redis の起動）

`Api` や `MyWeatherHub` プロジェクトには変更を加えていませんが、App Host を開始すると Redis キャッシュが開始されるのを確認できます。

> [!重要]
> Redis はコンテナ内で実行されるため、マシン上で Docker が実行されていることを確認する必要があります。

1. Docker Desktop または Podman を起動します。
1. App Host プロジェクトを起動します。
1. ダッシュボードと Docker Desktop の両方で、Redis コンテナと Redis Commander コンテナのダウンロードと開始を確認できます。

 ![ダッシュボードとデスクトップで実行中の Redis](.../media/redis-started.png)

## API における出力キャッシングの統合

1. `Api` プロジェクトに `Aspire.StackExchange.Redis.OutputCaching` NuGet パッケージをインストールして、Redis API にアクセスできるようにします。
1. `Api` プロジェクトの `Program.cs` ファイルを開きます。
1. ファイルの先頭にある `var builder = WebApplication.CreateBuilder(args);` の下に次のコードを追加します：

 ```csharp
 builder.AddRedisOutputCache("cache");
 ```

 > "cache" という名前を使用して、アプホストで構成した Redis キャッシュを参照しています。

1. `NwsManager` はすでにメモリキャッシュを使用するように構成されていますが、これを Redis キャッシュを使用するように更新します。`Data` フォルダーの `NwsManager.cs` ファイルを開きます。
1. `NwsManagerExtensions` クラスには `AddNwsManager` メソッドがあります。
1. 以下のコードを **削除** します：

 ```csharp
 // Add default output caching
 services.AddOutputCache(options =>
 {
  options.AddBasePolicy(builder => builder.Cache());
 });
 ```

 アプリケーションが `Program.cs` ファイルで Redis キャッシュを使用するように構成されたため、デフォルトの出力キャッシングポリシーを追加する必要はありません。

## アプリケーションの実行（出力キャッシュのテスト）

1. App Host プロジェクトを開始し、ダッシュボードから `MyWeatherHub` プロジェクトを開きます。
1. 都市をクリックし、再度クリックします。レスポンスがキャッシュされていることがわかり、`Traces` タブで 2 回目のリクエストが 1 回目よりもはるかに速いことが確認できます。

 ![アクション中の出力キャッシング](.../media/output-caching.png)

1. Redis Commander でキャッシュされたレスポンスも確認できます。ダッシュボードで `Redis Commander` エンドポイントをクリックして Redis Commander を開きます。`stats` の下に接続と処理されたコマンドが表示されます。
 ![Redis Commander](.../media/redis-commander.png)
1. さらに、`Console` タブで Redis キャッシュと Redis Commander のログを確認できます。

 ![Redis logs](.../media/redis-logs.png)

## カスタム Redis コンテナー

.NET Aspire コンポーネントは柔軟でカスタマイズ可能です。デフォルトでは、Redis コンポーネントは Docker Hub からの Redis コンテナ イメージを使用しますが、自分の Redis コンテナイメージを使用することもできます。`AddRedis` メソッドの後にイメージ名とタグを指定します。たとえば、[Garnet](https://github.com/microsoft/garnet) のようなカスタム Redis コンテナ イメージを持っている場合、次のようにイメージ名とタグを App Host に提供できます：

```csharp
var cache = builder.AddRedis("cache")
 .WithImage("ghcr.io/microsoft/garnet")
 .WithImageTag("latest")
 .WithRedisCommander();
```

1. アプリケーションを実行すると、ダッシュボードと Docker Desktop で Garnet が実行されているのが確認できます。

 ![ダッシュボードとデスクトップで実行中の Garnet](.../media/garnet-started.png)

1. `Console` タブで Garnet のログも確認できます。

 ![Garnet のログ](.../media/garnet-logs.png)

## まとめ

このセクションでは、App Host に Redis コンポーネントを追加し、API に出力キャッシングを統合しました。レスポンスが Redis キャッシュにキャッシュされ、2 回目のリクエストが 1 回目よりも速くなったことを確認しました。また、Redis Commander を使用して Redis キャッシュを管理する方法も確認しました。

統合できるコンポーネントは他にもたくさんあります。[.NET Aspire documentation](https://learn.microsoft.com/dotnet/aspire/fundamentals/components-overview?tabs=dotnet-cli#available-components) で利用可能なコンポーネントのリストを見つけることができます。

次のステップとしては、データベースの統合や Azure Redis Cache をホストされたソリューションとして利用することが考えられます。これらおよびその他のコンポーネントは NuGet で利用可能です。
