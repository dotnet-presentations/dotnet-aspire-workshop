# Aprendamos .NET Aspire

Ven y aprende todo sobre [.NET Aspire](https://learn.microsoft.com/dotnet/aspire/), una pila de tecnología lista para la nube para construir aplicaciones distribuidas, observables y listas para producción. .NET Aspire se puede agregar a cualquier aplicación, independientemente de su tamaño y escala, para ayudarte a construir aplicaciones mejores y más rápidas.

Este taller utiliza **.NET Aspire 9.4** y está diseñado para **.NET 9** (**.NET 8** también es compatible).

.NET Aspire simplifica el desarrollo de aplicaciones con:

- **Orquestación**: Orquestación incorporada con un motor de flujo de trabajo simple pero potente. Utiliza C# y API familiares sin necesidad de YAML. Agrega fácilmente servicios en la nube populares, conecta a tus proyectos y ejecútalos localmente con un solo clic.
- **Descubrimiento de servicios**: Inyección automática de las cadenas de conexión correctas o configuraciones de red y la información de descubrimiento de servicios para simplificar la experiencia del desarrollador.
- **Componentes**: Componentes incorporados para servicios en la nube comunes como bases de datos, colas y almacenamiento. Integrados con registro, comprobaciones de salud, telemetría y más.
- **Panel de control**: Visualiza datos en vivo de OpenTelemetry sin necesidad de configuración. El panel de control para desarrolladores de .NET Aspire muestra registros, variables de entorno, trazas distribuidas, métricas y más para verificar rápidamente el comportamiento de la aplicación.
- **Despliegue**: Gestiona la inyección de las cadenas de conexión correctas o configuraciones de red y la información de descubrimiento de servicios para simplificar la experiencia del desarrollador.
- **Y mucho más**: .NET Aspire está repleto de características que a los desarrolladores les encantarán y que te ayudarán a ser más productivo.

Obtén más información sobre .NET Aspire con los siguientes recursos:

- [Documentación](https://learn.microsoft.com/dotnet/aspire)
- [Ruta de aprendizaje de Microsoft Learn](https://learn.microsoft.com/training/paths/dotnet-aspire/)
- [Videos de .NET Aspire](https://aka.ms/aspire/videos)
- [Aplicación de muestra de referencia eShop](https://github.com/dotnet/eshop)
- [Ejemplos de .NET Aspire](https://learn.microsoft.com/samples/browse/?expanded=dotnet&products=dotnet-aspire)
- [Preguntas frecuentes de .NET Aspire](https://learn.microsoft.com/dotnet/aspire/reference/aspire-faq)

## Localización

Estos materiales del taller están disponibles en los siguientes idiomas:

- [Inglés](./README.md)
- [简体中文](./README.zh-cn.md)
- [한국어](./README.ko.md)
- [日本語](./README.jp.md)
- [Español](./README.es.md)
- [Français](./README.fr.md)
- [Português (PT-BR)](./README.pt-br.md)

También puedes ver los eventos en vivo de Let's Learn .NET Aspire para los siguientes idiomas:

- [Inglés](https://www.youtube.com/watch?v=8i3FaHChh20)
- [한국어](https://www.youtube.com/watch?v=rTpNgMaVM6g)
- [日本語](https://www.youtube.com/watch?v=Cm7mqHZJIgc)
- [Español](https://www.youtube.com/watch?v=dd1Mc5bQZSo)
- [Français](https://www.youtube.com/watch?v=jJiqqVPDN4w)
- [Português (PT-BR)](https://www.youtube.com/watch?v=PUCU9ZOOgQ8)
- [Tiếng Việt](https://www.youtube.com/watch?v=48CWnYfTZhk)

## Taller

Este taller de .NET Aspire forma parte de la serie [Aprendamos .NET](https://aka.ms/letslearndotnet). Este taller está diseñado para ayudarte a aprender sobre .NET Aspire y cómo utilizarlo para construir aplicaciones listas para la nube.

### Prerequisitos

Antes de comenzar este taller, asegúrate de tener:

- [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0) (recomendado) o [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Visual Studio 2022](https://visualstudio.microsoft.com/vs/) o [Visual Studio Code](https://code.visualstudio.com/) con la extensión de C#
- [Docker Desktop](https://www.docker.com/products/docker-desktop/) (para recursos en contenedores)

### Módulos del taller

El taller se divide en 15 módulos (tiempo estimado de completación: 4-6 horas):

1. [Configuración e instalación](./workshop/Lesson-01-Setup/README.md)
1. [Valores predeterminados de servicio](./workshop/Lesson-02-ServiceDefaults/README.md)
1. [Panel de control del desarrollador y orquestación](./workshop/Lesson-03-Dashboard-AppHost/README.md)
1. [Descubrimiento de servicios](./workshop/Lesson-04-ServiceDiscovery/README.md)
1. [Integraciones](./workshop/Lesson-05-Integrations/README.md)
1. [Módulo de telemetría](./workshop/Lesson-06-Telemetry/README.md)
1. [Módulo de base de datos](./workshop/Lesson-07-Database/README.md)
1. [Pruebas de integración](./workshop/Lesson-08-Integration-Testing/README.md)
1. [Despliegue](./workshop/Lesson-09-Deployment/README.md)
1. [Gestión de contenedores](./workshop/Lesson-10-Container-Management/README.md)
1. [Integraciones de Azure](./workshop/Lesson-11-Azure-Integrations/README.md)
1. [Comandos personalizados](./workshop/Lesson-12-Custom-Commands/README.md)
1. [Comprobaciones de salud](./workshop/Lesson-13-HealthChecks/README.md)
1. [Integración de GitHub Models](./workshop/Lesson-14-GitHub-Models-Integration/README.md)
1. [Integración de Docker](./workshop/Lesson-15-Docker-Integration/README.md)

Un conjunto completo de [diapositivas](./workshop/AspireWorkshop.pptx) está disponible para este taller.

### Comenzando

El proyecto inicial para este taller se encuentra en la carpeta `start`. Este proyecto es una API de clima simple que utiliza la API del Servicio Meteorológico Nacional para obtener datos meteorológicos y un frontend web para mostrar los datos meteorológicos impulsado por Blazor.

Para comenzar el taller:

1. Navega a la carpeta `start`
2. Abre el archivo de solución `MyWeatherHub.sln`
3. Sigue las instrucciones en [Módulo 1: Configuración e instalación](./workshop/Lesson-01-Setup/README.md)

## Datos de demostración

Los datos y servicios utilizados en este tutorial provienen del Servicio Meteorológico Nacional de los Estados Unidos (NWS) en <https://weather.gov>. Estamos utilizando su especificación de OpenAPI para consultar pronósticos del clima. La especificación de OpenAPI está [disponible en línea](https://www.weather.gov/documentation/services-web-api). Estamos utilizando solo 2 métodos de esta API y hemos simplificado nuestro código para utilizar solo esos métodos en lugar de crear el cliente completo de OpenAPI para la API de NWS.
