# 데이터베이스 통합

## 소개

이 모듈에서는 PostgreSQL 데이터베이스를 애플리케이션과 통합합니다. 데이터베이스와 상호 작용하기 위해 Entity Framework Core(EF Core)를 사용합니다. 또한 PostgreSQL 데이터베이스를 관리하기 위해 PgAdmin을 설정합니다.

## PostgreSQL 설정

.NET Aspire는 `Aspire.Hosting.PostgreSQL` 패키지를 통해 PostgreSQL에 대한 기본 지원을 제공합니다. PostgreSQL을 설정하려면:

1. AppHost 프로젝트에 필요한 NuGet 패키지를 설치합니다:

```xml
<PackageReference Include="Aspire.Hosting.PostgreSQL" Version="9.4.2" />
```

1. AppHost의 Program.cs를 업데이트하여 PostgreSQL을 추가합니다:

```csharp
var postgres = builder.AddPostgres("postgres")
    .WithDataVolume(isReadOnly: false);

var weatherDb = postgres.AddDatabase("weatherdb");
```

`WithDataVolume(isReadOnly: false)` 구성은 컨테이너 재시작 간에 데이터가 지속되도록 보장합니다. 데이터는 컨테이너 외부에 존재하는 Docker 볼륨에 저장되어 컨테이너 재시작을 견딜 수 있습니다. 이것은 워크샵에서 선택 사항입니다—이를 생략해도 샘플은 여전히 실행됩니다. 단지 실행 간에 데이터가 유지되지 않을 뿐입니다.

### .NET Aspire 9.4의 새로운 기능: 향상된 데이터베이스 초기화

.NET Aspire 9.4는 모든 데이터베이스 제공자에 대해 개선된 `WithInitFiles()` 메서드를 도입하여 더 복잡한 `WithInitBindMount()` 메서드를 대체합니다:

```csharp
var postgres = builder.AddPostgres("postgres")
    .WithDataVolume(isReadOnly: false)
    .WithInitFiles("./database-init");  // 파일에서 간소화된 초기화
```

이 메서드는 모든 데이터베이스 제공자(PostgreSQL, MySQL, MongoDB, Oracle)에서 일관되게 작동하며 더 나은 오류 처리와 간소화된 구성을 제공합니다. `WithInitFiles` 사용은 이 워크샵에서 선택 사항입니다. 데이터베이스 통합은 이것 없이도 작동합니다.

적절한 애플리케이션 시작을 보장하기 위해 웹 애플리케이션이 데이터베이스를 기다리도록 구성합니다:

```csharp
var web = builder.AddProject<Projects.MyWeatherHub>("myweatherhub")
    .WithReference(weatherDb)
    .WaitFor(postgres)  // 앱 시작 전에 데이터베이스 준비 완료 보장
    .WithExternalHttpEndpoints();
```

### 데이터베이스에 컨테이너 지속성 추가

여러 애플리케이션 실행에 걸쳐 데이터베이스 컨테이너와 데이터를 지속하려는 개발 시나리오의 경우 컨테이너 수명을 구성할 수 있습니다:

```csharp
var postgres = builder.AddPostgres("postgres")
    .WithDataVolume(isReadOnly: false)
    .WithLifetime(ContainerLifetime.Persistent);  // 앱 재시작 간 컨테이너 지속

var weatherDb = postgres.AddDatabase("weatherdb");
```

`ContainerLifetime.Persistent`를 사용하면 Aspire 애플리케이션을 중지해도 PostgreSQL 컨테이너가 계속 실행됩니다. 이는 선택 사항이며 모듈을 완료하는 데 필요하지 않습니다. 활성화하면 다음을 의미합니다:

- **빠른 시작 시간**: 후속 실행에서 PostgreSQL 초기화를 기다릴 필요가 없습니다
- **데이터 지속성**: 애플리케이션 세션 간에 데이터베이스 데이터가 그대로 유지됩니다
- **일관된 개발**: 데이터베이스가 마지막으로 남겨둔 상태와 동일한 상태를 유지합니다

