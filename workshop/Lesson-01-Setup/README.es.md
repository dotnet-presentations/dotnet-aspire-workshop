# Configuración de la Máquina

Este taller utilizará las siguientes herramientas:

- [SDK de .NET 9](https://get.dot.net/9) o [.NET 10 Preview](https://get.dot.net/10) (opcional)
- [Docker Desktop](https://docs.docker.com/engine/install/) o [Podman](https://podman.io/getting-started/installation)
- [Visual Studio 2022](https://visualstudio.microsoft.com/vs/) o [Visual Studio Code](https://code.visualstudio.com/) con [C# Dev Kit](https://code.visualstudio.com/docs/csharp/get-started)

Para obtener la mejor experiencia, recomendamos utilizar Visual Studio 2022 con la carga de trabajo de .NET Aspire. Sin embargo, también puedes utilizar Visual Studio Code con el C# Dev Kit y la carga de trabajo de .NET Aspire. A continuación, se muestran las guías de configuración para cada plataforma.

> **Nuevo en .NET Aspire 9.4**: ¡Soporte completo para .NET 10 Preview! Ahora puedes crear proyectos Aspire dirigidos a .NET 10 usando `dotnet new aspire --framework net10.0`

## Windows con Visual Studio

- Instala [Visual Studio 2022 versión 17.14 o posterior](https://visualstudio.microsoft.com/vs/).
  - Cualquier edición funcionará incluyendo la [Visual Studio Community gratuita](https://visualstudio.microsoft.com/free-developer-offers/)
  - Selecciona la carga de trabajo `ASP.NET y desarrollo web`.

## Mac, Linux y Windows sin Visual Studio

- Instala la última versión del [SDK de .NET 9](https://get.dot.net/9?cid=eshop)

- Instala [Visual Studio Code con C# Dev Kit](https://code.visualstudio.com/docs/csharp/get-started)

> Nota: Cuando se ejecuta en Mac con Apple Silicon (procesador de la serie M), se requiere Rosetta 2 para grpc-tools.

## Instalar las Plantillas Más Recientes de .NET Aspire

Ejecuta el siguiente comando para instalar las plantillas más recientes de .NET Aspire.

```cli
dotnet new install Aspire.ProjectTemplates --force
```

## Instalar la CLI de .NET Aspire (Opcional)

.NET Aspire 9.4 introduce la CLI de Aspire generalmente disponible, proporcionando una experiencia de desarrollador optimizada. Puedes instalarla usando uno de estos métodos:

### Instalación Rápida (Recomendada)

```bash
# Windows (PowerShell)
iex "& { $(irm https://aspire.dev/install.ps1) }"

# macOS/Linux (Bash)
curl -sSL https://aspire.dev/install.sh | bash
```

### Herramienta Global de .NET

```cli
dotnet tool install -g Aspire.Cli
```

La CLI de Aspire proporciona comandos útiles como:

- `aspire new` - Crear nuevos proyectos Aspire
- `aspire run` - Encontrar y ejecutar el AppHost desde cualquier lugar en tu repositorio
- `aspire add` - Agregar paquetes de integración de hospedaje
- `aspire config` - Configurar ajustes de Aspire
- `aspire publish` - Generar artefactos de implementación

## Prueba de la instalación

Para probar tu instalación, consulta el [Crear tu primer proyecto de .NET Aspire](https://learn.microsoft.com/dotnet/aspire/get-started/build-your-first-aspire-app) para obtener más información.

## Abrir la Solución de Inicio del Taller

Para comenzar el taller, abre `start/MyWeatherHub.sln` en Visual Studio 2022. Si estás usando Visual Studio Code, abre la carpeta `start` y cuando el C# Dev Kit te pregunte qué solución abrir, selecciona **MyWeatherHub.sln**.

**Siguiente**: [Módulo #2 - Service Defaults](../Lesson-02-ServiceDefaults/README.md)
