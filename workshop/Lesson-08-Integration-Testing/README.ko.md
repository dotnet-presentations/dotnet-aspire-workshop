# .NET Aspire를 이용한 통합 테스트

## 소개

이 모듈에서는 `MSTest`와 함께 `Aspire.Hosting.Testing`을 사용한 통합 테스트를 다룹니다. 통합 테스트는 애플리케이션의 서로 다른 부분들이 예상대로 함께 작동하는지 확인하는 데 중요합니다. API와 웹 애플리케이션을 모두 테스트하기 위해 별도의 테스트 프로젝트를 생성할 것입니다.

## 단위 테스트와 통합 테스트의 차이점

단위 테스트는 개별 구성 요소나 코드 단위를 격리된 상태에서 테스트하는 데 중점을 둡니다. 각 단위가 개별적으로 올바르게 기능하는지 확인합니다. 반대로 통합 테스트는 애플리케이션의 서로 다른 구성 요소들이 예상대로 함께 작동하는지 검증합니다. API, 데이터베이스, 웹 애플리케이션과 같은 시스템의 다양한 부분 간의 상호 작용을 테스트합니다.

.NET Aspire를 사용한 분산 애플리케이션의 맥락에서 통합 테스트는 서로 다른 서비스와 구성 요소가 올바르게 통신하고 함께 기능하는지 확인하는 데 필수적입니다.

## 통합 테스트 프로젝트 생성

1. `IntegrationTests`라는 이름의 새 테스트 프로젝트를 생성합니다.
1. `IntegrationTests.csproj` 파일에 필요한 패키지에 대한 참조를 추가합니다:

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

이 프로젝트 파일은 테스트 프로젝트로서는 상당히 표준적입니다. 주요 요소들은 다음과 같습니다:

