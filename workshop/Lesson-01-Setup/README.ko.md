> 이 문서는 [Azure OpenAI](https://learn.microsoft.com/azure/ai-services/openai/overview)를 이용해 초벌 번역 후 검수를 진행했습니다. 따라서 번역 품질이 기대와 다를 수 있습니다. 문서 번역에 대해 제안할 내용이 있을 경우, [이슈](../../../issue)에 남겨주시면 확인후 반영하겠습니다.

# 머신 설정

이 워크숍에서는 다음 도구들을 사용합니다:

- [.NET 9 SDK](https://get.dot.net/9) 또는 [.NET 10 Preview](https://get.dot.net/10) (선택사항)
- [Docker Desktop](https://docs.docker.com/engine/install/) 또는 [Podman](https://podman.io/getting-started/installation)
- [Visual Studio 2022](https://visualstudio.microsoft.com/vs/) 또는 [Visual Studio Code](https://code.visualstudio.com/)와 [C# Dev Kit](https://code.visualstudio.com/docs/csharp/get-started)

최고의 경험을 위해서는 .NET Aspire 워크로드가 포함된 Visual Studio 2022를 사용하는 것을 권장합니다. 하지만, C# Dev Kit과 .NET Aspire 워크로드가 포함된 Visual Studio Code를 사용할 수도 있습니다. 각 플랫폼에 대한 설정 가이드는 아래에 있습니다.

> **.NET Aspire 9.4의 새로운 기능**: .NET 10 Preview 완전 지원! 이제 `dotnet new aspire --framework net10.0`을 사용하여 .NET 10을 대상으로 하는 Aspire 프로젝트를 만들 수 있습니다

## Windows 환경에서 Visual Studio 사용하기

- [Visual Studio 2022 버전 17.14 이상](https://visualstudio.microsoft.com/vs/)을 설치합니다.
  - [무료 Visual Studio Community](https://visualstudio.microsoft.com/free-developer-offers/)를 포함한 모든 에디션이 작동합니다
  - `ASP.NET 및 웹 개발` 워크로드를 선택합니다.

## Mac, Linux, Windows 환경에서 Visual Studio 없이 사용하기

- 최신 [.NET 9 SDK](https://get.dot.net/9?cid=eshop)를 설치합니다.

- [C# Dev Kit을 포함한 Visual Studio Code](https://code.visualstudio.com/docs/csharp/get-started)를 설치합니다.

> 참고: Apple Silicon (M 시리즈 프로세서)을 사용하는 Mac에서 실행할 때는 grpc-tools를 위해 Rosetta 2가 필요합니다.

## 최신 .NET Aspire 템플릿 설치

다음 명령을 실행하여 최신 .NET Aspire 템플릿을 설치합니다.

```cli
dotnet new install Aspire.ProjectTemplates --force
```

## .NET Aspire CLI 설치 (선택사항)

.NET Aspire 9.4는 일반적으로 사용 가능한 Aspire CLI를 도입하여 간소화된 개발자 경험을 제공합니다. 다음 방법 중 하나를 사용하여 설치할 수 있습니다:

### 빠른 설치 (권장)

```bash
# Windows (PowerShell)
irm https://aspire.dev/install.ps1 | iex

# macOS/Linux (Bash)
curl -sSL https://aspire.dev/install.sh | bash
```

### .NET 글로벌 도구

```cli
dotnet tool install -g Aspire.Cli
```

Aspire CLI는 다음과 같은 유용한 명령을 제공합니다:

- `aspire new` - 새로운 Aspire 프로젝트 생성
- `aspire run` - 리포지토리의 어디서든 AppHost를 찾아 실행
- `aspire add` - 호스팅 통합 패키지 추가
- `aspire config` - Aspire 설정 구성
- `aspire publish` - 배포 아티팩트 생성

## 설치 테스트

설치를 테스트해 보려면, [첫 .NET Aspire 프로젝트 빌드하기](https://learn.microsoft.com/dotnet/aspire/get-started/build-your-first-aspire-app)에서 자세한 정보를 참조하세요.

## 워크숍 시작 솔루션 열기

워크숍을 시작하려면 Visual Studio 2022에서 `start/MyWeatherHub.sln`을 엽니다. Visual Studio Code를 사용하는 경우 `start` 폴더를 열고 C# Dev Kit에서 어떤 솔루션을 열지 묻는 메시지가 나타나면 **MyWeatherHub.sln**을 선택합니다.

**다음**: [모듈 #2 - Service Defaults](../Lesson-02-ServiceDefaults/README.md)
