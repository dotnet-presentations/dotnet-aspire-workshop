# 数据库集成

## 介绍

在本模块中，我们将在应用程序中集成PostgreSQL数据库。我们将使用Entity Framework Core (EF Core)与数据库进行交互。此外，我们将设置PgAdmin来管理我们的PostgreSQL数据库。

## 设置PostgreSQL

.NET Aspire通过`Aspire.Hosting.PostgreSQL`包提供对PostgreSQL的内置支持。要设置PostgreSQL：

1. 在AppHost项目中安装所需的NuGet包：

```xml
<PackageReference Include="Aspire.Hosting.PostgreSQL" Version="9.4.2" />
```

1. 更新AppHost的Program.cs以添加PostgreSQL：

```csharp
var postgres = builder.AddPostgres("postgres")
    .WithDataVolume(isReadOnly: false);

var weatherDb = postgres.AddDatabase("weatherdb");
```

`WithDataVolume(isReadOnly: false)`配置确保您的数据在容器重启之间持久化。数据存储在容器外部存在的Docker卷中，使其能够在容器重启后存活。这在研讨会中是可选的——如果您省略它，示例仍然运行；您只是不会在运行之间保持数据。

### .NET Aspire 9.4的新功能：增强的数据库初始化

.NET Aspire 9.4为所有数据库提供程序引入了改进的`WithInitFiles()`方法，替换了更复杂的`WithInitBindMount()`方法：

```csharp
var postgres = builder.AddPostgres("postgres")
    .WithDataVolume(isReadOnly: false)
    .WithInitFiles("./database-init");  // 从文件简化初始化
```

此方法在所有数据库提供程序（PostgreSQL、MySQL、MongoDB、Oracle）中一致工作，并提供更好的错误处理和简化的配置。在此研讨会中使用`WithInitFiles`是可选的；数据库集成无需它即可工作。

为确保适当的应用程序启动，我们将配置Web应用程序等待数据库：

```csharp
var web = builder.AddProject<Projects.MyWeatherHub>("myweatherhub")
    .WithReference(weatherDb)
    .WaitFor(postgres)  // 确保应用启动前数据库准备就绪
    .WithExternalHttpEndpoints();
```

### 向数据库添加容器持久性

对于希望数据库容器和数据在多次应用程序运行中持久化的开发场景，您可以配置容器生命周期：

```csharp
var postgres = builder.AddPostgres("postgres")
    .WithDataVolume(isReadOnly: false)
    .WithLifetime(ContainerLifetime.Persistent);  // 容器在应用重启间持久化

var weatherDb = postgres.AddDatabase("weatherdb");
```

使用`ContainerLifetime.Persistent`，即使您停止Aspire应用程序，PostgreSQL容器也将继续运行。这是可选的，不是完成模块所必需的。如果启用，这意味着：

- **更快的启动时间**：在后续运行中无需等待PostgreSQL初始化
- **数据持久性**：您的数据库数据在应用程序会话之间保持完整
- **一致的开发**：数据库保持您离开时的相同状态

