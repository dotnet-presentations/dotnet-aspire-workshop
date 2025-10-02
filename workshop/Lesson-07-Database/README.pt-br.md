# Integração de Banco de Dados

## Introdução

Neste módulo, integraremos um banco de dados PostgreSQL com nossa aplicação. Usaremos o Entity Framework Core (EF Core) para interagir com o banco de dados. Além disso, configuraremos o PgAdmin para gerenciar nosso banco de dados PostgreSQL.

## Configurando PostgreSQL

O .NET Aspire fornece suporte integrado para PostgreSQL através do pacote `Aspire.Hosting.PostgreSQL`. Para configurar o PostgreSQL:

1. Instale o pacote NuGet necessário no seu projeto AppHost:

```xml
<PackageReference Include="Aspire.Hosting.PostgreSQL" Version="9.4.2" />
```

1. Atualize o Program.cs do AppHost para adicionar PostgreSQL:

```csharp
var postgres = builder.AddPostgres("postgres")
    .WithDataVolume(isReadOnly: false);

var weatherDb = postgres.AddDatabase("weatherdb");
```

A configuração `WithDataVolume(isReadOnly: false)` garante que seus dados persistam entre reinicializações do contêiner. Os dados são armazenados em um volume Docker que existe fora do contêiner, permitindo que sobrevivam às reinicializações do contêiner. Isso é opcional para o workshop—se você omitir, o exemplo ainda funciona; você apenas não manterá dados entre execuções.

### Novidade no .NET Aspire 9.4: Inicialização de Banco de Dados Aprimorada

O .NET Aspire 9.4 introduz o método `WithInitFiles()` aprimorado para todos os provedores de banco de dados, substituindo o método mais complexo `WithInitBindMount()`:

```csharp
var postgres = builder.AddPostgres("postgres")
    .WithDataVolume(isReadOnly: false)
    .WithInitFiles("./database-init");  // Inicialização simplificada a partir de arquivos
```

Este método funciona consistentemente em todos os provedores de banco de dados (PostgreSQL, MySQL, MongoDB, Oracle) e fornece melhor tratamento de erros e configuração simplificada. O uso de `WithInitFiles` é opcional para este workshop; a integração do banco de dados funciona sem ele.

Para garantir o início adequado da aplicação, configuraremos a aplicação web para aguardar o banco de dados:

```csharp
var web = builder.AddProject<Projects.MyWeatherHub>("myweatherhub")
    .WithReference(weatherDb)
    .WaitFor(postgres)  // Garante que o banco de dados esteja pronto antes do início da app
    .WithExternalHttpEndpoints();
```

### Adicionando Persistência de Contêiner ao Seu Banco de Dados

Para cenários de desenvolvimento onde você quer que seu contêiner de banco de dados e dados persistam em múltiplas execuções da aplicação, você pode configurar o tempo de vida do contêiner:

```csharp
var postgres = builder.AddPostgres("postgres")
    .WithDataVolume(isReadOnly: false)
    .WithLifetime(ContainerLifetime.Persistent);  // Contêiner persiste entre reinicializações da app

var weatherDb = postgres.AddDatabase("weatherdb");
```

Com `ContainerLifetime.Persistent`, o contêiner PostgreSQL continuará executando mesmo quando você parar sua aplicação Aspire. Isso é opcional e não é necessário para completar o módulo. Se habilitado, significa:

- **Tempos de inicialização mais rápidos**: Não há necessidade de aguardar a inicialização do PostgreSQL em execuções subsequentes
- **Persistência de dados**: Seus dados de banco de dados permanecem intactos entre sessões da aplicação
- **Desenvolvimento consistente**: O banco de dados permanece no mesmo estado em que você o deixou