- [Aspire.Hosting.Testing](https://www.nuget.org/packages/Aspire.Hosting.Testing) NuGet 패키지에 대한 `PackageReference`로, .NET Aspire 애플리케이션을 테스트하는 데 필요한 타입과 API를 제공합니다.
- AppHost 프로젝트에 대한 `ProjectReference`로, 테스트 프로젝트가 대상 분산 애플리케이션 정의에 액세스할 수 있게 합니다.
- 테스트 프로젝트가 네이티브 MSTest 러너로 실행되도록 구성하는 `EnableMSTestRunner`와 `OutputType` 설정.

> 참고: 이 워크샵에서는 MSTest 3.x 버전이면 어느 것이든 괜찮습니다. 환경에서 더 새로운 3.x를 제공한다면 그것을 사용할 수 있습니다.

1. 통합 테스트용 테스트 클래스를 생성합니다:

`IntegrationTests.cs` 파일은 API와 웹 애플리케이션 기능을 테스트합니다:

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

이 테스트 클래스는 분산 애플리케이션을 테스트하는 방법을 보여줍니다. 이 테스트들이 무엇을 하는지 살펴보겠습니다:

- 두 테스트 모두 유사한 패턴을 따르며, `DistributedApplicationTestingBuilder.CreateAsync<Projects.AppHost>()`를 사용하여 애플리케이션 호스트의 인스턴스를 비동기적으로 생성합니다.
- `appHost`는 표준 HTTP 복원력 핸들러로 구성되어 더 견고한 HTTP 통신을 위한 재시도 정책과 회로 차단기를 제공합니다.
- 테스트는 `appHost.BuildAsync()`를 호출하여 애플리케이션을 빌드한 다음 DI 컨테이너에서 `ResourceNotificationService`를 검색합니다.
- `app.StartAsync()`로 앱을 시작한 후, 테스트되는 리소스("api" 또는 "myweatherhub")를 위해 특별히 `HttpClient`가 생성됩니다.
- 테스트는 대상 리소스가 "Running" 상태에 도달할 때까지 기다린 후 진행하여 서비스가 요청을 수락할 준비가 되었음을 보장합니다.
- 마지막으로 특정 엔드포인트에 HTTP 요청이 이루어지고 어설션이 응답을 검증합니다.

첫 번째 테스트에서는 API의 `/zones` 엔드포인트가 유효한 영역 데이터 컬렉션을 반환하는지 검증합니다. 두 번째 테스트에서는 웹 애플리케이션의 홈페이지가 성공적으로 로드되고 예상되는 콘텐츠를 포함하는지 확인합니다.

`EnvVarTests.cs` 파일은 환경 변수 해결을 검증합니다:

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

이 테스트는 서비스 검색 구성 검증에 중점을 둡니다:

- `DistributedApplicationTestingBuilder.CreateAsync<Projects.AppHost>()`를 사용하여 애플리케이션 호스트의 인스턴스를 생성합니다.
- 애플리케이션을 시작하는 대신 웹 프론트엔드("myweatherhub")를 나타내는 `IResourceWithEnvironment` 인스턴스를 직접 검색합니다.
- `DistributedApplicationOperation.Publish` 인수와 함께 `GetEnvironmentVariableValuesAsync()`를 호출하여 리소스에 게시될 환경 변수를 가져옵니다.
- 마지막으로 웹 프론트엔드가 API 서비스의 URL로 해결되는 환경 변수를 가지고 있는지 어설션하여 서비스 검색이 올바르게 구성되었음을 확인합니다.

이 테스트는 애플리케이션의 서비스가 환경 변수를 통해 올바르게 연결되어 있는지 검증하므로 특히 가치가 있습니다. 이는 .NET Aspire가 분산 애플리케이션에서 서비스 검색을 처리하는 방법입니다.

> 참고: 완성된 솔루션에서 비어있는 `WeatherBackgroundTests.cs` 파일을 보게 된다면, 이는 향후 백그라운드 작업 테스트를 위한 플레이스홀더이며 이 워크샵에서는 무시할 수 있습니다.

## 통합 테스트 실행

### 명령줄 사용

1. 터미널을 열고 `complete` 폴더로 이동합니다.
1. `dotnet test` 명령을 사용하여 통합 테스트를 실행합니다:

```bash
dotnet test IntegrationTests/IntegrationTests.csproj
```

### Visual Studio 테스트 탐색기 사용

1. Visual Studio에서 솔루션을 엽니다
1. 보기 > 테스트 탐색기로 이동하여(또는 Ctrl+E, T를 누름) 테스트 탐색기를 엽니다
1. 테스트 탐색기 창에서 솔루션의 모든 테스트를 볼 수 있습니다

![Visual Studio 테스트 탐색기](../media/vs-test-explorer.png)

1. 다음을 할 수 있습니다:
   - 상단의 "모두 실행" 버튼을 클릭하여 모든 테스트 실행
   - 특정 테스트를 마우스 오른쪽 버튼으로 클릭하고 "실행"을 선택하여 실행
   - "실패한 테스트 실행" 버튼을 클릭하여 실패한 테스트만 실행
   - 마우스 오른쪽 버튼을 클릭하고 "디버그"를 선택하여 디버그 모드에서 테스트 실행
   - 테스트 탐색기 창에서 테스트 결과와 출력 보기

테스트는 다음을 검증할 것입니다:

- 환경 변수가 올바르게 구성되어 있음
- API 엔드포인트가 올바르게 작동하고 있음
- 웹 애플리케이션이 예상대로 기능하고 있음

이러한 테스트를 실행할 때 모든 리소스 로그는 기본적으로 `DistributedApplication`으로 리다이렉트됩니다. 이 로그 리다이렉션은 리소스가 올바르게 로깅하고 있다고 어설션하려는 시나리오를 가능하게 하지만, 이러한 특정 테스트에서는 그렇게 하지 않습니다.

## 추가 테스트 도구

Playwright는 엔드투엔드 테스트를 위한 강력한 도구입니다. 브라우저 상호작용을 자동화하고 애플리케이션이 사용자 관점에서 예상대로 작동하는지 검증할 수 있게 해줍니다. Playwright는 Chromium, Firefox, WebKit을 포함한 여러 브라우저를 지원합니다.

### 사용 사례

Playwright는 웹 애플리케이션의 엔드투엔드 테스트를 수행하는 데 사용할 수 있습니다. 버튼 클릭, 양식 작성, 페이지 간 탐색과 같은 사용자 상호작용을 시뮬레이션할 수 있습니다. 이는 애플리케이션이 실제 시나리오에서 올바르게 동작함을 보장합니다.

### 고수준 개념

- **브라우저 자동화**: Playwright는 브라우저를 시작하고 제어하여 자동화된 테스트를 수행할 수 있습니다.
- **크로스 브라우저 테스트**: Playwright는 호환성을 보장하기 위해 다양한 브라우저에서의 테스트를 지원합니다.
- **헤드리스 모드**: Playwright는 헤드리스 모드에서 테스트를 실행할 수 있으며, 이는 브라우저가 그래픽 사용자 인터페이스 없이 백그라운드에서 실행됨을 의미합니다.
- **어설션**: Playwright는 요소가 존재하고, 표시되며, 예상되는 속성을 가지고 있는지 검증하기 위한 내장 어설션을 제공합니다.

Playwright에 대한 자세한 정보는 [공식 문서](https://playwright.dev/dotnet/)를 참조하세요.

## 결론

이 모듈에서는 `MSTest`와 함께 `Aspire.Hosting.Testing`을 사용한 통합 테스트를 다뤘습니다. API와 웹 애플리케이션을 모두 테스트하기 위해 별도의 테스트 프로젝트를 생성했으며, ASP.NET Core의 `WebApplicationFactory` 접근 방식과 유사하지만 분산 애플리케이션에 맞게 조정된 패턴을 따랐습니다.

우리의 테스트는 분산 애플리케이션의 세 가지 중요한 측면을 검증했습니다:

1. API 기능 (엔드포인트가 예상되는 데이터를 반환하는지 테스트)
1. 웹 애플리케이션 기능 (UI가 올바르게 렌더링되는지 테스트)
1. 서비스 검색 메커니즘 (서비스가 서로를 찾고 통신할 수 있는지 테스트)

비디오 안내를 포함하여 .NET Aspire로 테스트하는 것에 대한 더 깊은 이해를 위해서는 블로그 포스트 [Getting started with testing and .NET Aspire](https://devblogs.microsoft.com/dotnet/getting-started-with-testing-and-dotnet-aspire/)를 확인하세요.

이제 .NET Aspire를 사용할 때의 배포 옵션에 대해 알아보겠습니다.

**다음**: [모듈 #9: 배포](../Lesson-09-Deployment/README.md)