> **注意**：持久容器主要适用于开发场景。在生产部署中，您通常会使用自动处理持久性的托管数据库服务。
>
> **高级容器功能**：.NET Aspire还支持高级容器配置，如用于更好启动协调的`WithExplicitStart()`，以及用于挂载初始化脚本的`WithContainerFiles()`。当复杂开发场景需要时，这些功能提供对容器行为的细粒度控制。要了解这些高级功能的更多信息，请参阅官方文档中的[使用卷持久化数据](https://learn.microsoft.com/dotnet/aspire/fundamentals/persist-data-volumes)和[容器资源生命周期](https://learn.microsoft.com/dotnet/aspire/fundamentals/app-host-overview#container-resource-lifecycle)。

## 将EF Core与PostgreSQL集成

1. 在Web应用程序中安装所需的NuGet包：

```xml
<PackageReference Include="Aspire.Npgsql.EntityFrameworkCore.PostgreSQL" Version="9.4.2" />
```

1. 创建您的DbContext类：

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

1. 在应用程序的Program.cs中注册DbContext：

```csharp
builder.AddNpgsqlDbContext<MyWeatherContext>(connectionName: "weatherdb");
```

请注意，.NET Aspire自动处理连接字符串配置。连接名称"weatherdb"与我们在AppHost项目中创建的数据库名称匹配。

1. 设置数据库初始化：

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

对于开发环境，我们使用`EnsureCreatedAsync()`自动创建数据库架构。在生产环境中，您应该使用适当的数据库迁移。

## 更新Web应用

现在我们将更新Web应用程序以支持收藏天气区域和过滤它们。让我们逐步进行这些更改：

1. 确保在`Home.razor`顶部添加这些Entity Framework using语句（如果尚未存在）：

```csharp
@using Microsoft.EntityFrameworkCore
@inject MyWeatherContext DbContext
```

1. 将这些新属性添加到`@code`块以支持收藏功能：

```csharp
bool ShowOnlyFavorites { get; set; }
List<Zone> FavoriteZones { get; set; } = new List<Zone>();
```

1. 更新`OnInitializedAsync`方法以从数据库加载收藏。找到现有方法并替换为：

```csharp
protected override async Task OnInitializedAsync()
{
    AllZones = (await NwsManager.GetZonesAsync()).ToArray();
    FavoriteZones = await DbContext.FavoriteZones.ToListAsync();
}
```

1. 最后，添加`ToggleFavorite`方法来处理将收藏保存到数据库。将此方法添加到`@code`块：

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

1. 在`Home.razor`的`@code`块中，找到`zones`属性并替换为包含收藏过滤器的更新版本：

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

1. 首先，添加一个复选框来过滤区域列表。在`Home.razor`中，在`<QuickGrid>`元素之前添加此代码：

```csharp
<div class="form-check mb-3">
    <input class="form-check-input" type="checkbox" @bind="ShowOnlyFavorites" id="showFavorites">
    <label class="form-check-label" for="showFavorites">
        仅显示收藏
    </label>
</div>
```

1. 接下来，添加一个新列来显示收藏状态。在`<QuickGrid>`元素内的现有State列之后添加此列定义：

```csharp
<TemplateColumn Title="收藏">
    <ChildContent>
        <button @onclick="@(() => ToggleFavorite(context))">
            @if (FavoriteZones.Contains(context))
            {
                <span>&#9733;</span> <!-- 已收藏 -->
            }
            else
            {
                <span>&#9734;</span> <!-- 未收藏 -->
            }
        </button>
    </ChildContent>
</TemplateColumn>
```

## 测试您的更改

现在让我们通过测试收藏功能和数据库持久性来验证您的更改是否正常工作：

1. 启动应用程序：
   - 在Visual Studio中：右键单击AppHost项目并选择"Set as Startup Project"，然后按F5
   - 在VS Code中：打开Run and Debug面板(Ctrl+Shift+D)，从下拉菜单中选择"Run AppHost"，然后点击Run

1. 在浏览器中打开My Weather Hub应用程序：
   - 导航到<https://localhost:7274>
   - 验证您在网格上方看到新的"仅显示收藏"复选框
   - 检查网格中的每一行现在在收藏列中都有一个星形图标(☆)

1. 测试收藏功能：
   - 使用Name过滤器查找"Philadelphia"
   - 点击Philadelphia旁边的空星(☆) - 它应该填充(★)
   - 查找并收藏几个其他城市（尝试"Manhattan"和"Los Angeles County"）
   - 选中"仅显示收藏"复选框
   - 验证网格现在只显示您收藏的城市
   - 取消选中"仅显示收藏"以再次查看所有城市
   - 尝试通过点击填充的星(★)来取消收藏一个城市

1. 验证持久性：
   - 关闭浏览器窗口
   - 在IDE中停止应用程序（点击停止按钮或按Shift+F5）
   - 重新启动AppHost项目
   - 导航回<https://localhost:7274>
   - 验证：
     - 您收藏的城市仍然显示填充的星(★)
     - 选中"仅显示收藏"仍然过滤到只有您保存的城市
     - 星形切换仍然适用于添加/删除收藏

如果您想重置并重新开始：

1. 完全停止应用程序
1. 打开Docker Desktop
1. 导航到Volumes部分
1. 查找并删除PostgreSQL卷
1. 重新启动应用程序 - 它将自动创建一个新的数据库

> 注意：`Zone`类型是`record`，所以相等性是按值比较的。当UI检查`FavoriteZones.Contains(context)`时，它通过记录的值（如Key/Name/State）进行比较，这是收藏的预期行为。

## 其他数据选项

除了PostgreSQL，.NET Aspire还为其他几个数据库系统提供一流支持：

### [Azure SQL/SQL Server](https://learn.microsoft.com/en-us/dotnet/aspire/database/sql-server-entity-framework-integration)

.NET Aspire中的SQL Server集成包括开发的自动容器配置、连接字符串管理和健康检查。它支持本地SQL Server容器和生产中的Azure SQL数据库。集成自动处理连接弹性，并包括用于监控数据库操作的遥测。

### [MySQL](https://learn.microsoft.com/en-us/dotnet/aspire/database/mysql-entity-framework-integration)

.NET Aspire的MySQL集成提供与PostgreSQL类似的功能，包括容器化开发环境和生产就绪配置。它包括内置连接重试和健康监控，使其适合开发和生产场景。

### [MongoDB](https://learn.microsoft.com/en-us/dotnet/aspire/database/mongodb-integration)

对于NoSQL场景，Aspire的MongoDB集成提供连接管理、健康检查和遥测。它支持独立MongoDB实例和副本集，具有本地开发的自动容器配置。集成处理连接字符串管理，并包括专门为MongoDB操作调整的重试策略。

### SQLite

虽然SQLite不需要容器化，但Aspire提供一致的配置模式和健康检查。它对开发和测试场景特别有用，在完全自包含的同时提供与其他数据库提供程序相同的熟悉开发体验。

## 社区工具包数据库功能

.NET Aspire社区工具包通过附加工具扩展数据库功能：

### [SQL数据库项目](https://learn.microsoft.com/en-us/dotnet/aspire/community-toolkit/hosting-sql-database-projects)

SQL数据库项目集成使您能够将数据库架构作为源代码的一部分包含。它在开发期间自动构建和部署您的数据库架构，确保数据库结构得到版本控制和一致部署。这对于希望将数据库架构与应用程序代码一起维护的团队特别有用。

### [数据API构建器](https://learn.microsoft.com/en-us/dotnet/aspire/community-toolkit/hosting-data-api-builder)

数据API构建器(DAB)从您的数据库架构自动生成REST和GraphQL端点。此集成允许您通过现代API快速公开数据，而无需编写其他代码。它包括以下功能：

- 自动REST和GraphQL端点生成
- 内置身份验证和授权
- 自定义策略支持
- 通过GraphQL订阅进行实时更新
- 数据库架构驱动的API设计

## 结论

在本模块中，我们使用.NET Aspire的数据库集成功能向应用程序添加了PostgreSQL数据库支持。我们使用Entity Framework Core进行数据访问，并配置了应用程序以同时适用于本地开发和云托管数据库。

下一个自然步骤是添加测试以验证数据库集成是否正常工作。

前往[模块#8：集成测试](../Lesson-08-Integration-Testing/README.md)学习如何为您的.NET Aspire应用程序编写集成测试。

**下一步**：[模块#8：集成测试](../Lesson-08-Integration-Testing/README.md)
