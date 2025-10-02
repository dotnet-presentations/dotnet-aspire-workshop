# Testes de Integração com .NET Aspire

## Introdução

Neste módulo, abordaremos testes de integração usando `Aspire.Hosting.Testing` com `MSTest`. Os testes de integração são cruciais para garantir que diferentes partes da sua aplicação funcionem em conjunto como esperado. Criaremos um projeto de teste separado para testar tanto a API quanto a aplicação web.

## Diferença Entre Testes Unitários e Testes de Integração

Os testes unitários focam em testar componentes individuais ou unidades de código de forma isolada. Eles garantem que cada unidade funcione corretamente por si só. Em contraste, os testes de integração verificam se diferentes componentes da aplicação funcionam juntos como esperado. Eles testam as interações entre várias partes do sistema, como APIs, bancos de dados e aplicações web.

No contexto de aplicações distribuídas com .NET Aspire, os testes de integração são essenciais para garantir que os diferentes serviços e componentes se comuniquem e funcionem corretamente em conjunto.

## Criando o Projeto de Testes de Integração

1. Crie um novo projeto de teste chamado `IntegrationTests`.
1. Adicione referências aos pacotes necessários no arquivo `IntegrationTests.csproj`:

```xml
<Project Sdk="Microsoft.NET.Sdk">
    <PropertyGroup>
        <TargetFramework>net9.0</TargetFramework>
        <ImplicitUsings>enable</ImplicitUsings>
        <Nullable>enable</Nullable>
        <IsPackable>false</IsPackable>
        <IsTestProject>true</IsTestProject>
    </PropertyGroup>

    <PropertyGroup>
        <EnableMSTestRunner>true</EnableMSTestRunner>
        <OutputType>Exe</OutputType>
    </PropertyGroup>

    <ItemGroup>
        <PackageReference Include="Aspire.Hosting.Testing" Version="9.4.2" />
        <PackageReference Include="MSTest" Version="3.10.4" />
    </ItemGroup>

    <ItemGroup>
        <ProjectReference Include="..\AppHost\AppHost.csproj" />
    </ItemGroup>

    <ItemGroup>
        <Using Include="System.Net" />
        <Using Include="Microsoft.Extensions.DependencyInjection" />
        <Using Include="Aspire.Hosting.ApplicationModel" />
        <Using Include="Aspire.Hosting.Testing" />
        <Using Include="Microsoft.VisualStudio.TestTools.UnitTesting" />
    </ItemGroup>
</Project>
```

Este arquivo de projeto é bastante padrão para um projeto de teste. Os elementos-chave são:

