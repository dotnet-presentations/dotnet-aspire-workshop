# Pruebas de Integración con .NET Aspire

## Introducción

En este módulo, cubriremos las pruebas de integración usando `Aspire.Hosting.Testing` con `MSTest`. Las pruebas de integración son cruciales para asegurar que diferentes partes de tu aplicación funcionen juntas como se espera. Crearemos un proyecto de pruebas separado para probar tanto la API como la aplicación web.

## Diferencia Entre Pruebas Unitarias y Pruebas de Integración

Las pruebas unitarias se enfocan en probar componentes individuales o unidades de código de forma aislada. Aseguran que cada unidad funcione correctamente por sí sola. En contraste, las pruebas de integración verifican que diferentes componentes de la aplicación funcionen juntos como se espera. Prueban las interacciones entre varias partes del sistema, como APIs, bases de datos y aplicaciones web.

En el contexto de aplicaciones distribuidas con .NET Aspire, las pruebas de integración son esenciales para asegurar que los diferentes servicios y componentes se comuniquen y funcionen correctamente juntos.

## Creando el Proyecto de Pruebas de Integración

1. Crea un nuevo proyecto de pruebas llamado `IntegrationTests`.
1. Agrega referencias a los paquetes requeridos en el archivo `IntegrationTests.csproj`:

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

Este archivo de proyecto es bastante estándar para un proyecto de pruebas. Los elementos clave son:

