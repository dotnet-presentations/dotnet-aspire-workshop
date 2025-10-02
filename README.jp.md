# .NET Aspire Workshop

[.NET Aspire](https://learn.microsoft.com/dotnet/aspire/) について学びましょう。.NET Aspire はサイズやスケールに関係なく、あらゆるアプリケーションに追加でき、より優れたアプリケーションを迅速に構築出来ます。

このワークショップは **.NET Aspire 9.4** を使用し、**.NET 9** 向けに設計されています（**.NET 8** もサポートされています）。

.NET Aspire はアプリケーションの開発を効率化します。

- **オーケストレーション**: シンプルで協力なワークフロによる組み込みオーケストレーション。C# と慣れ親しんだ API を使用し、YAML を 1 行も書くことなく利用できます。人気のクラウドサービスを簡単に追加し、プロジェクトに接続して、ワンクリックでローカルで実行できます。
- **サービスディスカバリー**: 適切な接続文字列やネットワーク構成、サービスディスカバリー情報を自動的に挿入して、開発者の体験を簡素化します。
- **コンポーネント**: データベース、キュー、ストレージなどの一般的なクラウドサービス向けの組み込みコンポーネント。ロギング、ヘルスチェック、テレメトリなどと統合されています。
- **ダッシュボード**: 設定不要でリアルタイムの OpenTelemetry データを表示します。実行時にデフォルトで起動する .NET Aspire の開発者ダッシュボードは、ログ、環境変数、分散トレース、メトリクスなどを表示して、アプリの動作を素早く確認できます。
- **デプロイメント**: 適切な接続文字列やネットワーク構成、サービスディスカバリー情報を挿入する行い、開発者の体験を簡素化します。
- **さらに多く**: .NET Aspire には、開発者が気に入る多くの機能を備え、生産性を向上させます。

こちらのリリースを使用して、.NET Aspire についてもっと学びましょう:

- [Documentation](https://learn.microsoft.com/dotnet/aspire)
- [Microsoft Learn Training Path](https://learn.microsoft.com/en-us/training/paths/dotnet-aspire/)
- [.NET Aspire ビデオ](https://aka.ms/aspire/videos)
- [eShop Reference サンプルアプリ](https://github.com/dotnet/eshop)
- [.NET Aspire サンプル](https://learn.microsoft.com/samples/browse/?expanded=dotnet&products=dotnet-aspire)
- [.NET Aspire FAQ](https://learn.microsoft.com/dotnet/aspire/reference/aspire-faq)

## ワークショップ

この .NET Aspire ワークショップは、[Let's Learn .NET](https://aka.ms/letslearndotnet) シリーズの一部です。このワークショップは、.NET Aspire について学び、クラウドレディなアプリケーションの構築に使用する方法を学ぶために設計されています。

### 前提条件

このワークショップを開始する前に、以下があることを確認してください:

- [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)（推奨）または [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Visual Studio 2022](https://visualstudio.microsoft.com/vs/) または C# 拡張機能付きの [Visual Studio Code](https://code.visualstudio.com/)
- [Docker Desktop](https://www.docker.com/products/docker-desktop/)（コンテナ化されたリソース用）

### ワークショップモジュール

このワークショップは、15 つのモジュールに分かれています（推定完了時間：4-6時間）:

1. [セットアップ](./workshop/Lesson-01-Setup/README.md)
1. [Service Defaults](./workshop/Lesson-02-ServiceDefaults/README.md)
1. [ダッシュボードとオーケストレーター](./workshop/Lesson-03-Dashboard-AppHost/README.md)
1. [サービス ディスカバリ](./workshop/Lesson-04-ServiceDiscovery/README.md)
1. [統合](./workshop/Lesson-05-Integrations/README.md)
1. [テレメトリモジュール](./workshop/Lesson-06-Telemetry/README.md)
1. [データベースモジュール](./workshop/Lesson-07-Database/README.md)
1. [統合テスト](./workshop/Lesson-08-Integration-Testing/README.md)
1. [デプロイメント](./workshop/Lesson-09-Deployment/README.md)
1. [コンテナ管理](./workshop/Lesson-10-Container-Management/README.md)
1. [Azure 統合](./workshop/Lesson-11-Azure-Integrations/README.md)
1. [カスタムコマンド](./workshop/Lesson-12-Custom-Commands/README.md)
1. [ヘルスチェック](./workshop/Lesson-13-HealthChecks/README.md)
1. [GitHub Models 統合](./workshop/Lesson-14-GitHub-Models-Integration/README.md)
1. [Docker 統合](./workshop/Lesson-15-Docker-Integration/README.md)

このワークショップの完全な[スライド](./workshop/AspireWorkshop.pptx)を利用できます。

### はじめに

このワークショップの開始プロジェクトは、`start` フォルダに格納されています。このプロジェクトは、National Weather Service API を使用して、天気データを取得し、Blazor によって提供される Web フロントエンドで天気データを表示するシンプルな天気 API です。

ワークショップを開始するには:

1. `start` フォルダに移動
2. ソリューションファイル `MyWeatherHub.sln` を開く
3. [モジュール 1: セットアップ](./workshop/Lesson-01-Setup/README.md) の手順に従ってください

## デモ データ

このチュートリアルで使用するデータとサービスは、アメリカ国立気象局 (NWS) の <https://weather.gov> から提供されています。NWS の OpenAPI 仕様を使用して天気予報をクエリします。OpenAPI 仕様は [こちら](https://www.weather.gov/documentation/services-web-api) から入手できます。私たちはこの API の 2 つのメソッドのみを使用しており、NWS API の全体的な OpenAPI クライアントを作成するのではなく、そのメソッドだけを使用するようにコードを簡略化しています。