> **Nota**: Contêineres persistentes são principalmente úteis para cenários de desenvolvimento. Em implantações de produção, você normalmente usará serviços de banco de dados gerenciados que tratam da persistência automaticamente.
>
> **Recursos Avançados de Contêiner**: O .NET Aspire também suporta configuração avançada de contêiner como `WithExplicitStart()` para melhor coordenação de inicialização, e `WithContainerFiles()` para montar scripts de inicialização. Esses recursos fornecem controle fino sobre o comportamento do contêiner quando necessário para cenários de desenvolvimento complexos. Para saber mais sobre esses recursos avançados, veja [Persistir dados usando volumes](https://learn.microsoft.com/dotnet/aspire/fundamentals/persist-data-volumes) e [Ciclo de vida do recurso de contêiner](https://learn.microsoft.com/dotnet/aspire/fundamentals/app-host-overview#container-resource-lifecycle) na documentação oficial.

## Integrando EF Core com PostgreSQL

1. Instale os pacotes NuGet necessários na sua aplicação web:

```xml
<PackageReference Include="Aspire.Npgsql.EntityFrameworkCore.PostgreSQL" Version="9.4.2" />
```

1. Crie sua classe DbContext:

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

1. Registre o DbContext no Program.cs da sua aplicação:

```csharp
builder.AddNpgsqlDbContext<MyWeatherContext>(connectionName: "weatherdb");
```

Note que o .NET Aspire trata da configuração da string de conexão automaticamente. O nome da conexão "weatherdb" corresponde ao nome do banco de dados que criamos no projeto AppHost.

1. Configure a inicialização do banco de dados:

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

Para ambientes de desenvolvimento, usamos `EnsureCreatedAsync()` para criar automaticamente o esquema do banco de dados. Em um ambiente de produção, você deve usar migrações de banco de dados adequadas.

## Atualizando a Aplicação Web

Agora atualizaremos a aplicação web para suportar favoritismo de zonas climáticas e filtragem. Vamos fazer essas alterações passo a passo:

1. Certifique-se de adicionar essas declarações using do Entity Framework no topo de `Home.razor` se ainda não estiverem presentes:

```csharp
@using Microsoft.EntityFrameworkCore
@inject MyWeatherContext DbContext
```

1. Adicione essas novas propriedades ao bloco `@code` para suportar a funcionalidade de favoritos:

```csharp
bool ShowOnlyFavorites { get; set; }
List<Zone> FavoriteZones { get; set; } = new List<Zone>();
```

1. Atualize o método `OnInitializedAsync` para carregar favoritos do banco de dados. Encontre o método existente e substitua por:

```csharp
protected override async Task OnInitializedAsync()
{
    AllZones = (await NwsManager.GetZonesAsync()).ToArray();
    FavoriteZones = await DbContext.FavoriteZones.ToListAsync();
}
```

1. Finalmente, adicione o método `ToggleFavorite` para tratar do salvamento de favoritos no banco de dados. Adicione este método ao bloco `@code`:

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

1. No bloco `@code` de `Home.razor`, localize a propriedade `zones` e substitua por esta versão atualizada que inclui o filtro de favoritos:

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

1. Primeiro, adicione uma caixa de seleção para filtrar a lista de zonas. Em `Home.razor`, adicione este código logo antes do elemento `<QuickGrid>`:

```csharp
<div class="form-check mb-3">
    <input class="form-check-input" type="checkbox" @bind="ShowOnlyFavorites" id="showFavorites">
    <label class="form-check-label" for="showFavorites">
        Mostrar apenas favoritos
    </label>
</div>
```

1. Em seguida, adicione uma nova coluna para mostrar o status de favorito. Adicione esta definição de coluna dentro do elemento `<QuickGrid>`, após a coluna State existente:

```csharp
<TemplateColumn Title="Favorito">
    <ChildContent>
        <button @onclick="@(() => ToggleFavorite(context))">
            @if (FavoriteZones.Contains(context))
            {
                <span>&#9733;</span> <!-- Favoritado -->
            }
            else
            {
                <span>&#9734;</span> <!-- Não favoritado -->
            }
        </button>
    </ChildContent>
</TemplateColumn>
```

## Testando Suas Alterações

Agora vamos verificar se suas alterações estão funcionando corretamente testando a funcionalidade de favoritos e a persistência do banco de dados:

1. Inicie a aplicação:
   - No Visual Studio: Clique com o botão direito no projeto AppHost e selecione "Set as Startup Project", depois pressione F5
   - No VS Code: Abra o painel Run and Debug (Ctrl+Shift+D), selecione "Run AppHost" no dropdown e clique em Run

1. Abra seu navegador para a aplicação My Weather Hub:
   - Navegue para <https://localhost:7274>
   - Verifique se você vê a nova caixa de seleção "Mostrar apenas favoritos" acima da grade
   - Verifique se cada linha na grade agora tem um ícone de estrela (☆) na coluna Favorito

1. Teste a funcionalidade de favoritos:
   - Use o filtro Name para encontrar "Philadelphia"
   - Clique na estrela vazia (☆) ao lado de Philadelphia - deve preencher (★)
   - Encontre e favorite algumas outras cidades (tente "Manhattan" e "Los Angeles County")
   - Marque a caixa "Mostrar apenas favoritos"
   - Verifique se a grade agora mostra apenas suas cidades favoritas
   - Desmarque "Mostrar apenas favoritos" para ver todas as cidades novamente
   - Tente desfavoritar uma cidade clicando em sua estrela preenchida (★)

1. Verifique a persistência:
   - Feche a janela do seu navegador
   - Pare a aplicação no seu IDE (clique no botão stop ou pressione Shift+F5)
   - Reinicie o projeto AppHost
   - Navegue de volta para <https://localhost:7274>
   - Verifique se:
     - Suas cidades favoritas ainda mostram estrelas preenchidas (★)
     - Marcar "Mostrar apenas favoritos" ainda filtra para apenas suas cidades salvas
     - Os alternadores de estrela ainda funcionam para adicionar/remover favoritos

Se você quiser redefinir e começar do zero:

1. Pare a aplicação completamente
1. Abra o Docker Desktop
1. Navegue para a seção Volumes
1. Encontre e exclua o volume PostgreSQL
1. Reinicie a aplicação - ela criará um banco de dados novo automaticamente

> Nota: O tipo `Zone` é um `record`, então a igualdade é por valor. Quando a UI verifica `FavoriteZones.Contains(context)`, está comparando pelos valores do registro (como Key/Name/State), que é o comportamento pretendido para favoritos.

## Outras Opções de Dados

Além do PostgreSQL, o .NET Aspire fornece suporte de primeira classe para vários outros sistemas de banco de dados:

### [Azure SQL/SQL Server](https://learn.microsoft.com/en-us/dotnet/aspire/database/sql-server-entity-framework-integration)

A integração do SQL Server no .NET Aspire inclui provisionamento automático de contêiner para desenvolvimento, gerenciamento de string de conexão e verificações de saúde. Suporta tanto contêineres SQL Server locais quanto Azure SQL Database em produção. A integração trata da resiliência de conexão automaticamente e inclui telemetria para monitorar operações de banco de dados.

### [MySQL](https://learn.microsoft.com/en-us/dotnet/aspire/database/mysql-entity-framework-integration)

A integração MySQL para .NET Aspire fornece capacidades similares ao PostgreSQL, incluindo ambientes de desenvolvimento containerizados e configurações prontas para produção. Inclui tentativas de conexão integradas e monitoramento de saúde, tornando-o adequado para cenários de desenvolvimento e produção.

### [MongoDB](https://learn.microsoft.com/en-us/dotnet/aspire/database/mongodb-integration)

Para cenários NoSQL, a integração MongoDB do Aspire oferece gerenciamento de conexão, verificações de saúde e telemetria. Suporta tanto instâncias MongoDB standalone quanto conjuntos de réplicas, com provisionamento automático de contêiner para desenvolvimento local. A integração trata do gerenciamento de string de conexão e inclui políticas de tentativa especificamente ajustadas para operações MongoDB.

### SQLite

Embora o SQLite não exija containerização, o Aspire fornece padrões de configuração consistentes e verificações de saúde. É particularmente útil para cenários de desenvolvimento e teste, oferecendo a mesma experiência familiar de desenvolvimento de outros provedores de banco de dados enquanto é completamente auto-contido.

## Recursos de Banco de Dados do Community Toolkit

O .NET Aspire Community Toolkit estende as capacidades de banco de dados com ferramentas adicionais:

### [SQL Database Projects](https://learn.microsoft.com/en-us/dotnet/aspire/community-toolkit/hosting-sql-database-projects)

A integração de SQL Database Projects permite incluir seu esquema de banco de dados como parte do seu código-fonte. Automaticamente constrói e implanta seu esquema de banco de dados durante o desenvolvimento, garantindo que sua estrutura de banco de dados seja versionada e implantada consistentemente. Isso é particularmente útil para equipes que querem manter seu esquema de banco de dados junto com o código da aplicação.

### [Data API Builder](https://learn.microsoft.com/en-us/dotnet/aspire/community-toolkit/hosting-data-api-builder)

O Data API Builder (DAB) gera automaticamente endpoints REST e GraphQL a partir do seu esquema de banco de dados. Esta integração permite expor rapidamente seus dados através de APIs modernas sem escrever código adicional. Inclui recursos como:

- Geração automática de endpoints REST e GraphQL
- Autenticação e autorização integradas
- Suporte a políticas customizadas
- Atualizações em tempo real via assinaturas GraphQL
- Design de API orientado por esquema de banco de dados

## Conclusão

Neste módulo, adicionamos suporte de banco de dados PostgreSQL à nossa aplicação usando os recursos de integração de banco de dados do .NET Aspire. Usamos o Entity Framework Core para acesso a dados e configuramos nossa aplicação para funcionar tanto com desenvolvimento local quanto com bancos de dados hospedados na nuvem.

O próximo passo natural seria adicionar testes para verificar se a integração do banco de dados funciona corretamente.

Vá para [Módulo #8: Testes de Integração](../Lesson-08-Integration-Testing/README.md) para aprender como escrever testes de integração para sua aplicação .NET Aspire.

**Próximo**: [Módulo #8: Testes de Integração](../Lesson-08-Integration-Testing/README.md)
