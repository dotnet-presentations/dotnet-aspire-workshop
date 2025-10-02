# データベース統合

## 概要

このモジュールでは、PostgreSQLデータベースをアプリケーションと統合します。データベースとのやり取りにはEntity Framework Core（EF Core）を使用します。さらに、PostgreSQLデータベースを管理するためにPgAdminを設定します。

## PostgreSQLの設定

.NET Aspireは、`Aspire.Hosting.PostgreSQL`パッケージを通じてPostgreSQLの組み込みサポートを提供します。PostgreSQLを設定するには：

1. AppHostプロジェクトに必要なNuGetパッケージをインストールします：

```xml
<PackageReference Include="Aspire.Hosting.PostgreSQL" Version="9.4.2" />
```

1. AppHostのProgram.csを更新してPostgreSQLを追加します：

```csharp
var postgres = builder.AddPostgres("postgres")
    .WithDataVolume(isReadOnly: false);

var weatherDb = postgres.AddDatabase("weatherdb");
```

`WithDataVolume(isReadOnly: false)`設定により、コンテナの再起動間でデータが保持されます。データはコンテナの外部に存在するDockerボリュームに保存され、コンテナの再起動に耐えることができます。これはワークショップでは任意です—省略しても、サンプルは正常に動作します。実行間でデータが保持されないだけです。

### .NET Aspire 9.4の新機能：強化されたデータベース初期化

.NET Aspire 9.4では、すべてのデータベースプロバイダーに対して改良された`WithInitFiles()`メソッドが導入され、より複雑な`WithInitBindMount()`メソッドを置き換えます：

```csharp
var postgres = builder.AddPostgres("postgres")
    .WithDataVolume(isReadOnly: false)
    .WithInitFiles("./database-init");  // ファイルからの簡単な初期化
```

このメソッドは、すべてのデータベースプロバイダー（PostgreSQL、MySQL、MongoDB、Oracle）で一貫して動作し、より良いエラー処理と簡潔な設定を提供します。`WithInitFiles`の使用はこのワークショップでは任意です。データベース統合はこれなしでも動作します。

適切なアプリケーション起動を確保するため、Webアプリケーションがデータベースを待機するように設定します：

```csharp
var web = builder.AddProject<Projects.MyWeatherHub>("myweatherhub")
    .WithReference(weatherDb)
    .WaitFor(postgres)  // アプリ開始前にデータベースの準備完了を確保
    .WithExternalHttpEndpoints();
```

### データベースへのコンテナ永続化の追加

複数のアプリケーション実行にわたってデータベースコンテナとデータを永続化したい開発シナリオでは、コンテナのライフタイムを設定できます：

```csharp
var postgres = builder.AddPostgres("postgres")
    .WithDataVolume(isReadOnly: false)
    .WithLifetime(ContainerLifetime.Persistent);  // アプリ再起動間でコンテナが永続化

var weatherDb = postgres.AddDatabase("weatherdb");
```

`ContainerLifetime.Persistent`を使用すると、Aspireアプリケーションを停止してもPostgreSQLコンテナは実行を続けます。これは任意であり、モジュールを完了するために必要ではありません。有効にすると：

- **高速起動時間**：後続の実行でPostgreSQLの初期化を待つ必要がありません
- **データ永続性**：アプリケーションセッション間でデータベースデータが保持されます
- **一貫した開発**：データベースは前回のままの状態を保ちます