> **참고**: 지속적 컨테이너는 주로 개발 시나리오에 유용합니다. 프로덕션 배포에서는 일반적으로 지속성을 자동으로 처리하는 관리형 데이터베이스 서비스를 사용합니다.
>
> **고급 컨테이너 기능**: .NET Aspire는 더 나은 시작 조정을 위한 `WithExplicitStart()`와 초기화 스크립트 마운트를 위한 `WithContainerFiles()`와 같은 고급 컨테이너 구성도 지원합니다. 이러한 기능은 복잡한 개발 시나리오에 필요할 때 컨테이너 동작에 대한 세밀한 제어를 제공합니다. 이러한 고급 기능에 대해 자세히 알아보려면 공식 문서의 [볼륨을 사용한 데이터 지속성](https://learn.microsoft.com/dotnet/aspire/fundamentals/persist-data-volumes) 및 [컨테이너 리소스 수명 주기](https://learn.microsoft.com/dotnet/aspire/fundamentals/app-host-overview#container-resource-lifecycle)를 참조하세요.

## EF Core와 PostgreSQL 통합

1. 웹 애플리케이션에 필요한 NuGet 패키지를 설치합니다:

```xml
<PackageReference Include="Aspire.Npgsql.EntityFrameworkCore.PostgreSQL" Version="9.4.2" />
```

1. DbContext 클래스를 생성합니다:

```csharp
public class MyWeatherContext : DbContext
{
    public MyWeatherContext(DbContextOptions<MyWeatherContext> options)
        : base(options)
    {
    }

    public DbSet<Zone> FavoriteZones => Set<Zone>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Zone>()
            .HasKey(z => z.Key);
    }
}
```

1. 애플리케이션의 Program.cs에서 DbContext를 등록합니다:

```csharp
builder.AddNpgsqlDbContext<MyWeatherContext>(connectionName: "weatherdb");
```

.NET Aspire가 연결 문자열 구성을 자동으로 처리한다는 점에 주목하세요. 연결 이름 "weatherdb"는 AppHost 프로젝트에서 생성한 데이터베이스 이름과 일치합니다.

1. 데이터베이스 초기화를 설정합니다:

```csharp
if (app.Environment.IsDevelopment())
{
    using (var scope = app.Services.CreateScope())
    {
        var context = scope.ServiceProvider.GetRequiredService<MyWeatherContext>();
        await context.Database.EnsureCreatedAsync();
    }
}
```

개발 환경에서는 `EnsureCreatedAsync()`를 사용하여 데이터베이스 스키마를 자동으로 생성합니다. 프로덕션 환경에서는 대신 적절한 데이터베이스 마이그레이션을 사용해야 합니다.

## 웹 앱 업데이트

이제 날씨 존을 즐겨찾기에 추가하고 필터링하는 기능을 지원하도록 웹 애플리케이션을 업데이트합니다. 이러한 변경사항을 단계별로 수행해 보겠습니다:

1. `Home.razor` 상단에 아직 없다면 이러한 Entity Framework using 문을 추가합니다:

```csharp
@using Microsoft.EntityFrameworkCore
@inject MyWeatherContext DbContext
```

1. 즐겨찾기 기능을 지원하기 위해 `@code` 블록에 이러한 새 속성을 추가합니다:

```csharp
bool ShowOnlyFavorites { get; set; }
List<Zone> FavoriteZones { get; set; } = new List<Zone>();
```

1. 데이터베이스에서 즐겨찾기를 로드하도록 `OnInitializedAsync` 메서드를 업데이트합니다. 기존 메서드를 찾아 다음으로 교체합니다:

```csharp
protected override async Task OnInitializedAsync()
{
    AllZones = (await NwsManager.GetZonesAsync()).ToArray();
    FavoriteZones = await DbContext.FavoriteZones.ToListAsync();
}
```

1. 마지막으로, 즐겨찾기를 데이터베이스에 저장하는 `ToggleFavorite` 메서드를 추가합니다. 이 메서드를 `@code` 블록에 추가하세요:

```csharp
private async Task ToggleFavorite(Zone zone)
{
    if (FavoriteZones.Contains(zone))
    {
        FavoriteZones.Remove(zone);
        DbContext.FavoriteZones.Remove(zone);
    }
    else
    {
        FavoriteZones.Add(zone);
        DbContext.FavoriteZones.Add(zone);
    }
    await DbContext.SaveChangesAsync();
}
```

1. `Home.razor`의 `@code` 블록에서 `zones` 속성을 찾아 즐겨찾기 필터를 포함하는 이 업데이트된 버전으로 교체합니다:

```csharp
IQueryable<Zone> zones
{
    get
    {
        var results = AllZones.AsQueryable();

        if (ShowOnlyFavorites)
        {
            results = results.Where(z => FavoriteZones.Contains(z));
        }

        results = string.IsNullOrEmpty(StateFilter) ? results
                : results.Where(z => z.State == StateFilter.ToUpper());

        results = string.IsNullOrEmpty(NameFilter) ? results
                : results.Where(z => z.Name.Contains(NameFilter, StringComparison.InvariantCultureIgnoreCase));

        return results.OrderBy(z => z.Name);
    }
}
```

1. 먼저, 존 목록을 필터링하기 위한 체크박스를 추가합니다. `Home.razor`에서 `<QuickGrid>` 요소 바로 앞에 이 코드를 추가합니다:

```csharp
<div class="form-check mb-3">
    <input class="form-check-input" type="checkbox" @bind="ShowOnlyFavorites" id="showFavorites">
    <label class="form-check-label" for="showFavorites">
        즐겨찾기만 표시
    </label>
</div>
```

1. 다음으로, 즐겨찾기 상태를 표시하는 새 열을 추가합니다. `<QuickGrid>` 요소 내의 기존 State 열 뒤에 이 열 정의를 추가합니다:

```csharp
<TemplateColumn Title="즐겨찾기">
    <ChildContent>
        <button @onclick="@(() => ToggleFavorite(context))">
            @if (FavoriteZones.Contains(context))
            {
                <span>&#9733;</span> <!-- 즐겨찾기 등록됨 -->
            }
            else
            {
                <span>&#9734;</span> <!-- 즐겨찾기 등록 안됨 -->
            }
        </button>
    </ChildContent>
</TemplateColumn>
```

## 변경사항 테스트

이제 즐겨찾기 기능과 데이터베이스 지속성을 테스트하여 변경사항이 올바르게 작동하는지 확인해 보겠습니다:

1. 애플리케이션을 시작합니다:
   - Visual Studio: AppHost 프로젝트를 마우스 오른쪽 버튼으로 클릭하고 "시작 프로젝트로 설정"을 선택한 다음 F5를 누릅니다
   - VS Code: 실행 및 디버그 패널(Ctrl+Shift+D)을 열고 드롭다운에서 "Run AppHost"를 선택한 다음 실행을 클릭합니다

1. 브라우저에서 My Weather Hub 애플리케이션을 엽니다:
   - <https://localhost:7274>로 이동합니다
   - 그리드 위에 새로운 "즐겨찾기만 표시" 체크박스가 표시되는지 확인합니다
   - 그리드의 각 행에 즐겨찾기 열에 별표 아이콘(☆)이 있는지 확인합니다

1. 즐겨찾기 기능을 테스트합니다:
   - 이름 필터를 사용하여 "Philadelphia"를 찾습니다
   - Philadelphia 옆의 빈 별표(☆)를 클릭합니다 - 채워진 별표(★)가 되어야 합니다
   - 몇 개의 다른 도시를 즐겨찾기로 추가합니다("Manhattan"과 "Los Angeles County"를 시도해 보세요)
   - "즐겨찾기만 표시" 체크박스를 선택합니다
   - 그리드가 이제 즐겨찾기 도시만 표시하는지 확인합니다
   - "즐겨찾기만 표시"를 선택 해제하여 모든 도시를 다시 표시합니다
   - 채워진 별표(★)를 클릭하여 도시의 즐겨찾기를 해제해 보세요

1. 지속성을 확인합니다:
   - 브라우저 창을 닫습니다
   - IDE에서 애플리케이션을 중지합니다(중지 버튼을 클릭하거나 Shift+F5를 누릅니다)
   - AppHost 프로젝트를 다시 시작합니다
   - <https://localhost:7274>로 다시 이동합니다
   - 다음을 확인합니다:
     - 즐겨찾기 도시가 여전히 채워진 별표(★)를 표시합니다
     - "즐겨찾기만 표시"를 선택하면 저장된 도시로만 필터링됩니다
     - 별표 토글이 즐겨찾기 추가/제거에 여전히 작동합니다

처음부터 다시 시작하려면:

1. 애플리케이션을 완전히 중지합니다
1. Docker Desktop을 엽니다
1. 볼륨 섹션으로 이동합니다
1. PostgreSQL 볼륨을 찾아 삭제합니다
1. 애플리케이션을 다시 시작합니다 - 새로운 데이터베이스가 자동으로 생성됩니다

> 참고: `Zone` 타입은 `record`이므로 동등성은 값에 의한 것입니다. UI가 `FavoriteZones.Contains(context)`를 확인할 때 레코드의 값(Key/Name/State 등)으로 비교하며, 이것이 즐겨찾기의 의도된 동작입니다.

## 기타 데이터 옵션

PostgreSQL 외에도 .NET Aspire는 여러 다른 데이터베이스 시스템에 대한 최고 수준의 지원을 제공합니다:

### [Azure SQL/SQL Server](https://learn.microsoft.com/en-us/dotnet/aspire/database/sql-server-entity-framework-integration)

.NET Aspire의 SQL Server 통합에는 개발을 위한 자동 컨테이너 프로비저닝, 연결 문자열 관리 및 상태 확인이 포함됩니다. 로컬 SQL Server 컨테이너와 프로덕션의 Azure SQL Database를 모두 지원합니다. 통합은 연결 복원력을 자동으로 처리하고 데이터베이스 작업 모니터링을 위한 원격 측정을 포함합니다.

### [MySQL](https://learn.microsoft.com/en-us/dotnet/aspire/database/mysql-entity-framework-integration)

.NET Aspire의 MySQL 통합은 컨테이너화된 개발 환경과 프로덕션 준비 구성을 포함하여 PostgreSQL과 유사한 기능을 제공합니다. 기본 제공 연결 재시도 및 상태 모니터링을 포함하여 개발 및 프로덕션 시나리오 모두에 적합합니다.

### [MongoDB](https://learn.microsoft.com/en-us/dotnet/aspire/database/mongodb-integration)

NoSQL 시나리오의 경우 Aspire의 MongoDB 통합은 연결 관리, 상태 확인 및 원격 측정을 제공합니다. 독립 실행형 MongoDB 인스턴스와 복제본 세트를 모두 지원하며 로컬 개발을 위한 자동 컨테이너 프로비저닝을 제공합니다. 통합은 연결 문자열 관리를 처리하고 MongoDB 작업에 특별히 조정된 재시도 정책을 포함합니다.

### SQLite

SQLite는 컨테이너화가 필요하지 않지만 Aspire는 일관된 구성 패턴과 상태 확인을 제공합니다. 완전히 자체 포함되면서도 다른 데이터베이스 제공자와 동일한 친숙한 개발 경험을 제공하므로 개발 및 테스트 시나리오에 특히 유용합니다.

## 커뮤니티 툴킷 데이터베이스 기능

.NET Aspire Community Toolkit은 추가 도구로 데이터베이스 기능을 확장합니다:

### [SQL Database Projects](https://learn.microsoft.com/en-us/dotnet/aspire/community-toolkit/hosting-sql-database-projects)

SQL Database Projects 통합을 통해 데이터베이스 스키마를 소스 코드의 일부로 포함할 수 있습니다. 개발 중에 데이터베이스 스키마를 자동으로 빌드하고 배포하여 데이터베이스 구조가 버전 제어되고 일관되게 배포되도록 합니다. 이는 데이터베이스 스키마를 애플리케이션 코드와 함께 유지하려는 팀에 특히 유용합니다.

### [Data API Builder](https://learn.microsoft.com/en-us/dotnet/aspire/community-toolkit/hosting-data-api-builder)

Data API Builder(DAB)는 데이터베이스 스키마에서 REST 및 GraphQL 엔드포인트를 자동으로 생성합니다. 이 통합을 통해 추가 코드를 작성하지 않고도 최신 API를 통해 데이터를 빠르게 노출할 수 있습니다. 다음과 같은 기능이 포함됩니다:

- 자동 REST 및 GraphQL 엔드포인트 생성
- 기본 제공 인증 및 권한 부여
- 사용자 정의 정책 지원
- GraphQL 구독을 통한 실시간 업데이트
- 데이터베이스 스키마 중심 API 설계

## 결론

이 모듈에서는 .NET Aspire의 데이터베이스 통합 기능을 사용하여 애플리케이션에 PostgreSQL 데이터베이스 지원을 추가했습니다. 데이터 액세스에는 Entity Framework Core를 사용했고 로컬 개발 및 클라우드 호스팅 데이터베이스 모두에서 작동하도록 애플리케이션을 구성했습니다.

다음 자연스러운 단계는 데이터베이스 통합이 올바르게 작동하는지 확인하는 테스트를 추가하는 것입니다.

[모듈 #8: 통합 테스트](../Lesson-08-Integration-Testing/README.md)로 이동하여 .NET Aspire 애플리케이션의 통합 테스트 작성 방법을 알아보세요.

**다음**: [모듈 #8: 통합 테스트](../Lesson-08-Integration-Testing/README.md)
