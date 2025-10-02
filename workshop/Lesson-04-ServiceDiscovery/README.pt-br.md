# Descoberta de Serviços (Service Discovery)

O .NET Aspire inclui funcionalidades para configurar a descoberta de serviços durante o desenvolvimento e testes. A funcionalidade de descoberta de serviços funciona fornecendo configuração no formato esperado pelo resolvedor de endpoints baseado em configuração do projeto .NET Aspire AppHost para os projetos de serviço individuais adicionados ao modelo de aplicação.

## Configuração de Descoberta de Serviços

Atualmente, o `MyWeatherHub` está usando uma configuração estática para se conectar ao `Api`. Isso não é ideal por várias razões, incluindo:

- O número da porta do serviço `Api` pode mudar.
- O endereço IP do serviço `Api` pode mudar.
- Múltiplas configurações precisariam ser definidas para as configurações http e https.
- À medida que adicionamos mais serviços, a configuração se tornaria mais complexa.

Para resolver esses problemas, usaremos a funcionalidade de descoberta de serviços fornecida pelo projeto .NET Aspire AppHost. Isso permitirá que o serviço `MyWeatherHub` descubra o serviço `Api` em tempo de execução.

1. Abra o arquivo `Program.cs` no projeto `AppHost`.
1. Anteriormente, adicionamos orquestração para incluir vários projetos usando o método `builder.AddProject`. Isso retornou um `IResourceBuild` que pode ser usado para referenciar projetos. Vamos referenciar o projeto `Api` no projeto `MyWeatherHub` atualizando o código:

    ```csharp

 var api = builder.AddProject<Projects.Api>("api");

 var web = builder.AddProject<Projects.MyWeatherHub>("myweatherhub")
  .WithReference(api)
  .WithExternalHttpEndpoints();
    ```

1. O método `WithReference` é usado para referenciar o projeto `Api`. Isso permitirá que o projeto `MyWeatherHub` descubra o projeto `Api` em tempo de execução.
1. Se você decidir implantar este aplicativo mais tarde, precisará da chamada para `WithExternalHttpEndpoints` para garantir que ele seja público para o mundo externo.

## Habilitando a Descoberta de Serviços no MyWeatherHub

Quando adicionamos ServiceDefaults aos projetos, nós automaticamente os inscrevemos no sistema de descoberta de serviços. Isso significa que o projeto `MyWeatherHub` já está configurado para usar a descoberta de serviços.
Alguns serviços expõem múltiplos endpoints (named endpoints). Endpoints podem ser resolvidos especificando o nome do endpoint na parte do host da URI da solicitação HTTP, seguindo o formato `scheme://_endpointName.serviceName`. Por exemplo, se um serviço chamado "basket" expõe um endpoint chamado "dashboard", então a URI `scheme+http://_dashboard.basket` pode ser usada para especificar este endpoint, por exemplo:

```csharp
builder.Services.AddHttpClient<BasketServiceClient>(
 static client => client.BaseAddress = new("https+http://basket"));

builder.Services.AddHttpClient<BasketServiceDashboardClient>(
 static client => client.BaseAddress = new("https+http://_dashboard.basket"));
```

No exemplo, o `BasketServiceClient` usará o endpoint padrão do serviço `basket`, enquanto o `BasketServiceDashboardClient` usará o endpoint `dashboard` do serviço `basket`. Agora, vamos atualizar o projeto `MyWeatherHub` para usar a descoberta de serviços para se conectar ao serviço `Api`.

Isso pode ser realizado atualizando as configurações existentes de `WeatherEndpoint` no `appsettings.json`. Isso é conveniente ao habilitar o .NET Aspire em um aplicativo já implantado, pois você pode continuar a usar suas configurações existentes.

1. Abra o arquivo `appsettings.json` no projeto `MyWeatherHub`.

1. Atualize as configurações de `WeatherEndpoint` para usar a descoberta de serviços:

```json
"WeatherEndpoint": "https+http://api"
```

> URI multi-esquema: `https+http://api` instrui o resolvedor a tentar primeiro um endpoint HTTPS para o serviço `api` e, caso não exista, usar HTTP. Os esquemas são avaliados da esquerda para a direita e separados por `+`. Isso evita ter que alterar a configuração ao alternar entre desenvolvimento local (somente HTTP) e ambientes com HTTPS. Use esse formato apenas para endereços internos de `HttpClient`—não o exponha diretamente em links para o usuário.

1. A configuração de `WeatherEndpoint` agora está usando a descoberta de serviços para se conectar ao serviço `Api`.

Opcionalmente, podemos atualizar a url para não usar as configurações de `WeatherEndpoint`.

1. Abra o arquivo `Program.cs` no projeto `MyWeatherHub`.
1. Atualize as configurações de `WeatherEndpoint` para usar a descoberta de serviços:

```csharp
builder.Services.AddHttpClient<NwsManager>(
    static client => client.BaseAddress = new("https+http://api"));
```

## Execute a aplicação

1. Execute a aplicação pressionando `F5` ou selecionando a opção `Start Debugging`.
1. Abra o `MyWeatheApp` selecionando o endpoint no painel.
1. Observe que o aplicativo `MyWeatherHub` ainda funciona e agora está usando a descoberta de serviços para se conectar ao serviço `Api`.
1. No painel, clique em `Details` para o projeto `MyWeatherHub`. Isso mostrará todas as configurações que o .NET Aspire configurou ao executar o aplicativo a partir do App Host
1. Clique no ícone de olho para revelar os valores e role até o final onde você verá `services__api_http_0` e `services__api_https_0` configurados com os valores corretos do serviço `Api`.

    ![Configurações de descoberta de serviços no painel](.../media/dashboard-servicediscovery.png)

## Conclusão

Isso foi apenas o começo do que podemos fazer com a descoberta de serviços e o .NET Aspire. À medida que nossa aplicação cresce e adicionamos mais serviços, podemos continuar a usar a descoberta de serviços para conectar serviços em tempo de execução. Isso nos permitirá escalar nossa aplicação facilmente e torná-la mais resiliente a mudanças no ambiente.

## Saiba Mais

Você pode aprender mais sobre o uso avançado e configuração da descoberta de serviços na documentação do [.NET Aspire Service Discovery](https://learn.microsoft.com/dotnet/aspire/service-discovery/overview).