- Un `PackageReference` al paquete NuGet [Aspire.Hosting.Testing](https://www.nuget.org/packages/Aspire.Hosting.Testing), que proporciona los tipos y APIs necesarios para probar aplicaciones .NET Aspire.
- Un `ProjectReference` al proyecto AppHost, que le da al proyecto de pruebas acceso a la definición de la aplicación distribuida objetivo.
- Las configuraciones `EnableMSTestRunner` y `OutputType` para configurar el proyecto de pruebas para ejecutarse con el ejecutor nativo de MSTest.

> Nota: Cualquier versión de MSTest 3.x está bien para este taller. Si tu entorno proporciona una 3.x más nueva, puedes usar esa.

1. Crea clases de prueba para las pruebas de integración:

El archivo `IntegrationTests.cs` prueba la funcionalidad de la API y la aplicación web:

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

Esta clase de prueba demuestra cómo probar tu aplicación distribuida. Examinemos lo que estas pruebas están haciendo:

- Ambas pruebas siguen un patrón similar, utilizando `DistributedApplicationTestingBuilder.CreateAsync<Projects.AppHost>()` para crear asincrónicamente una instancia de tu host de aplicación.
- El `appHost` se configura con manejadores de resiliencia HTTP estándar, que proporcionan políticas de reintento y circuit breakers para una comunicación HTTP más robusta.
- La prueba llama a `appHost.BuildAsync()` para construir la aplicación y luego recupera el `ResourceNotificationService` del contenedor DI.
- Después de iniciar la aplicación con `app.StartAsync()`, se crea un `HttpClient` específicamente para el recurso que se está probando (ya sea "api" o "myweatherhub").
- La prueba espera a que el recurso objetivo alcance el estado "Running" antes de proceder, asegurando que el servicio esté listo para aceptar solicitudes.
- Finalmente, se hacen solicitudes HTTP a endpoints específicos, y las aserciones verifican las respuestas.

En la primera prueba, verificamos que el endpoint `/zones` de la API devuelva una colección válida de datos de zona. En la segunda prueba, verificamos que la página de inicio de la aplicación web se cargue exitosamente y contenga el contenido esperado.

El archivo `EnvVarTests.cs` verifica la resolución de variables de entorno:

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

Esta prueba se enfoca en verificar la configuración de descubrimiento de servicios:

- Usa `DistributedApplicationTestingBuilder.CreateAsync<Projects.AppHost>()` para crear una instancia del host de aplicación.
- En lugar de iniciar la aplicación, recupera directamente una instancia `IResourceWithEnvironment` que representa el frontend web ("myweatherhub").
- Llama a `GetEnvironmentVariableValuesAsync()` con el argumento `DistributedApplicationOperation.Publish` para obtener las variables de entorno que se publicarían al recurso.
- Finalmente, afirma que el frontend web tiene una variable de entorno que se resuelve a la URL del servicio API, confirmando que el descubrimiento de servicios está configurado correctamente.

Esta prueba es particularmente valiosa porque verifica que los servicios de tu aplicación están correctamente conectados a través de variables de entorno, que es como .NET Aspire maneja el descubrimiento de servicios en aplicaciones distribuidas.

> Nota: Si ves un archivo `WeatherBackgroundTests.cs` en la solución completa que está vacío, es un marcador de posición para futuras pruebas de trabajos en segundo plano y puede ser ignorado para este taller.

## Ejecutando las Pruebas de Integración

### Usando la Línea de Comandos

1. Abre una terminal y navega a la carpeta `complete`.
1. Ejecuta las pruebas de integración usando el comando `dotnet test`:

```bash
dotnet test IntegrationTests/IntegrationTests.csproj
```

### Usando el Explorador de Pruebas de Visual Studio

1. Abre la solución en Visual Studio
1. Abre el Explorador de Pruebas yendo a Ver > Explorador de Pruebas (o presiona Ctrl+E, T)
1. En la ventana del Explorador de Pruebas, verás todas las pruebas en tu solución

![Explorador de Pruebas de Visual Studio](../media/vs-test-explorer.png)

1. Puedes:
   - Ejecutar todas las pruebas haciendo clic en el botón "Ejecutar Todo" en la parte superior
   - Ejecutar una prueba específica haciendo clic derecho en ella y seleccionando "Ejecutar"
   - Ejecutar solo las pruebas fallidas haciendo clic en el botón "Ejecutar Pruebas Fallidas"
   - Ejecutar pruebas en modo de depuración haciendo clic derecho y seleccionando "Depurar"
   - Ver resultados de pruebas y salida en la ventana del Explorador de Pruebas

Las pruebas verificarán que:

- Las variables de entorno están configuradas correctamente
- Los endpoints de la API están funcionando correctamente
- La aplicación web está funcionando como se espera

Al ejecutar estas pruebas, todos los logs de recursos se redirigen al `DistributedApplication` por defecto. Esta redirección de logs habilita escenarios donde quieres afirmar que un recurso está registrando correctamente, aunque no estamos haciendo eso en estas pruebas particulares.

## Herramientas de Pruebas Adicionales

Playwright es una herramienta poderosa para pruebas de extremo a extremo. Te permite automatizar interacciones del navegador y verificar que tu aplicación funcione como se espera desde la perspectiva del usuario. Playwright soporta múltiples navegadores, incluyendo Chromium, Firefox y WebKit.

### Caso de Uso

Playwright puede ser usado para realizar pruebas de extremo a extremo de tu aplicación web. Puede simular interacciones del usuario, como hacer clic en botones, llenar formularios y navegar entre páginas. Esto asegura que tu aplicación se comporte correctamente en escenarios del mundo real.

### Conceptos de Alto Nivel

- **Automatización de Navegador**: Playwright puede lanzar y controlar navegadores para realizar pruebas automatizadas.
- **Pruebas Multi-navegador**: Playwright soporta pruebas a través de diferentes navegadores para asegurar compatibilidad.
- **Modo Sin Cabeza**: Playwright puede ejecutar pruebas en modo sin cabeza, lo que significa que el navegador se ejecuta en segundo plano sin una interfaz gráfica de usuario.
- **Aserciones**: Playwright proporciona aserciones incorporadas para verificar que los elementos estén presentes, visibles y tengan las propiedades esperadas.

Para más información sobre Playwright, consulta la [documentación oficial](https://playwright.dev/dotnet/).

## Conclusión

En este módulo, cubrimos las pruebas de integración usando `Aspire.Hosting.Testing` con `MSTest`. Creamos un proyecto de pruebas separado para probar tanto la API como la aplicación web, siguiendo patrones similares al enfoque `WebApplicationFactory` en ASP.NET Core pero adaptado para aplicaciones distribuidas.

Nuestras pruebas verificaron tres aspectos críticos de la aplicación distribuida:

1. La funcionalidad de la API (probando que los endpoints devuelvan datos esperados)
1. La funcionalidad de la aplicación web (probando que la UI se renderice correctamente)
1. El mecanismo de descubrimiento de servicios (probando que los servicios puedan encontrarse y comunicarse entre sí)

Para una inmersión más profunda en las pruebas con .NET Aspire, incluyendo un video tutorial, consulta la publicación del blog [Getting started with testing and .NET Aspire](https://devblogs.microsoft.com/dotnet/getting-started-with-testing-and-dotnet-aspire/).

Ahora, aprendamos sobre las opciones de despliegue al usar .NET Aspire.

**Siguiente**: [Módulo #9: Despliegue](../Lesson-09-Deployment/README.md)
