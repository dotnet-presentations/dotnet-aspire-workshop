# Padrões de Serviço (Service Defaults/Smart Defaults)

## Introdução

.NET Aspire fornece um conjunto de padrões inteligentes para serviços que são comumente usados em aplicações .NET. Esses padrões são projetados para ajudá-lo a começar rapidamente e fornecer uma experiência consistente em diferentes tipos de aplicações. Isso inclui:

- Telemetria: Métricas, Rastreamento, Log
- Resiliência
- Verificações de Saúde do Ambiente (Health Checks)
- Descoberta de Serviço (Service Discovery)

## Crie um Projeto de Padrões de Serviço

### Visual Studio & Visual Studio Code

1. Adicione um novo projeto à solução chamado `ServiceDefaults`:

- Clique com o botão direito na solução e selecione `Add` > `New Project`.
- Selecione o modelo de projeto `.NET Aspire Service Defaults`.
- Nomeie o projeto como `ServiceDefaults`.
- Clique em `Next` > `Create`.

 *Visual Studio*
 ![Janela do Visual Studio para adicionar um projeto de padrões de serviço](.../media/vs-add-servicedefaults.png)

 *Visual Studio Code*
 ![Janela do Visual Studio Code para adicionar um projeto de padrões de serviço](.../media/vsc-add-servicedefaults.png)

### Linha de Comando

1. Crie um novo projeto usando o comando `dotnet new aspire-servicedefaults`:

 ```bash
 dotnet new aspire-servicedefaults -n ServiceDefaults
 ```

## Configure o Padrões de Serviço

1. Adicione uma referência ao projeto `ServiceDefaults` nos projetos `Api` e `MyWeatherHub`:

- Clique com o botão direito no projeto `Api` e selecione `Add` > `Reference`.
- Selecione o projeto `ServiceDefaults` e clique em `OK`.
- Clique com o botão direito no projeto `MyWeatherHub` e selecione `Add` > `Reference`.
- Selecione o projeto `ServiceDefaults` e clique em `OK`.

 > Dica: No Visual Studio 2022, você pode arrastar e soltar o projeto em outro projeto para adicionar uma referência.

1. Nos projetos `Api` e `MyWeatherHub`, atualize seus arquivos `Program.cs`, adicionando a seguinte linha imediatamente após a linha `var builder = WebApplication.CreateBuilder(args);`:

 ```csharp
 builder.AddServiceDefaults();
 ```

1. Nos projetos `Api` e `MyWeatherHub`, atualize seus arquivos `Program.cs`, adicionando a seguinte linha imediatamente após a linha `var app = builder.Build();`:

 ```csharp
 app.MapDefaultEndpoints();
 ```

## Execute a aplicação

1. Execute a aplicação usando uma configuração de execução multiprojeto no Visual Studio ou Visual Studio Code.

- Visual Studio: Clique com o botão direito na solução `MyWeatherHub` e vá para propriedades. Selecione `Api` e `MyWeatherHub` como "startup projects", selecione `OK`.
- ![Propriedades da solução do Visual Studio](.../media/vs-multiproject.png)
- Clique em `Start` para começar e depurar ambos os projetos.
- Visual Studio Code: Execute os projetos `Api` e `MyWeatherHub` usando o painel `Run and Debug`. Fornecemos um arquivo `launch.json` com as configurações necessárias para executar ambos.

1. Teste a aplicação navegando para os seguintes URLs:

- [https://localhost:7032/openapi/v1.json](https://localhost:7032/openapi/v1.json) - API (documento OpenAPI)
- [https://localhost:7274/](https://localhost:7274/) - MyWeatherHub

1. Você deve ver o documento OpenAPI (JSON) da API e a página inicial do MyWeatherHub.
1. Você também pode accessar as verificações de saúde (health checks) da API navegando para [https://localhost:7032/health](https://localhost:7032/health).
1. Você também pode visualizar as verificações de saúde (health checks) do MyWeatherHub navegando para [https://localhost:7274/health](https://localhost:7274/health).
1. Veja os logs no terminal para ver as verificações de saúde (health checks) e outros dados de telemetria, como resiliência com Polly:

 ```bash
 Polly: Information: Execution attempt. Source: '-standard//Standard-Retry', Operation Key: '', Result: '200', Handled: 'False', Attempt: '0', Execution Time: '13.0649'
 ```

1. Clique em 5 cidades diferentes e um erro "aleatório" será lançado. Você verá a política de nova tentativa do Polly em ação.

 ```bash
 Polly: Warning: Execution attempt. Source: '-standard//Standard-Retry', Operation Key: '', Result: '500', Handled: 'True', Attempt: '0', Execution Time: '9732.8258'
 Polly: Warning: Resilience event occurred. EventName: 'OnRetry', Source: '-standard//Standard-Retry', Operation Key: '', Result: '500'
 System.Net.Http.HttpClient.NwsManager.ClientHandler: Information: Sending HTTP request GET http://localhost:5271/forecast/AKZ318
 ```
