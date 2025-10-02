# Integrações .NET Aspire

Os componentes .NET Aspire são um conjunto selecionado de pacotes NuGet especificamente escolhidos para facilitar a integração de aplicações nativas da nuvem com serviços e plataformas proeminentes, incluindo, mas não se limitando a, Redis e PostgreSQL. Cada componente fornece funcionalidades essenciais nativas da nuvem por meio de provisionamento automático ou padrões de configuração padronizados. Os componentes .NET Aspire podem ser usados sem um projeto de host de aplicativo (orquestrador), mas são projetados para funcionar melhor com o host de aplicativos .NET Aspire.

Os componentes .NET Aspire não devem ser confundidos com os pacotes de hospedagem .NET Aspire, pois servem a propósitos diferentes. Os pacotes de hospedagem são usados para modelar e configurar vários recursos em um aplicativo .NET Aspire, enquanto os componentes são usados para mapear a configuração para várias bibliotecas de clientes.

Existe uma lista sempre crescente de [Componentes .NET Aspire](https://learn.microsoft.com/dotnet/aspire/fundamentals/components-overview?tabs=dotnet-cli#available-components) criados e distribuídos pela Microsoft e pela comunidade. .NET Aspire é flexível e qualquer um pode criar seu próprio componente para integrar com seus próprios serviços.

Vamos melhorar nossa aplicação adicionando um componente a ela. Vamos adicionar um componente que nos ajudará a conectar a um cache Redis para melhorar o desempenho da nossa API.

## Adicione o componente Redis ao Host de Aplicativo

Existem dois tipos de cache que poderíamos integrar à nossa aplicação, incluindo:

- **Cache de saída** (Output caching): Um método de cache configurável e extensível para armazenar respostas HTTP completas para solicitações futuras.
- **Cache distribuído** (Distributed caching): Um cache compartilhado por vários servidores de aplicativos que permite armazenar peças específicas de dados. Um cache distribuído é normalmente mantido como um serviço externo aos servidores de aplicativos que o acessam e pode melhorar o desempenho e a escalabilidade de um aplicativo ASP.NET Core.

Vamos integrar o componente _Output caching_ ao nosso host de aplicativo. Este componente nos ajudará a armazenar em cache a resposta da nossa API no cache Redis.

Para adicionar o componente Redis ao nosso host de aplicativo, precisamos instalar o pacote NuGet `Aspire.Hosting.Redis`. Este pacote fornece os componentes necessários para configurar o serviço no Host de Aplicativo. O Redis é fornecido por meio de uma imagem de contêiner neste workshop, e quando iniciamos o Host de Aplicativo .NET Aspire, ele automaticamente baixa a imagem do contêiner Redis e inicia o servidor Redis.

Com o NuGet instalado, podemos configurá-lo.

1. Abra o arquivo `Program.cs` no projeto `AppHost`.
1. Adicione o seguinte código abaixo de `var builder = DistributedApplication.CreateBuilder(args);`

    ```csharp

 var cache = builder.AddRedis("cache")
    ```

 Aqui, configuramos o cache Redis com o nome `cache`. Este nome é usado para identificar o cache na `Api` ou `MyWeatherHub`.

1. Atualize a `api` no Host de Aplicativo com uma referência ao cache.

    ```csharp
    var api = builder.AddProject<Projects.Api>("api")
            .WithReference(cache);
    ```

1. Além disso, poderíamos configurar o [Redis Commander](https://joeferner.github.io/redis-commander/), uma ferramenta de gerenciamento Redis. Como parte do pacote `Aspire.Hosting.Redis`, o Redis Commander está disponível no mesmo componente. Para adicionar o Redis Commander, adicione o seguinte código abaixo da nova configuração do Redis adicionada.

    ```csharp
    var cache = builder.AddRedis("cache")
            .WithRedisCommander();
    ```

## Execute a aplicação (iniciar o Redis)

Não fizemos nenhuma alteração nos projetos `Api` ou `MyWeatherHub`, mas podemos ver o cache Redis iniciar quando iniciamos o Host de Aplicativo.

> [!IMPORTANT]
> Como o Redis é executado em um contêiner, você precisará garantir que o Docker esteja rodando em sua máquina.

1. Inicie o Docker Desktop ou Podman.
1. Inicie o projeto Host (App Host) de Aplicativo.
1. Você verá tanto o contêiner do Redis quanto o contêiner do Redis Commander sendo baixados e iniciados tanto no dashboard quanto no Docker Desktop.

    ![Redis rodando no dashboard e no desktop](.../media/redis-started.png)

## Integre o Cache de Saída (Output Caching) na API

1. Instale o pacote NuGet `Aspire.StackExchange.Redis.OutputCaching` no projeto `Api` para obter acesso às APIs Redis.
1. Abra o arquivo `Program.cs` no projeto `Api`.
1. Adicione o seguinte código abaixo de `var builder = WebApplication.CreateBuilder(args);` no topo do arquivo:

    ```csharp
    builder.AddRedisOutputCache("cache");
    ```

    > Observe que estamos usando o nome "cache" para referenciar o cache Redis que configuramos no Host de Aplicativo.

1. O `NwsManager` já foi configurado para usar o cache de saída, mas com um cache de memória. Vamos atualizá-lo para usar o cache Redis. Abra o arquivo `NwsManager.cs` na pasta `Data`.
1. Na classe `NwsManagerExtensions`, você encontrará um método `AddNwsManager`.
1. **DELETE** o seguinte código:

    ```csharp
    // Adicionar cache de saída padrão
    services.AddOutputCache(options =>
    {
        options.AddBasePolicy(builder => builder.Cache());
    });
    ```

    Como configuramos a aplicação para usar o cache Redis no arquivo `Program.cs`, não precisamos mais adicionar a política de cache de saída padrão.

## Execute a aplicação e teste o cache de saída

1. Inicie o projeto Host de Aplicativo e abra o projeto `MyWeatherHub` a partir do dashboard
1. Clique em uma cidade e depois clique novamente. Você verá que a resposta é armazenada em cache e a segunda solicitação é muito mais rápida que a primeira na aba `Traces`.

    ![Cache de saída em ação](.../media/output-caching.png)

1. Você também pode ver a resposta armazenada em cache no Redis Commander. Abra o Redis Commander clicando no endpoint `Redis Commander` no dashboard. Nas estatísticas, você verá conexões e comandos processados.

    ![Redis Commander](.../media/redis-commander.png)

1. Além disso, você pode ver os logs do cache Redis e do Redis Commander na aba `Console`.
    ![Logs do Redis](.../media/redis-logs.png)

## Contêineres Redis Personalizados

Os componentes .NET Aspire são flexíveis e personalizáveis. Por padrão, o componente Redis usa uma imagem de contêiner Redis do Docker Hub. No entanto, você pode usar sua própria imagem de contêiner Redis fornecendo o nome da imagem e a tag após o método `AddRedis`. Por exemplo, se você tem uma imagem de contêiner Redis personalizada, como [Garnet](https://github.com/microsoft/garnet), você pode fornecer o nome da imagem e a tag no Host de Aplicativo da seguinte forma:

```csharp
var cache = builder.AddRedis("cache")
    .WithImage("ghcr.io/microsoft/garnet")
    .WithImageTag("latest")
    .WithRedisCommander();
```

1. Execute a aplicação e agora você verá o Garnet rodando no dashboard e no Docker Desktop.

 ![Garnet rodando no dashboard e no Docker Desktop.](.../media/garnet-started.png)

1. Você também pode ver os logs do Garnet na aba `Console`.

 ![Logs do Garnet](.../media/garnet-logs.png)

## Resumo

Nesta seção, adicionamos um componente Redis ao Host de Aplicativo e integramos o cache de saída na API. Vimos como a resposta foi armazenada em cache no cache Redis e como a segunda solicitação foi muito mais rápida que a primeira. Também vimos como usar o Redis Commander para gerenciar o cache Redis.

Existem muitos mais componentes disponíveis que você pode usar para integrar com seus serviços. Você pode encontrar a lista de componentes disponíveis na documentação do .NET Aspire.

Um próximo passo natural seria integrar um banco de dados ou aproveitar o Azure Redis Cache como uma solução hospedada. Componentes para estes e mais estão disponíveis no NuGet.
