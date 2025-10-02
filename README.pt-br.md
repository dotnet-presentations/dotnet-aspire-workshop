# Vamos aprender sobre .NET Aspire

Venha aprender tudo sobre o [.NET Aspire](https://learn.microsoft.com/dotnet/aspire/), um conjunto de ferramentas preparado para a nuvem, ideal para construir aplicações distribuídas, fáceis de monitorar e prontas para produção. O .NET Aspire pode ser adicionado a qualquer aplicação, independentemente do tamanho e da escala, ajudando você a desenvolver melhores aplicações mais rapidamente.

Este workshop utiliza **.NET Aspire 9.4** e é projetado para **.NET 9** (**.NET 8** também é suportado).

O .NET Aspire simplifica o desenvolvimento de aplicações com:

- **Orquestração**: Orquestração integrada com um fluxo de trabalho simples, mas poderoso. Use C# e APIs familiares sem uma linha de YAML. Adicione facilmente serviços populares na nuvem, conecte-os aos seus projetos e execute localmente com um único clique.
- **Identificação de serviços**: Injeção automática das informações de conexão ou configurações de rede corretas, além de informações de serviços para simplificar a experiência do desenvolvedor.
- **Componentes**: Componentes nativos e integrados para serviços comuns na nuvem, como bancos de dados, filas e armazenamento. Integrado com logs, verificações de integridade, telemetria e muito mais.
- **Dashboard**: Veja dados ao vivo do OpenTelemetry sem necessidade de configuração. Iniciado por padrão ao executar, o painel do desenvolvedor do .NET Aspire mostra logs, variáveis de ambiente, rastreamentos (traces) distribuídos, métricas e mais para verificar rapidamente o comportamento do aplicativo.
- **Implantação**: Gerencia a injeção das informações de conexão ou configurações de rede corretas e informações de serviços para simplificar a experiência do desenvolvedor.
- **E muito mais**: O .NET Aspire está repleto de recursos que os desenvolvedores vão adorar e que ajudarão a aumentar sua produtividade.

Saiba mais sobre o .NET Aspire com os seguintes recursos (em Inglês):

- [Documentação](https://learn.microsoft.com/dotnet/aspire)
- [Treinamento do Microsoft Learn](https://learn.microsoft.com/training/paths/dotnet-aspire/)
- [Vídeos do .NET Aspire](https://aka.ms/aspire/videos)
- [Aplicativo de exemplo e referência eShop](https://github.com/dotnet/eshop)
- [Exemplos do .NET Aspire](https://learn.microsoft.com/samples/browse/?expanded=dotnet&products=dotnet-aspire)
- [Perguntas frequentes do .NET Aspire](https://learn.microsoft.com/dotnet/aspire/reference/aspire-faq)

## Localização

Os materiais deste workshop estão disponíveis nos seguintes idiomas:

- [Inglês](./README.md)
- [简体中文](./README.zh-cn.md)
- [한국어](./README.ko.md)
- [日本語](./README.jp.md)
- [Español](./README.es.md)
- [Français](./README.fr.md)
- [Português (PT-BR)](./README.pt-br.md)

Você também pode assistir aos eventos ao vivo Let's Learn .NET Aspire nos seguintes idiomas:

- [Inglês](https://www.youtube.com/watch?v=8i3FaHChh20)
- [한국어](https://www.youtube.com/watch?v=rTpNgMaVM6g)
- [日本語](https://www.youtube.com/watch?v=Cm7mqHZJIgc)
- [Español](https://www.youtube.com/watch?v=dd1Mc5bQZSo)
- [Français](https://www.youtube.com/watch?v=jJiqqVPDN4w)
- [Português (PT-BR)](https://www.youtube.com/watch?v=PUCU9ZOOgQ8)
- [Tiếng Việt](https://www.youtube.com/watch?v=48CWnYfTZhk)

## Workshop

Este workshop do .NET Aspire faz parte da série [Vamos aprender .NET](https://aka.ms/letslearndotnet). Este workshop foi criado para ajudá-lo a aprender sobre o .NET Aspire e como usá-lo para construir aplicações prontas para a nuvem.

### Pré-requisitos

Antes de começar este workshop, certifique-se de ter:

- [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0) (recomendado) ou [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Visual Studio 2022](https://visualstudio.microsoft.com/vs/) ou [Visual Studio Code](https://code.visualstudio.com/) com a extensão C#
- [Docker Desktop](https://www.docker.com/products/docker-desktop/) (para recursos containerizados)

### Módulos do Workshop

Este workshop é dividido em 15 módulos (tempo estimado de conclusão: 4-6 horas):

1. [Configuração e Instalação](./workshop/Lesson-01-Setup/README.md)
1. [Padrões de Serviço](./workshop/Lesson-02-ServiceDefaults/README.md)
1. [Painel de Desenvolvimento e Orquestração](./workshop/Lesson-03-Dashboard-AppHost/README.md)
1. [Descoberta de Serviços](./workshop/Lesson-04-ServiceDiscovery/README.md)
1. [Integrações](./workshop/Lesson-05-Integrations/README.md)
1. [Módulo de Telemetria](./workshop/Lesson-06-Telemetry/README.md)
1. [Módulo de Banco de Dados](./workshop/Lesson-07-Database/README.md)
1. [Testes de Integração](./workshop/Lesson-08-Integration-Testing/README.md)
1. [Implantação](./workshop/Lesson-09-Deployment/README.md)
1. [Gerenciamento de Contêineres](./workshop/Lesson-10-Container-Management/README.md)
1. [Integrações Azure](./workshop/Lesson-11-Azure-Integrations/README.md)
1. [Comandos Personalizados](./workshop/Lesson-12-Custom-Commands/README.md)
1. [Verificações de Saúde](./workshop/Lesson-13-HealthChecks/README.md)
1. [Integração GitHub Models](./workshop/Lesson-14-GitHub-Models-Integration/README.md)
1. [Integração Docker](./workshop/Lesson-15-Docker-Integration/README.md)

Um conjunto completo de [slides](./workshop/AspireWorkshop.pptx) está disponível para este workshop.

### Começando

O projeto inicial para este workshop está localizado na pasta `start`. Este projeto é uma API de clima simples que usa a API do Serviço Nacional de Meteorologia dos Estados Unidos (NWS) para obter dados meteorológicos e uma interface web para exibir os dados do clima alimentada por Blazor.

Para começar o workshop:

1. Navegue até a pasta `start`
2. Abra o arquivo de solução `MyWeatherHub.sln`
3. Siga as instruções em [Módulo 1: Configuração e Instalação](./workshop/Lesson-01-Setup/README.md)

## Dados de demonstração

Os dados e o serviço usados para este tutorial vêm do Serviço Nacional de Meteorologia dos Estados Unidos (NWS) em <https://weather.gov>. Estamos usando sua especificação OpenAPI para consultar previsões meteorológicas. A especificação OpenAPI está [disponível online](https://www.weather.gov/documentation/services-web-api). Estamos usando apenas 2 métodos dessa API, e simplificamos nosso código para usar apenas esses métodos em vez de criar o cliente OpenAPI inteiro para a API NWS.