- Uma `PackageReference` para o pacote NuGet [Aspire.Hosting.Testing](https://www.nuget.org/packages/Aspire.Hosting.Testing), que fornece os tipos e APIs necessários para testar aplicações .NET Aspire.
- Uma `ProjectReference` para o projeto AppHost, que dá ao projeto de teste acesso à definição da aplicação distribuída alvo.
- As configurações `EnableMSTestRunner` e `OutputType` para configurar o projeto de teste para executar com o runner nativo do MSTest.

> Nota: Qualquer versão MSTest 3.x está adequada para este workshop. Se o seu ambiente fornecer uma 3.x mais recente, você pode usar essa.

1. Crie classes de teste para testes de integração:

O arquivo `IntegrationTests.cs` testa a funcionalidade da API e da aplicação web:

```csharp
namespace MyWeatherHub.Tests;

[TestClass]
public class IntegrationTests
{
    [TestMethod]
    public async Task TestApiGetZones()
    {
        // Arrange
        var appHost = await DistributedApplicationTestingBuilder
            .CreateAsync<Projects.AppHost>();

        appHost.Services.ConfigureHttpClientDefaults(clientBuilder =>
        {
            clientBuilder.AddStandardResilienceHandler();
        });

        await using var app = await appHost.BuildAsync();

        var resourceNotificationService = app.Services
            .GetRequiredService<ResourceNotificationService>();

        await app.StartAsync();

        // Act
        var httpClient = app.CreateHttpClient("api");

        await resourceNotificationService.WaitForResourceAsync(
                "api",
                KnownResourceStates.Running
            )
            .WaitAsync(TimeSpan.FromSeconds(30));

        var response = await httpClient.GetAsync("/zones");

        // Assert
        response.EnsureSuccessStatusCode();
        var zones = await response.Content.ReadFromJsonAsync<Zone[]>();
        Assert.IsNotNull(zones);
        Assert.IsTrue(zones.Length > 0);
    }

    [TestMethod]
    public async Task TestWebAppHomePage()
    {
        // Arrange
        var appHost = await DistributedApplicationTestingBuilder
            .CreateAsync<Projects.AppHost>();

        appHost.Services.ConfigureHttpClientDefaults(clientBuilder =>
        {
            clientBuilder.AddStandardResilienceHandler();
        });

        await using var app = await appHost.BuildAsync();

        var resourceNotificationService = app.Services
            .GetRequiredService<ResourceNotificationService>();

        await app.StartAsync();

        // Act
        var httpClient = app.CreateHttpClient("myweatherhub");

        await resourceNotificationService.WaitForResourceAsync(
                "myweatherhub",
                KnownResourceStates.Running
            )
            .WaitAsync(TimeSpan.FromSeconds(30));

        var response = await httpClient.GetAsync("/");

        // Assert
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();
        Assert.IsTrue(content.Contains("MyWeatherHub"));
    }
}

public record Zone(string Key, string Name, string State);
```

Esta classe de teste demonstra como testar sua aplicação distribuída. Vamos examinar o que esses testes estão fazendo:

- Ambos os testes seguem um padrão similar, utilizando `DistributedApplicationTestingBuilder.CreateAsync<Projects.AppHost>()` para criar assincronamente uma instância do seu host de aplicação.
- O `appHost` é configurado com handlers de resiliência HTTP padrão, que fornecem políticas de retry e circuit breakers para comunicação HTTP mais robusta.
- O teste chama `appHost.BuildAsync()` para construir a aplicação e então recupera o `ResourceNotificationService` do contêiner DI.
- Após iniciar a aplicação com `app.StartAsync()`, um `HttpClient` é criado especificamente para o recurso sendo testado (seja "api" ou "myweatherhub").
- O teste aguarda até que o recurso alvo alcance o estado "Running" antes de prosseguir, garantindo que o serviço esteja pronto para aceitar requisições.
- Finalmente, requisições HTTP são feitas para endpoints específicos, e assertions verificam as respostas.

No primeiro teste, verificamos que o endpoint `/zones` da API retorna uma coleção válida de dados de zona. No segundo teste, verificamos que a página inicial da aplicação web carrega com sucesso e contém o conteúdo esperado.

O arquivo `EnvVarTests.cs` verifica a resolução de variáveis de ambiente:

```csharp
namespace MyWeatherHub.Tests;

[TestClass]
public class EnvVarTests
{
    [TestMethod]
    public async Task WebResourceEnvVarsResolveToApiService()
    {
        // Arrange
        var appHost = await DistributedApplicationTestingBuilder
            .CreateAsync<Projects.AppHost>();

        var frontend = (IResourceWithEnvironment)appHost.Resources
            .Single(static r => r.Name == "myweatherhub");

        // Act
        var envVars = await frontend.GetEnvironmentVariableValuesAsync(
            DistributedApplicationOperation.Publish);

        // Assert
        CollectionAssert.Contains(envVars,
            new KeyValuePair<string, string>(
                key: "services__api__https__0",
                value: "{api.bindings.https.url}"));
    }
}
```

Este teste foca na verificação da configuração de descoberta de serviços:

- Ele usa `DistributedApplicationTestingBuilder.CreateAsync<Projects.AppHost>()` para criar uma instância do host de aplicação.
- Em vez de iniciar a aplicação, ele recupera diretamente uma instância `IResourceWithEnvironment` representando o frontend web ("myweatherhub").
- Ele chama `GetEnvironmentVariableValuesAsync()` com o argumento `DistributedApplicationOperation.Publish` para obter as variáveis de ambiente que seriam publicadas para o recurso.
- Finalmente, ele afirma que o frontend web tem uma variável de ambiente que resolve para a URL do serviço da API, confirmando que a descoberta de serviços está configurada corretamente.

Este teste é particularmente valioso porque verifica que os serviços da sua aplicação estão corretamente conectados através de variáveis de ambiente, que é como o .NET Aspire lida com descoberta de serviços em aplicações distribuídas.

> Nota: Se você ver um arquivo `WeatherBackgroundTests.cs` na solução completa que está vazio, é um placeholder para futuros testes de job em background e pode ser ignorado para este workshop.

## Executando os Testes de Integração

### Usando a Linha de Comando

1. Abra um terminal e navegue para a pasta `complete`.
1. Execute os testes de integração usando o comando `dotnet test`:

```bash
dotnet test IntegrationTests/IntegrationTests.csproj
```

### Usando o Explorador de Testes do Visual Studio

1. Abra a solução no Visual Studio
1. Abra o Explorador de Testes indo para Exibir > Explorador de Testes (ou pressione Ctrl+E, T)
1. Na janela do Explorador de Testes, você verá todos os testes na sua solução

![Explorador de Testes do Visual Studio](../media/vs-test-explorer.png)

1. Você pode:
   - Executar todos os testes clicando no botão "Executar Todos" no topo
   - Executar um teste específico clicando com o botão direito nele e selecionando "Executar"
   - Executar apenas testes falhados clicando no botão "Executar Testes Falhados"
   - Executar testes em modo debug clicando com o botão direito e selecionando "Depurar"
   - Visualizar resultados dos testes e saída na janela do Explorador de Testes

Os testes verificarão que:

- As variáveis de ambiente estão configuradas adequadamente
- Os endpoints da API estão funcionando corretamente
- A aplicação web está funcionando como esperado

Ao executar esses testes, todos os logs de recursos são redirecionados para a `DistributedApplication` por padrão. Este redirecionamento de log habilita cenários onde você quer afirmar que um recurso está logando corretamente, embora não estejamos fazendo isso nesses testes particulares.

## Ferramentas de Teste Adicionais

Playwright é uma ferramenta poderosa para testes de ponta a ponta. Permite automatizar interações do navegador e verificar se sua aplicação funciona como esperado da perspectiva do usuário. Playwright suporta múltiplos navegadores, incluindo Chromium, Firefox e WebKit.

### Caso de Uso

Playwright pode ser usado para realizar testes de ponta a ponta da sua aplicação web. Pode simular interações do usuário, como clicar em botões, preencher formulários e navegar entre páginas. Isso garante que sua aplicação se comporte corretamente em cenários do mundo real.

### Conceitos de Alto Nível

- **Automação de Navegador**: Playwright pode lançar e controlar navegadores para realizar testes automatizados.
- **Testes Cross-Browser**: Playwright suporta testes através de diferentes navegadores para garantir compatibilidade.
- **Modo Headless**: Playwright pode executar testes em modo headless, o que significa que o navegador roda em background sem uma interface gráfica do usuário.
- **Assertions**: Playwright fornece assertions incorporadas para verificar que elementos estão presentes, visíveis e têm as propriedades esperadas.

Para mais informações sobre Playwright, consulte a [documentação oficial](https://playwright.dev/dotnet/).

## Conclusão

Neste módulo, cobrimos testes de integração usando `Aspire.Hosting.Testing` com `MSTest`. Criamos um projeto de teste separado para testar tanto a API quanto a aplicação web, seguindo padrões similares à abordagem `WebApplicationFactory` no ASP.NET Core, mas adaptados para aplicações distribuídas.

Nossos testes verificaram três aspectos críticos da aplicação distribuída:

1. A funcionalidade da API (testando que endpoints retornam dados esperados)
1. A funcionalidade da aplicação web (testando que a UI renderiza corretamente)
1. O mecanismo de descoberta de serviços (testando que serviços podem se encontrar e comunicar entre si)

Para um mergulho mais profundo em testes com .NET Aspire, incluindo um walkthrough em vídeo, confira o post do blog [Getting started with testing and .NET Aspire](https://devblogs.microsoft.com/dotnet/getting-started-with-testing-and-dotnet-aspire/).

Agora, vamos aprender sobre opções de deploy ao usar .NET Aspire.

**Próximo**: [Módulo #9: Deployment](../Lesson-09-Deployment/README.md)
