# Azure Container Apps への .NET Aspire アプリのデプロイ

.NET Aspire は、コンテナ化環境での実行を目的としたアプリケーション向けに最適化されています。[Azure Container Apps](https://learn.microsoft.com/azure/container-apps/overview) は、サーバーレス プラットフォームでマイクロサービスやコンテナ化されたアプリケーションを実行できる完全管理型環境です。この記事では、新しい .NET Aspire ソリューションを作成し、Visual Studio と Azure Developer CLI (`azd`) を使用して Microsoft Azure Container Apps にデプロイする方法を説明します。

この例では、前のセクションで作成した MyWeatherHub アプリをデプロイすることを前提としています。自分で構築したコードを使用するか、**complete** ディレクトリのコードを使用できます。ただし、一般的な手順はどの .NET Aspire アプリでも同じです。

## Visual Studio を使用してアプリをデプロイ

1. ソリューション エクスプローラーで **AppHost** プロジェクトを右クリックし、**発行** を選択して **発行** ダイアログを開きます。

    > .NET Aspire の発行には、現在のバージョンの `azd` CLI が必要です。これは .NET Aspire ワークロードと一緒にインストールされるはずですが、CLI がインストールされていない、または最新でないという通知を受け取った場合は、このチュートリアルの次の部分の手順に従ってインストールできます。

1. 発行ターゲットとして **Azure Container Apps for .NET Aspire** を選択します。

    ![発行ダイアログ ワークフローのスクリーンショット。](../media/vs-deploy.png)

1. **AzDev Environment** ステップで、希望する **サブスクリプション** と **場所** の値を選択し、_aspire-weather_ などの **環境名** を入力します。環境名は、Azure Container Apps 環境リソースの命名を決定します。
1. **完了** を選択して環境を作成し、**閉じる** を選択してダイアログ ワークフローを終了し、デプロイ環境の概要を表示します。
1. **発行** を選択して、Azure でリソースをプロビジョニングしてデプロイします。

    > このプロセスの完了には数分かかる場合があります。Visual Studio は出力ログでデプロイの進行状況の状態更新を提供し、これらの更新を観察することで発行の動作について多くを学ぶことができます！プロセスには、リソース グループ、Azure Container Registry、Log Analytics ワークスペース、Azure Container Apps 環境の作成が含まれることがわかります。その後、アプリが Azure Container Apps 環境にデプロイされます。

1. 発行が完了すると、Visual Studio は環境画面の下部にリソース URL を表示します。これらのリンクを使用して、デプロイされたさまざまなリソースを表示します。**webfrontend** URL を選択して、デプロイされたアプリへのブラウザーを開きます。

    ![完了した発行プロセスとデプロイされたリソースのスクリーンショット。](../media/vs-publish-complete.png)

## Azure Developer CLI (azd) を使用してアプリをデプロイ

### Azure Developer CLI のインストール

`azd` をインストールするプロセスはオペレーティング システムによって異なりますが、`winget`、`brew`、`apt`、または `curl` を介して直接広く利用できます。`azd` をインストールするには、[Azure Developer CLI のインストール](https://learn.microsoft.com/azure/developer/azure-developer-cli/install-azd) を参照してください。

### .NET Aspire 9.4 の新機能：aspire deploy コマンド

.NET Aspire 9.4 では、発行機能を拡張してターゲット環境に積極的にデプロイする `aspire deploy` コマンド（プレビュー/機能フラグ）が導入されました。このコマンドは、カスタムのプリ/ポストデプロイロジックを使用した拡張デプロイ ワークフローを提供します。

この機能を有効にするには：

```bash
aspire config set features.deployCommandEnabled true
```

その後、次を使用できます：

```bash
aspire deploy
```

このコマンドは、進行状況レポートの向上、より良いエラー メッセージ、複雑なデプロイ シナリオ用のカスタム デプロイ フックのサポートを提供します。

### テンプレートの初期化

> 前提条件：
>
> - サインインしていることを確認してください：`azd login` を実行し、正しい Azure サブスクリプションを選択します。
> - AppHost を含むフォルダーから次のコマンドを実行します（このリポジトリの場合、完成したサンプルをデプロイする場合は通常 `complete` フォルダー）。

1. 新しいターミナル ウィンドウを開き、.NET Aspire プロジェクトのルートに `cd` します。
1. `azd init` コマンドを実行してプロジェクトを `azd` で初期化します。これにより、ローカル ディレクトリ構造が検査され、アプリの種類が決定されます。

    ```console
    azd init
    ```

    `azd init` コマンドの詳細については、[azd init](https://learn.microsoft.com/azure/developer/azure-developer-cli/reference#azd-init) を参照してください。
1. アプリを初めて初期化する場合、`azd` は環境名の入力を求めます：

    ```console
    Azure で実行するアプリの初期化 (azd init)
    
    ? 新しい環境名を入力してください： [? でヘルプ]
    ```

    希望する環境名を入力して続行します。`azd` での環境管理の詳細については、[azd env](https://learn.microsoft.com/azure/developer/azure-developer-cli/reference#azd-env) を参照してください。
1. `azd` が 2 つのアプリ初期化オプションを提示したら、**現在のディレクトリのコードを使用** を選択します。

    ```console
    ? アプリをどのように初期化しますか？  [矢印で移動、タイプでフィルター]
    > 現在のディレクトリのコードを使用
      テンプレートを選択
    ```

1. ディレクトリをスキャンした後、`azd` は正しい .NET Aspire _AppHost_ プロジェクトが見つかったことの確認を求めます。**確認してアプリの初期化を続行** オプションを選択します。

    ```console
    検出されたサービス：
    
      .NET (Aspire)
      検出場所： D:\source\repos\letslearn-dotnet-aspire\complete\AppHost\AppHost.csproj
    
    azd は Azure Container Apps を使用して Azure でアプリをホストするために必要なファイルを生成します。
    
    ? オプションを選択してください  [矢印で移動、タイプでフィルター]
    > 確認してアプリの初期化を続行
      キャンセルして終了
    ```

1. `azd` は .NET Aspire ソリューション内の各プロジェクトを表示し、すべてのインターネット トラフィックに公開的に開かれた HTTP イングレスでデプロイするものを特定するよう求めます。API (`api`) を Azure Container Apps 環境のプライベートにして、公開的に利用できないようにしたいため、`myweatherhub` のみを選択します（↓ キーとスペース キーを使用）。

    ```console
    ? オプションを選択してください 確認してアプリの初期化を続行
    デフォルトでは、サービスは実行されている Azure Container Apps 環境の内部からのみ到達可能です。ここでサービスを選択すると、インターネットからも到達可能になります。
    ? インターネットに公開するサービスを選択してください  [矢印で移動、スペースで選択、<右>ですべて、<左>でなし、タイプでフィルター]
          [ ]  api
        > [x]  myweatherhub
    ```

1. 最後に、環境名を指定します。これは Azure でプロビジョニングされるリソースの命名と、`dev` や `prod` などの異なる環境の管理に使用されます。

    ```console
    Azure でアプリを実行するためのファイル生成：
    
      (✓) 完了： ./azure.yaml の生成
      (✓) 完了： ./next-steps.md の生成
    
    成功： アプリはクラウド対応です！
    このディレクトリで azd up コマンドを実行して、Azure でアプリをプロビジョニングしてデプロイできます。アプリの構成の詳細については、./next-steps.md を参照してください
    ```

`azd` は多数のファイルを生成し、作業ディレクトリに配置します。これらのファイルは：

- _azure.yaml_：.NET Aspire AppHost プロジェクトなどのアプリのサービスを記述し、Azure リソースにマップします。
- _.azure/config.json_：現在のアクティブな環境が何かを `azd` に通知する構成ファイル。
- _.azure/aspireazddev/.env_：環境固有のオーバーライドを含みます。
- _.azure/aspireazddev/config.json_：この環境でパブリック エンドポイントを持つべきサービスを `azd` に通知する構成ファイル。

### アプリのデプロイ

`azd` が初期化されると、プロビジョニングとデプロイのプロセスを単一のコマンドとして実行できます。[azd up](https://learn.microsoft.com/azure/developer/azure-developer-cli/reference#azd-up)

```console
デフォルトでは、サービスは実行されている Azure Container Apps 環境の内部からのみ到達可能です。ここでサービスを選択すると、インターネットからも到達可能になります。
? インターネットに公開するサービスを選択してください webfrontend
? 使用する Azure サブスクリプションを選択してください：  1. <あなたのサブスクリプション>
? 使用する Azure の場所を選択してください： 1. <あなたの場所>

サービスのパッケージ化 (azd package)


成功： アプリケーションは 1 秒未満で Azure 用にパッケージ化されました。

Azure リソースのプロビジョニング (azd provision)
Azure リソースのプロビジョニングには時間がかかる場合があります。

サブスクリプション： <あなたのサブスクリプション>
場所： <あなたの場所>

  Azure ポータルで詳細な進行状況を確認できます：
<デプロイへのリンク>

  (✓) 完了： リソース グループ： <あなたのリソース グループ>
  (✓) 完了： Container Registry： <ID>
  (✓) 完了： Log Analytics ワークスペース： <ID>
  (✓) 完了： Container Apps 環境： <ID>
  (✓) 完了： Container App： <ID>

成功： アプリケーションは 1 分 13 秒で Azure にプロビジョニングされました。
Azure ポータルでリソース グループ <あなたのリソース グループ> の下に作成されたリソースを確認できます：
<リソース グループ概要へのリンク>

サービスのデプロイ (azd deploy)

  (✓) 完了： サービス api のデプロイ
  - エンドポイント： <内部のみ>

  (✓) 完了： サービス myweatherhub のデプロイ
  - エンドポイント： <あなたの固有の myweatherhub アプリ>.azurecontainerapps.io/


成功： アプリケーションは 1 分 39 秒で Azure にデプロイされました。
Azure ポータルでリソース グループ <あなたのリソース グループ> の下に作成されたリソースを確認できます：
<リソース グループ概要へのリンク>

成功： Azure でのプロビジョニングとデプロイの up ワークフローが 3 分 50 秒で完了しました。
```

まず、プロジェクトは `azd package` フェーズでコンテナーにパッケージ化され、その後 `azd provision` フェーズでアプリが必要とするすべての Azure リソースがプロビジョニングされます。

`provision` が完了すると、`azd deploy` が実行されます。このフェーズでは、プロジェクトがコンテナーとして Azure Container Registry インスタンスにプッシュされ、その後コードがホストされる Azure Container Apps の新しいリビジョンを作成するために使用されます。

この時点で、アプリがデプロイされて構成され、Azure ポータルを開いてリソースを探索できます。

## リソースのクリーンアップ

作成した Azure リソースが不要になったら、次の Azure CLI コマンドを実行してリソース グループを削除します。リソース グループを削除すると、その中に含まれるリソースも削除されます。

```console
az group delete --name <あなたのリソース グループ名>
```

**次**: [モジュール #10: 高度なコンテナー管理](../Lesson-10-Container-Management/README.md)