> **注意**: 永続コンテナは主に開発シナリオで有用です。本番環境のデプロイでは、通常、永続性を自動的に処理するマネージドデータベースサービスを使用します。
>
> **高度なコンテナ機能**: .NET Aspireは、より良い起動調整のための`WithExplicitStart()`や、初期化スクリプトをマウントするための`WithContainerFiles()`など、高度なコンテナ設定もサポートします。これらの機能は、複雑な開発シナリオで必要な場合に、コンテナの動作を細かく制御します。これらの高度な機能について詳しくは、公式ドキュメントの[ボリュームを使用したデータの永続化](https://learn.microsoft.com/dotnet/aspire/fundamentals/persist-data-volumes)と[コンテナリソースのライフサイクル](https://learn.microsoft.com/dotnet/aspire/fundamentals/app-host-overview#container-resource-lifecycle)を参照してください。

## EF CoreとPostgreSQLの統合

1. Webアプリケーションに必要なNuGetパッケージをインストールします：

```xml
<PackageReference Include="Aspire.Npgsql.EntityFrameworkCore.PostgreSQL" Version="9.4.2" />
```

1. DbContextクラスを作成します：

```csharp
public class MyWeatherContext : DbContext
{
    public MyWeatherContext(DbContextOptions<MyWeatherContext> options)
        : base(options)
    {
    }

    public DbSet<Zone> FavoriteZones => Set<Zone>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Zone>()
            .HasKey(z => z.Key);
    }
}
```

1. アプリケーションのProgram.csでDbContextを登録します：

```csharp
builder.AddNpgsqlDbContext<MyWeatherContext>(connectionName: "weatherdb");
```

.NET Aspireが接続文字列の設定を自動的に処理することに注意してください。接続名「weatherdb」は、AppHostプロジェクトで作成したデータベース名と一致します。

1. データベース初期化を設定します：

```csharp
if (app.Environment.IsDevelopment())
{
    using (var scope = app.Services.CreateScope())
    {
        var context = scope.ServiceProvider.GetRequiredService<MyWeatherContext>();
        await context.Database.EnsureCreatedAsync();
    }
}
```

開発環境では、`EnsureCreatedAsync()`を使用してデータベーススキーマを自動的に作成します。本番環境では、代わりに適切なデータベースマイグレーションを使用する必要があります。

## Webアプリの更新

次に、気象ゾーンをお気に入りに追加し、フィルタリングをサポートするようにWebアプリケーションを更新します。これらの変更を段階的に行いましょう：

1. `Home.razor`の上部に、まだ存在しない場合はこれらのEntity Frameworkのusing文を追加してください：

```csharp
@using Microsoft.EntityFrameworkCore
@inject MyWeatherContext DbContext
```

1. お気に入り機能をサポートするために、`@code`ブロックにこれらの新しいプロパティを追加します：

```csharp
bool ShowOnlyFavorites { get; set; }
List<Zone> FavoriteZones { get; set; } = new List<Zone>();
```

1. データベースからお気に入りを読み込むために`OnInitializedAsync`メソッドを更新します。既存のメソッドを見つけて、次のように置き換えます：

```csharp
protected override async Task OnInitializedAsync()
{
    AllZones = (await NwsManager.GetZonesAsync()).ToArray();
    FavoriteZones = await DbContext.FavoriteZones.ToListAsync();
}
```

1. 最後に、お気に入りをデータベースに保存するための`ToggleFavorite`メソッドを追加します。このメソッドを`@code`ブロックに追加してください：

```csharp
private async Task ToggleFavorite(Zone zone)
{
    if (FavoriteZones.Contains(zone))
    {
        FavoriteZones.Remove(zone);
        DbContext.FavoriteZones.Remove(zone);
    }
    else
    {
        FavoriteZones.Add(zone);
        DbContext.FavoriteZones.Add(zone);
    }
    await DbContext.SaveChangesAsync();
}
```

1. `Home.razor`の`@code`ブロックで`zones`プロパティを見つけ、お気に入りフィルターを含むこの更新されたバージョンに置き換えます：

```csharp
IQueryable<Zone> zones
{
    get
    {
        var results = AllZones.AsQueryable();

        if (ShowOnlyFavorites)
        {
            results = results.Where(z => FavoriteZones.Contains(z));
        }

        results = string.IsNullOrEmpty(StateFilter) ? results
                : results.Where(z => z.State == StateFilter.ToUpper());

        results = string.IsNullOrEmpty(NameFilter) ? results
                : results.Where(z => z.Name.Contains(NameFilter, StringComparison.InvariantCultureIgnoreCase));

        return results.OrderBy(z => z.Name);
    }
}
```

1. まず、ゾーンリストをフィルタリングするためのチェックボックスを追加します。`Home.razor`で、`<QuickGrid>`要素の直前にこのコードを追加します：

```csharp
<div class="form-check mb-3">
    <input class="form-check-input" type="checkbox" @bind="ShowOnlyFavorites" id="showFavorites">
    <label class="form-check-label" for="showFavorites">
        お気に入りのみ表示
    </label>
</div>
```

1. 次に、お気に入りの状態を表示する新しい列を追加します。`<QuickGrid>`要素内の既存のState列の後に、この列定義を追加します：

```csharp
<TemplateColumn Title="お気に入り">
    <ChildContent>
        <button @onclick="@(() => ToggleFavorite(context))">
            @if (FavoriteZones.Contains(context))
            {
                <span>&#9733;</span> <!-- お気に入り登録済み -->
            }
            else
            {
                <span>&#9734;</span> <!-- お気に入り未登録 -->
            }
        </button>
    </ChildContent>
</TemplateColumn>
```

## 変更のテスト

次に、お気に入り機能とデータベースの永続性をテストして、変更が正しく動作していることを確認しましょう：

1. アプリケーションを開始します：
   - Visual Studio: AppHostプロジェクトを右クリックして「スタートアッププロジェクトに設定」を選択し、F5を押します
   - VS Code: 実行とデバッグパネル（Ctrl+Shift+D）を開き、ドロップダウンから「Run AppHost」を選択し、実行をクリックします

1. ブラウザでMy Weather Hubアプリケーションを開きます：
   - <https://localhost:7274>に移動します
   - グリッドの上に新しい「お気に入りのみ表示」チェックボックスが表示されることを確認します
   - グリッドの各行のお気に入り列に星のアイコン（☆）があることを確認します

1. お気に入り機能をテストします：
   - 名前フィルターを使って「Philadelphia」を見つけます
   - Philadelphiaの横の空の星（☆）をクリックします - 塗りつぶされた星（★）になるはずです
   - 他のいくつかの都市をお気に入りに追加します（「Manhattan」と「Los Angeles County」を試してください）
   - 「お気に入りのみ表示」チェックボックスをチェックします
   - グリッドがお気に入りの都市のみを表示することを確認します
   - 「お気に入りのみ表示」のチェックを外してすべての都市を再度表示します
   - 塗りつぶされた星（★）をクリックして都市のお気に入りを解除してみます

1. 永続性を確認します：
   - ブラウザウィンドウを閉じます
   - IDEでアプリケーションを停止します（停止ボタンをクリックするかShift+F5を押します）
   - AppHostプロジェクトを再起動します
   - <https://localhost:7274>に戻ります
   - 以下を確認します：
     - お気に入りの都市が塗りつぶされた星（★）を表示している
     - 「お気に入りのみ表示」をチェックすると、保存された都市のみにフィルタリングされる
     - 星の切り替えがお気に入りの追加/削除で正常に機能する

最初からやり直したい場合：

1. アプリケーションを完全に停止します
1. Docker Desktopを開きます
1. ボリュームセクションに移動します
1. PostgreSQLボリュームを見つけて削除します
1. アプリケーションを再起動します - 新しいデータベースが自動的に作成されます

> 注意: `Zone`型は`record`なので、等価性は値によるものです。UIが`FavoriteZones.Contains(context)`をチェックするとき、レコードの値（Key/Name/Stateなど）で比較しており、これはお気に入りの意図された動作です。

## その他のデータオプション

PostgreSQLに加えて、.NET Aspireは他のいくつかのデータベースシステムに対してファーストクラスのサポートを提供します：

### [Azure SQL/SQL Server](https://learn.microsoft.com/en-us/dotnet/aspire/database/sql-server-entity-framework-integration)

.NET AspireのSQL Server統合には、開発用の自動コンテナプロビジョニング、接続文字列管理、ヘルスチェックが含まれます。ローカルSQL Serverコンテナと本番環境のAzure SQL Databaseの両方をサポートします。統合は接続の回復力を自動的に処理し、データベース操作を監視するためのテレメトリを含みます。

### [MySQL](https://learn.microsoft.com/en-us/dotnet/aspire/database/mysql-entity-framework-integration)

.NET AspireのMySQL統合は、コンテナ化された開発環境と本番準備構成を含む、PostgreSQLと同様の機能を提供します。組み込みの接続再試行とヘルス監視が含まれており、開発と本番の両方のシナリオに適しています。

### [MongoDB](https://learn.microsoft.com/en-us/dotnet/aspire/database/mongodb-integration)

NoSQLシナリオでは、AspireのMongoDB統合は接続管理、ヘルスチェック、テレメトリを提供します。スタンドアロンMongoDBインスタンスとレプリカセットの両方をサポートし、ローカル開発用の自動コンテナプロビジョニングを備えています。統合は接続文字列管理を処理し、MongoDB操作に特別に調整された再試行ポリシーを含みます。

### SQLite

SQLiteはコンテナ化を必要としませんが、Aspireは一貫した設定パターンとヘルスチェックを提供します。完全に自己完結型でありながら、他のデータベースプロバイダーと同じ親しみやすい開発体験を提供するため、開発とテストのシナリオで特に有用です。

## コミュニティツールキットデータベース機能

.NET Aspire Community Toolkitは、追加のツールでデータベース機能を拡張します：

### [SQL Database Projects](https://learn.microsoft.com/en-us/dotnet/aspire/community-toolkit/hosting-sql-database-projects)

SQL Database Projects統合により、データベーススキーマをソースコードの一部として含めることができます。開発中にデータベーススキーマを自動的にビルドし、デプロイし、データベース構造がバージョン管理され、一貫してデプロイされることを保証します。これは、データベーススキーマをアプリケーションコードと一緒に維持したいチームに特に有用です。

### [Data API Builder](https://learn.microsoft.com/en-us/dotnet/aspire/community-toolkit/hosting-data-api-builder)

Data API Builder（DAB）は、データベーススキーマからRESTとGraphQLエンドポイントを自動的に生成します。この統合により、追加のコードを書くことなく、モダンAPIを通じてデータを迅速に公開できます。次のような機能が含まれます：

- 自動RESTおよびGraphQLエンドポイント生成
- 組み込み認証と承認
- カスタムポリシーサポート
- GraphQLサブスクリプションによるリアルタイム更新
- データベーススキーマ駆動API設計

## まとめ

このモジュールでは、.NET Aspireのデータベース統合機能を使用して、アプリケーションにPostgreSQLデータベースサポートを追加しました。データアクセスにはEntity Framework Coreを使用し、ローカル開発とクラウドホストされたデータベースの両方で動作するようにアプリケーションを設定しました。

次の自然なステップは、データベース統合が正しく動作することを確認するテストを追加することです。

[モジュール #8: 統合テスト](../Lesson-08-Integration-Testing/README.md)に進んで、.NET Aspireアプリケーションの統合テストの書き方を学びましょう。

**次へ**: [モジュール #8: 統合テスト](../Lesson-08-Integration-Testing/README.md)
