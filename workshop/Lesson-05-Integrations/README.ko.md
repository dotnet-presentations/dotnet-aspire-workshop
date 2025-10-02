> 이 문서는 [Azure OpenAI](https://learn.microsoft.com/azure/ai-services/openai/overview)를 이용해 초벌 번역 후 검수를 진행했습니다. 따라서 번역 품질이 기대와 다를 수 있습니다. 문서 번역에 대해 제안할 내용이 있을 경우, [이슈](../../../issue)에 남겨주시면 확인후 반영하겠습니다.

# .NET Aspire 통합

.NET Aspire 컴포넌트는 Redis 및 PostgreSQL을 포함하여 주요 서비스 및 플랫폼과 클라우드 네이티브 애플리케이션의 통합을 용이하게 하기 위해 특별히 큐레이션한 NuGet 패키지 모음입니다. 각 컴포넌트는 자동 프로비저닝 또는 표준화된 구성 패턴을 통해 필수적인 클라우드 네이티브 기능을 제공합니다. .NET Aspire 컴포넌트는 AppHost(오케스트레이터) 프로젝트 없이도 사용할 수 있지만, .NET Aspire AppHost와 함께 사용할 때 가장 잘 작동하도록 만들어졌습니다.

.NET Aspire 컴포넌트는 .NET Aspire 호스팅 패키지와 혼동하지 않아야 합니다. 이들은 다른 목적을 가지고 있습니다. 호스팅 패키지는 .NET Aspire 앱에서 다양한 리소스를 모델링하고 구성하는 데 사용하는 반면, 컴포넌트는 다양한 클라이언트 라이브러리에 대한 구성을 매핑하는 데 사용합니다.

Microsoft 및 개발자 커뮤니티에서 만든 [.NET Aspire 컴포넌트](https://learn.microsoft.com/dotnet/aspire/fundamentals/components-overview?tabs=dotnet-cli#available-components) 목록은 계속 증가하고 있습니다. .NET Aspire는 유연하며 누구나 자신의 서비스를 통합하기 위해 자체 컴포넌트를 만들 수 있습니다.

애플리케이션의 성능을 개선하기 위해 컴포넌트를 추가해 보겠습니다. 우리는 API 성능을 개선하기 위해 Redis 캐시에 연결하는 컴포넌트를 추가할 것입니다.

## App Host에 Redis 컴포넌트 추가

애플리케이션에 통합할 수 있는 두 가지 유형의 캐싱이 있습니다:

- **아웃풋 캐싱**: 전체 HTTP 응답을 향후 요청을 위해 저장하는 구성 가능하고 확장 가능한 캐싱 방법입니다.
- **분산 캐싱**: 여러 앱 서버에서 공유하는 캐시로, 특정 데이터 조각을 캐시할 수 있습니다. 분산 캐시는 일반적으로 이를 액세스하는 앱 서버 외부의 서비스로 유지되며 ASP.NET Core 앱의 성능과 확장성을 향상시킬 수 있습니다.

우리는 _아웃풋 캐싱_ 컴포넌트를 AppHost에 통합할 것입니다. 이 컴포넌트는 Redis 캐시에 API 응답을 캐시하는 데 도움을 줄 것입니다.

AppHost에 Redis 컴포넌트를 추가하려면 `Aspire.Hosting.Redis` NuGet 패키지를 설치해야 합니다. 이 패키지는 AppHost에서 서비스를 구성하는 데 필요한 컴포넌트를 제공합니다. Redis는 이 워크숍에서 컨테이너 이미지로 제공하며, .NET Aspire AppHost 프로젝트를 시작하면 자동으로 Redis 컨테이너 이미지를 다운로드하고 Redis 서버를 시작합니다.

NuGet을 설치하면 이를 구성할 수 있습니다.

1. `AppHost` 프로젝트의 `Program.cs` 파일을 엽니다.
1. `var builder = DistributedApplication.CreateBuilder(args);` 아래에 다음 코드를 추가합니다:

    ```csharp
    var cache = builder.AddRedis("cache")
    ```

    여기서 우리는 `cache`라는 이름으로 Redis 캐시를 구성했습니다. 이 이름은 `Api` 또는 `MyWeatherHub`에서 캐시를 식별하는 데 쓰입니다.

1. App Host에서 `api`를 업데이트하여 캐시에 대한 참조를 추가합니다.

    ```csharp
    var api = builder.AddProject<Projects.Api>("api")
                     .WithReference(cache);
    ```

1. 또한, Redis 관리 도구인 [Redis Commander](https://joeferner.github.io/redis-commander/)를 구성할 수 있습니다. `Aspire.Hosting.Redis` 패키지의 일부로, Redis Commander는 동일한 컴포넌트 내에서 사용할 수 있습니다. Redis Commander를 추가하려면 새로 추가된 Redis 구성 아래에 다음 코드를 추가합니다.

    ```csharp
    var cache = builder.AddRedis("cache")
                       .WithRedisCommander();
    ```

## 애플리케이션 실행 (Redis 시작)

우리는 `Api`나 `MyWeatherHub` 프로젝트에 아무런 변경을 하지 않았지만, AppHost 프로젝트를 시작하면 Redis 캐시가 함께 시작하는 것을 볼 수 있습니다.

> [!IMPORTANT]
> Redis는 컨테이너에서 동작하므로 Docker가 컴퓨터에서 실행 중인지 확인해야 합니다.

1. Docker Desktop 또는 Podman을 시작합니다.
1. AppHost 프로젝트를 시작합니다.
1. 대시보드와 Docker Desktop에서 Redis 컨테이너와 Redis Commander 컨테이너를 다운로드한 후 시작하는 것을 볼 수 있습니다.

    ![대시보드와 데스크탑에서 실행 중인 Redis](.../media/redis-started.png)

## API에 아웃풋 캐싱 통합

1. `Api` 프로젝트에 `Aspire.StackExchange.Redis.OutputCaching` NuGet 패키지를 설치하여 Redis API에 접근할 수 있도록 합니다.
1. `Api` 프로젝트의 `Program.cs` 파일을 엽니다.
1. 파일 상단의 `var builder = WebApplication.CreateBuilder(args);` 아래에 다음 코드를 추가합니다:

    ```csharp
    builder.AddRedisOutputCache("cache");
    ```

    > 우리는 AppHost 프로젝트에서 구성한 Redis 캐시를 참조하기 위해 "cache" 이름을 사용하고 있습니다.

1. `NwsManager`는 이미 메모리 캐시를 사용하여 출력 캐싱을 구성했습니다. 이를 Redis 캐시를 사용하도록 업데이트하겠습니다. `Data` 폴더의 `NwsManager.cs` 파일을 엽니다.
1. `NwsManagerExtensions` 클래스에서 `AddNwsManager` 메서드를 찾습니다.
1. 다음 코드를 **삭제**합니다:

    ```csharp
    // 기본 출력 캐싱 추가
    services.AddOutputCache(options =>
    {
        options.AddBasePolicy(builder => builder.Cache());
    });
    ```

    우리는 `Program.cs` 파일에서 애플리케이션이 Redis 캐시를 사용하도록 구성했기 때문에 기본 출력 캐싱 정책을 추가할 필요가 없습니다.

## 애플리케이션 실행 (Output Caching 테스트)

1. AppHost 프로젝트를 시작하고 대시보드에서 `MyWeatherHub` 프로젝트를 엽니다.
1. 도시를 클릭한 다음 다시 클릭합니다. 응답이 캐시되고 `Traces` 탭에서 두 번째 요청이 첫 번째 요청보다 훨씬 빠른 것을 확인할 수 있습니다.

    ![출력 캐싱 작동](.../media/output-caching.png)

1. Redis Commander에서 캐시된 응답도 볼 수 있습니다. 대시보드에서 `Redis Commander` 엔드포인트를 클릭하여 Redis Commander를 엽니다. 통계 아래에서 연결 및 처리된 명령을 볼 수 있습니다.

    ![Redis Commander](.../media/redis-commander.png)
1. 또한, `Console` 탭에서 Redis 캐시와 Redis Commander에 대한 로그를 볼 수 있습니다.

    ![Redis 로그](.../media/redis-logs.png)

## 사용자 정의 Redis 컨테이너

.NET Aspire 컴포넌트는 유연하고 커스터마이징이 가능합니다. 기본적으로 Redis 컴포넌트는 Docker Hub의 Redis 컨테이너 이미지를 사용합니다. 그러나 `AddRedis` 메서드 후에 이미지 이름과 태그를 제공하여 사용자 정의 Redis 컨테이너 이미지를 사용할 수 있습니다. 예를 들어, [Garnet](https://github.com/microsoft/garnet)과 같은 사용자 정의 Redis 컨테이너 이미지가 있는 경우, App Host에서 다음과 같이 이미지 이름과 태그를 제공할 수 있습니다:

```csharp
var cache = builder.AddRedis("cache")
                   .WithImage("ghcr.io/microsoft/garnet")
                   .WithImageTag("latest")
                   .WithRedisCommander();
```

1. 애플리케이션을 실행하면 이제 대시보드와 Docker Desktop에서 Garnet이 동작하는 것을 볼 수 있습니다.

    ![대시보드와 데스크탑에서 실행 중인 Garnet](.../media/garnet-started.png)
1. 또한, `콘솔` 탭에서 Garnet의 로그를 볼 수 있습니다.

    ![Garnet 로그](.../media/garnet-logs.png)

## 요약

이 섹션에서는 AppHost 프로젝트에 Redis 컴포넌트를 추가하고 API에 출력 캐싱을 통합했습니다. 우리는 응답이 Redis 캐시에 캐시되고 두 번째 요청이 첫 번째 요청보다 훨씬 빠른 것을 확인했습니다. 또한, Redis Commander를 사용하여 Redis 캐시를 관리하는 방법도 보았습니다.

지금 당장 사용할 수 있는 많은 다른 컴포넌트가 있으며 이를 사용하여 서비스와 통합할 수 있습니다. 사용할 수 있는 컴포넌트 목록은 [.NET Aspire 문서](https://learn.microsoft.com/dotnet/aspire/fundamentals/components-overview?tabs=dotnet-cli#available-components)에서 찾을 수 있습니다.

다음 단계로는 데이터베이스를 통합하거나 Azure Redis Cache를 호스팅 솔루션으로 사용하는 것을 자연스럽게 고려할 수 있습니다. 이러한 컴포넌트는 NuGet에서 사용할 수 있습니다.
