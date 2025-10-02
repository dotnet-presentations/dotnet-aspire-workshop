# Configuração da Máquina

Este workshop utilizará as seguintes ferramentas:

- [SDK do .NET 9](https://get.dot.net/9) ou [.NET 10 Preview](https://get.dot.net/10) (opcional)
- [Docker Desktop](https://docs.docker.com/engine/install/) ou [Podman](https://podman.io/getting-started/installation)
- [Visual Studio 2022](https://visualstudio.microsoft.com/vs/) ou [Visual Studio Code](https://code.visualstudio.com/) com [C# Dev Kit](https://code.visualstudio.com/docs/csharp/get-started)

Para uma experiência melhor, recomendamos usar o Visual Studio 2022 com o workload .NET Aspire. No entanto, você pode usar o Visual Studio Code com o C# Dev Kit e o workload .NET Aspire. A seguir estão os passos para configuração de cada plataforma.

> **Novo no .NET Aspire 9.4**: Suporte completo para .NET 10 Preview! Agora você pode criar projetos Aspire direcionados ao .NET 10 usando `dotnet new aspire --framework net10.0`

## Windows com Visual Studio

- Instale o [Visual Studio 2022 versão 17.14 ou mais recente](https://visualstudio.microsoft.com/vs/).
  - Qualquer edição funciona, incluindo a [Visual Studio Community gratuita](https://visualstudio.microsoft.com/free-developer-offers/)
  - Selecione o workload `ASP.NET and web development`.

## Mac, Linux e Windows sem Visual Studio

- Instale o último [SDK do .NET 9](https://get.dot.net/9?cid=eshop)

- Instale o [Visual Studio Code com C# Dev Kit](https://code.visualstudio.com/docs/csharp/get-started)

> Nota: Ao executar no Mac com processador Apple Silicon (série M), Rosetta 2 para grpc-tools.

## Instalar os Templates .NET Aspire Mais Recentes

Execute o seguinte comando para instalar os templates mais recentes do .NET Aspire.

```cli
dotnet new install Aspire.ProjectTemplates --force
```

## Instalar a CLI do .NET Aspire (Opcional)

O .NET Aspire 9.4 introduz a CLI do Aspire geralmente disponível, proporcionando uma experiência de desenvolvedor simplificada. Você pode instalá-la usando um destes métodos:

### Instalação Rápida (Recomendada)

```bash
# Windows (PowerShell)
iex "& { $(irm https://aspire.dev/install.ps1) }"

# macOS/Linux (Bash)
curl -sSL https://aspire.dev/install.sh | bash
```

### Ferramenta Global .NET

```cli
dotnet tool install -g Aspire.Cli
```

A CLI do Aspire oferece comandos úteis como:

- `aspire new` - Criar novos projetos Aspire
- `aspire run` - Encontrar e executar o AppHost de qualquer lugar em seu repositório
- `aspire add` - Adicionar pacotes de integração de hospedagem
- `aspire config` - Configurar definições do Aspire
- `aspire publish` - Gerar artefatos de implantação

## Teste de Instalação

Para testar sua instalação, consulte [Construa seu primeiro projeto .NET Aspire](https://learn.microsoft.com/dotnet/aspire/get-started/build-your-first-aspire-app) para mais informações.

## Abrir a Solução de Início do Workshop

Para iniciar o workshop, abra `start/MyWeatherHub.sln` no Visual Studio 2022. Se você estiver usando o Visual Studio Code, abra a pasta `start` e quando solicitado pelo C# Dev Kit sobre qual solução abrir, selecione **MyWeatherHub.sln**.

**Próximo**: [Módulo #2 - Service Defaults](../Lesson-02-ServiceDefaults/README.md)
