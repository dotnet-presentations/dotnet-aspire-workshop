# Implante um aplicativo .NET Aspire no Azure Container Apps

O .NET Aspire é otimizado para aplicativos destinados a serem executados em ambientes contêinerizados. O [Azure Container Apps](https://learn.microsoft.com/azure/container-apps/overview) é um ambiente totalmente gerenciado que permite executar microsserviços e aplicativos contêinerizados em uma plataforma sem servidor. Este artigo irá guiá-lo na criação de uma nova solução .NET Aspire e na implantação dela no Microsoft Azure Container Apps usando o Visual Studio e o Azure Developer CLI (`azd`).

Neste exemplo, assumiremos que você está implantando o aplicativo MyWeatherHub das seções anteriores. Você pode usar o código que construiu, ou pode usar o código no diretório **complete**. No entanto, os passos gerais são os mesmos para qualquer aplicativo .NET Aspire.

## Implante o aplicativo com o Visual Studio

1. No Gerenciador de Soluções, clique com o botão direito do mouse no projeto **AppHost** e selecione **Publicar** para abrir a caixa de diálogo **Publicar**.

    > Publicar .NET Aspire requer a versão atual da CLI `azd`. Isso deve ser instalado com a carga de trabalho do .NET Aspire, mas se você receber uma notificação de que a CLI não está instalada ou atualizada, pode seguir as instruções na próxima parte deste tutorial para instalá-la.

1. Selecione **Azure Container Apps for .NET Aspire** como o destino de publicação.

    ![Uma captura de tela do fluxo de trabalho da caixa de diálogo de publicação.](../media/vs-deploy.png)

1. Na etapa **Ambiente AzDev**, selecione os valores desejados de **Assinatura** e **Localização** e então digite um **Nome do ambiente** como _aspire-weather_. O nome do ambiente determina a nomenclatura dos recursos do ambiente Azure Container Apps.
1. Selecione **Concluir** para criar o ambiente, então **Fechar** para sair do fluxo de trabalho da caixa de diálogo e ver o resumo do ambiente de implantação.
1. Selecione **Publicar** para provisionar e implantar os recursos no Azure.

    > Este processo pode levar vários minutos para ser concluído. O Visual Studio fornece atualizações de status sobre o progresso da implantação nos logs de saída e você pode aprender muito sobre como a publicação funciona observando essas atualizações! Você verá que o processo envolve criar um grupo de recursos, um Azure Container Registry, um workspace do Log Analytics e um ambiente Azure Container Apps. O aplicativo é então implantado no ambiente Azure Container Apps.

1. Quando a publicação for concluída, o Visual Studio exibe as URLs dos recursos na parte inferior da tela do ambiente. Use esses links para ver os vários recursos implantados. Selecione a URL **webfrontend** para abrir um navegador para o aplicativo implantado.

    ![Uma captura de tela do processo de publicação concluído e dos recursos implantados.](../media/vs-publish-complete.png)

## Implante o aplicativo com o Azure Developer CLI (azd)

### Instalar o Azure Developer CLI

O processo para instalar `azd` varia com base no seu sistema operacional, mas está amplamente disponível via `winget`, `brew`, `apt`, ou diretamente via `curl`. Para instalar `azd`, veja [Instalar Azure Developer CLI](https://learn.microsoft.com/azure/developer/azure-developer-cli/install-azd).

### Novo no .NET Aspire 9.4: comando aspire deploy

O .NET Aspire 9.4 introduz o comando `aspire deploy` (preview/feature flag) que estende as capacidades de publicação para implantar ativamente em ambientes de destino. Este comando fornece fluxos de trabalho de implantação aprimorados com lógica personalizada de pré/pós-implantação.

Para habilitar este recurso:

```bash
aspire config set features.deployCommandEnabled true
```

Então você pode usar:

```bash
aspire deploy
```

Este comando fornece relatórios de progresso aprimorados, melhores mensagens de erro e suporte para hooks de implantação personalizados para cenários de implantação complexos.

### Inicializar o template

> Pré-requisitos:
>
> - Certifique-se de estar logado: execute `azd login` e selecione a assinatura Azure correta.
> - Execute os comandos seguintes da pasta que contém seu AppHost (para este repositório, tipicamente a pasta `complete` se você estiver implantando o exemplo concluído).

1. Abra uma nova janela de terminal e `cd` para a raiz do seu projeto .NET Aspire.
1. Execute o comando `azd init` para inicializar seu projeto com `azd`, que inspecionará a estrutura de diretório local e determinará o tipo de aplicativo.

    ```console
    azd init
    ```

    Para mais informações sobre o comando `azd init`, veja [azd init](https://learn.microsoft.com/azure/developer/azure-developer-cli/reference#azd-init).
1. Se esta for a primeira vez que você inicializa o aplicativo, `azd` solicita o nome do ambiente:

    ```console
    Inicializando um aplicativo para executar no Azure (azd init)
    
    ? Digite um novo nome de ambiente: [? para ajuda]
    ```

    Digite o nome do ambiente desejado para continuar. Para mais informações sobre gerenciar ambientes com `azd`, veja [azd env](https://learn.microsoft.com/azure/developer/azure-developer-cli/reference#azd-env).
1. Selecione **Usar código no diretório atual** quando `azd` apresentar duas opções de inicialização de aplicativo.

    ```console
    ? Como você quer inicializar seu aplicativo?  [Use setas para mover, digite para filtrar]
    > Usar código no diretório atual
      Selecionar um template
    ```

1. Após escanear o diretório, `azd` solicita que você confirme que encontrou o projeto .NET Aspire _AppHost_ correto. Selecione a opção **Confirmar e continuar inicializando meu aplicativo**.

    ```console
    Serviços detectados:
    
      .NET (Aspire)
      Detectado em: D:\source\repos\letslearn-dotnet-aspire\complete\AppHost\AppHost.csproj
    
    azd gerará os arquivos necessários para hospedar seu aplicativo no Azure usando Azure Container Apps.
    
    ? Selecione uma opção  [Use setas para mover, digite para filtrar]
    > Confirmar e continuar inicializando meu aplicativo
      Cancelar e sair
    ```

1. `azd` apresenta cada um dos projetos na solução .NET Aspire e solicita que você identifique quais implantar com ingresso HTTP aberto publicamente para todo o tráfego da internet. Selecione apenas `myweatherhub` (usando as teclas ↓ e Espaço), já que você quer que a API (`api`) seja privada para o ambiente Azure Container Apps e não disponível publicamente.

    ```console
    ? Selecione uma opção Confirmar e continuar inicializando meu aplicativo
    Por padrão, um serviço só pode ser alcançado de dentro do ambiente Azure Container Apps em que está executando. Selecionar um serviço aqui também permitirá que seja alcançado da Internet.
    ? Selecione quais serviços expor à Internet  [Use setas para mover, espaço para selecionar, <direita> para todos, <esquerda> para nenhum, digite para filtrar]
          [ ]  api
        > [x]  myweatherhub
    ```

1. Finalmente, especifique o nome do ambiente, que é usado para nomear recursos provisionados no Azure e gerenciar diferentes ambientes como `dev` e `prod`.

    ```console
    Gerando arquivos para executar seu aplicativo no Azure:
    
      (✓) Concluído: Gerando ./azure.yaml
      (✓) Concluído: Gerando ./next-steps.md
    
    SUCESSO: Seu aplicativo está pronto para a nuvem!
    Você pode provisionar e implantar seu aplicativo no Azure executando o comando azd up neste diretório. Para mais informações sobre configurar seu aplicativo, veja ./next-steps.md
    ```

`azd` gera vários arquivos e os coloca no diretório de trabalho. Esses arquivos são:

- _azure.yaml_: Descreve os serviços do aplicativo, como o projeto .NET Aspire AppHost, e os mapeia para recursos Azure.
- _.azure/config.json_: Arquivo de configuração que informa ao `azd` qual é o ambiente ativo atual.
- _.azure/aspireazddev/.env_: Contém substituições específicas do ambiente.
- _.azure/aspireazddev/config.json_: Arquivo de configuração que informa ao `azd` quais serviços devem ter um endpoint público neste ambiente.

### Implantar o aplicativo

Uma vez que `azd` está inicializado, o processo de provisionamento e implantação pode ser executado como um único comando, [azd up](https://learn.microsoft.com/azure/developer/azure-developer-cli/reference#azd-up).

```console
Por padrão, um serviço só pode ser alcançado de dentro do ambiente Azure Container Apps em que está executando. Selecionar um serviço aqui também permitirá que seja alcançado da Internet.
? Selecione quais serviços expor à Internet webfrontend
? Selecione uma Assinatura Azure para usar:  1. <SUA ASSINATURA>
? Selecione uma localização Azure para usar: 1. <SUA LOCALIZAÇÃO>

Empacotando serviços (azd package)


SUCESSO: Seu aplicativo foi empacotado para Azure em menos de um segundo.

Provisionando recursos Azure (azd provision)
Provisionar recursos Azure pode levar algum tempo.

Assinatura: <SUA ASSINATURA>
Localização: <SUA LOCALIZAÇÃO>

  Você pode ver o progresso detalhado no Portal Azure:
<LINK PARA IMPLANTAÇÃO>

  (✓) Concluído: Grupo de recursos: <SEU GRUPO DE RECURSOS>
  (✓) Concluído: Container Registry: <ID>
  (✓) Concluído: Workspace Log Analytics: <ID>
  (✓) Concluído: Ambiente Container Apps: <ID>
  (✓) Concluído: Container App: <ID>

SUCESSO: Seu aplicativo foi provisionado no Azure em 1 minuto e 13 segundos.
Você pode ver os recursos criados sob o grupo de recursos <SEU GRUPO DE RECURSOS> no Portal Azure:
<LINK PARA VISÃO GERAL DO GRUPO DE RECURSOS>

Implantando serviços (azd deploy)

  (✓) Concluído: Implantando serviço api
  - Endpoint: <somente interno>

  (✓) Concluído: Implantando serviço myweatherhub
  - Endpoint: <SEU APLICATIVO myweatherhub ÚNICO>.azurecontainerapps.io/


SUCESSO: Seu aplicativo foi implantado no Azure em 1 minuto e 39 segundos.
Você pode ver os recursos criados sob o grupo de recursos <SEU GRUPO DE RECURSOS> no Portal Azure:
<LINK PARA VISÃO GERAL DO GRUPO DE RECURSOS>

SUCESSO: Seu fluxo de trabalho up para provisionar e implantar no Azure foi concluído em 3 minutos e 50 segundos.
```

Primeiro, os projetos serão empacotados em contêineres durante a fase `azd package`, seguida pela fase `azd provision` durante a qual todos os recursos Azure que o aplicativo precisará são provisionados.

Uma vez que `provision` esteja completo, `azd deploy` ocorrerá. Durante esta fase, os projetos são enviados como contêineres para uma instância Azure Container Registry, e então usados para criar novas revisões de Azure Container Apps nas quais o código será hospedado.

Neste ponto o aplicativo foi implantado e configurado, e você pode abrir o portal Azure e explorar os recursos.

## Limpar recursos

Execute o seguinte comando Azure CLI para deletar o grupo de recursos quando você não precisar mais dos recursos Azure que criou. Deletar o grupo de recursos também deleta os recursos contidos dentro dele.

```console
az group delete --name <nome-do-seu-grupo-de-recursos>
```

**Próximo**: [Módulo #10: Gerenciamento Avançado de Contêineres](../Lesson-10-Container-Management/README.md)
