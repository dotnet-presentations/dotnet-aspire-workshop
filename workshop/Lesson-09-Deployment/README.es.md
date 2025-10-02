# Desplegar una aplicación .NET Aspire en Azure Container Apps

.NET Aspire está optimizado para aplicaciones destinadas a ejecutarse en entornos contenerizados. [Azure Container Apps](https://learn.microsoft.com/azure/container-apps/overview) es un entorno completamente administrado que te permite ejecutar microservicios y aplicaciones contenerizadas en una plataforma sin servidor. Este artículo te guiará a través de la creación de una nueva solución .NET Aspire y su despliegue en Microsoft Azure Container Apps usando Visual Studio y la CLI de Desarrollador de Azure (`azd`).

En este ejemplo, asumiremos que estás desplegando la aplicación MyWeatherHub de las secciones anteriores. Puedes usar el código que has construido, o puedes usar el código en el directorio **complete**. Sin embargo, los pasos generales son los mismos para cualquier aplicación .NET Aspire.

## Desplegar la aplicación con Visual Studio

1. En el Explorador de Soluciones, haz clic derecho en el proyecto **AppHost** y selecciona **Publicar** para abrir el diálogo de **Publicación**.

    > Publicar .NET Aspire requiere la versión actual de la CLI `azd`. Esto debería instalarse con la carga de trabajo de .NET Aspire, pero si recibes una notificación de que la CLI no está instalada o actualizada, puedes seguir las instrucciones en la siguiente parte de este tutorial para instalarla.

1. Selecciona **Azure Container Apps para .NET Aspire** como el destino de publicación.

    ![Una captura de pantalla del flujo de trabajo del diálogo de publicación.](../media/vs-deploy.png)

1. En el paso **Ambiente AzDev**, selecciona los valores de **Suscripción** y **Ubicación** deseados y luego ingresa un **Nombre del ambiente** como _aspire-weather_. El nombre del ambiente determina la nomenclatura de los recursos del ambiente de Azure Container Apps.
1. Selecciona **Finalizar** para crear el ambiente, luego **Cerrar** para salir del flujo de trabajo del diálogo y ver el resumen del ambiente de despliegue.
1. Selecciona **Publicar** para aprovisionar y desplegar los recursos en Azure.

    > Este proceso puede tardar varios minutos en completarse. Visual Studio proporciona actualizaciones de estado sobre el progreso del despliegue en los registros de salida y puedes aprender mucho sobre cómo funciona la publicación observando estas actualizaciones! Verás que el proceso implica crear un grupo de recursos, un Registro de Contenedores de Azure, un espacio de trabajo de Log Analytics y un ambiente de Azure Container Apps. La aplicación se despliega entonces en el ambiente de Azure Container Apps.

1. Cuando la publicación se completa, Visual Studio muestra las URLs de los recursos en la parte inferior de la pantalla del ambiente. Usa estos enlaces para ver los diversos recursos desplegados. Selecciona la URL de **webfrontend** para abrir un navegador a la aplicación desplegada.

    ![Una captura de pantalla del proceso de publicación completado y los recursos desplegados.](../media/vs-publish-complete.png)

## Desplegar la aplicación con la CLI de Desarrollador de Azure (azd)

### Instalar la CLI de Desarrollador de Azure

El proceso para instalar `azd` varía según tu sistema operativo, pero está ampliamente disponible a través de `winget`, `brew`, `apt`, o directamente mediante `curl`. Para instalar `azd`, consulta [Instalar la CLI de Desarrollador de Azure](https://learn.microsoft.com/azure/developer/azure-developer-cli/install-azd).

### Nuevo en .NET Aspire 9.4: comando aspire deploy

.NET Aspire 9.4 introduce el comando `aspire deploy` (vista previa/feature flag) que extiende las capacidades de publicación para desplegar activamente en entornos de destino. Este comando proporciona flujos de trabajo de despliegue mejorados con lógica personalizada pre/post-despliegue.

Para habilitar esta característica:

```bash
aspire config set features.deployCommandEnabled true
```

Luego puedes usar:

```bash
aspire deploy
```

Este comando proporciona informes de progreso mejorados, mejores mensajes de error, y soporte para hooks de despliegue personalizados para escenarios de despliegue complejos.

### Inicializar la plantilla

> Prerrequisitos:
>
> - Asegúrate de haber iniciado sesión: ejecuta `azd login` y selecciona la suscripción de Azure correcta.
> - Ejecuta los siguientes comandos desde la carpeta que contiene tu AppHost (para este repositorio, típicamente la carpeta `complete` si estás desplegando el ejemplo terminado).

1. Abre una nueva ventana de terminal y navega (`cd`) a la raíz de tu proyecto .NET Aspire.
1. Ejecuta el comando `azd init` para inicializar tu proyecto con `azd`, el cual inspeccionará la estructura de directorios local y determinará el tipo de aplicación.

    ```console
    azd init
    ```

    Para más información sobre el comando `azd init`, consulta [azd init](https://learn.microsoft.com/azure/developer/azure-developer-cli/reference#azd-init).
1. Si esta es la primera vez que has inicializado la aplicación, `azd` te pedirá el nombre del entorno:

    ```console
    Inicializando una aplicación para ejecutar en Azure (azd init)
    
    ? Ingresa un nuevo nombre de entorno: [? para ayuda]
    ```

    Ingresa el nombre de entorno deseado para continuar. Para más información sobre la gestión de entornos con `azd`, consulta [azd env](https://learn.microsoft.com/azure/developer/azure-developer-cli/reference#azd-env).
1. Selecciona **Usar código en el directorio actual** cuando `azd` te ofrezca dos opciones de inicialización de la aplicación.

  ```console
  ? ¿Cómo deseas inicializar tu aplicación?  [Usa las flechas para moverte, escribe para filtrar]
  > Usar código en el directorio actual
    Seleccionar una plantilla
  ```

1. Después de escanear el directorio, `azd` te pide confirmar que ha encontrado el correcto proyecto _AppHost_ de .NET Aspire. Selecciona la opción **Confirmar y continuar con la inicialización de mi aplicación**.

    ```console
    Servicios detectados:
    
      .NET (Aspire)
      Detectado en: D:\source\repos\letslearn-dotnet-aspire\complete\AppHost\AppHost.csproj
    
    azd generará los archivos necesarios para alojar tu aplicación en Azure usando Azure Container Apps.
    
    ? Selecciona una opción  [Usa las flechas para moverte, escribe para filtrar]
    > Confirmar y continuar con la inicialización de mi aplicación
      Cancelar y salir
    ```

1. `azd` presenta cada uno de los proyectos en la solución .NET Aspire y te pide identificar cuál desplegar con ingreso HTTP abierto públicamente a todo el tráfico de internet. Selecciona solo `myweatherhub` (usando las teclas ↓ y Espacio), ya que quieres que la API sea privada al entorno de Azure Container Apps y _no_ esté disponible públicamente.

    ```console
    ? Selecciona una opción Confirmar y continuar con la inicialización de mi aplicación
    Por defecto, un servicio solo puede ser alcanzado desde dentro del entorno de Azure Container Apps en el que se está ejecutando. Seleccionar un servicio aquí también permitirá que sea alcanzado desde Internet.
    ? Selecciona qué servicios exponer a Internet  [Usa las flechas para moverte, espacio para seleccionar, <derecha> para todos, <izquierda> para ninguno, escribe para filtrar]
      [ ]  apiservice
    > [x]  myweatherhub
    ```

1. Finalmente, especifica el nombre del entorno, que se utiliza para nombrar los recursos provisionados en Azure y gestionar diferentes entornos como `dev` y `prod`.

    ```console
    Generando archivos para ejecutar tu aplicación en Azure:
    
      (✓) Hecho: Generando ./azure.yaml
      (✓) Hecho: Generando ./next-steps.md
    
    ÉXITO: ¡Tu aplicación está lista para la nube!
    Puedes provisionar y desplegar tu aplicación en Azure ejecutando el comando azd up en este directorio. Para más información sobre cómo configurar tu aplicación, consulta ./next-steps.md
    ```

`azd` genera varios archivos y los coloca en el directorio de trabajo. Estos archivos son:

- _azure.yaml_: Describe los servicios de la aplicación, como el proyecto .NET Aspire AppHost, y los mapea a recursos de Azure.
- _.azure/config.json_: Archivo de configuración que informa a `azd` cuál es el entorno activo actual.
- _.azure/aspireazddev/.env_: Contiene sobreescrituras específicas del entorno.
- _.azure/aspireazddev/config.json_: Archivo de configuración que informa a `azd` qué servicios deben tener un punto final público en este entorno.

### Desplegar la aplicación

Una vez que `azd` está inicializado, el proceso de aprovisionamiento y despliegue se puede ejecutar como un solo comando, [azd up](https://learn.microsoft.com/azure/developer/azure-developer-cli/reference#azd-up).

```console
Por defecto, un servicio solo puede ser alcanzado desde dentro del entorno de Azure Container Apps en el que se está ejecutando. Seleccionar un servicio aquí también permitirá que sea alcanzado desde Internet.
? Selecciona qué servicios exponer a Internet webfrontend
? Selecciona una Suscripción de Azure para usar:  1. <TU SUSCRIPCIÓN>
? Selecciona una ubicación de Azure para usar: 1. <TU UBICACIÓN>

Empaquetando servicios (azd package)


ÉXITO: Tu aplicación fue empaquetada para Azure en menos de un segundo.

Aprovisionando recursos de Azure (azd provision)
Aprovisionar recursos de Azure puede tomar algún tiempo.

Suscripción: <TU SUSCRIPCIÓN>
Ubicación: <TU UBICACIÓN>

  Puedes ver el progreso detallado en el Portal de Azure:
<ENLACE AL DESPLIEGUE>

  (✓) Hecho: Grupo de recursos: <TU GRUPO DE RECURSOS>
  (✓) Hecho: Registro de Contenedores: <ID>
  (✓) Hecho: Espacio de trabajo de Log Analytics: <ID>
  (✓) Hecho: Entorno de Apps de Contenedor: <ID>
  (✓) Hecho: App de Contenedor: <ID>

ÉXITO: Tu aplicación fue aprovisionada en Azure en 1 minuto 13 segundos.
Puedes ver los recursos creados bajo el grupo de recursos <TU GRUPO DE RECURSOS> en el Portal de Azure:
<ENLACE A LA VISTA GENERAL DEL GRUPO DE RECURSOS>

Desplegando servicios (azd deploy)

  (✓) Hecho: Desplegando servicio apiservice
  - Endpoint: <TU ÚNICA APP apiservice>.azurecontainerapps.io/

  (✓) Hecho: Desplegando servicio webfrontend
  - Endpoint: <TU ÚNICA APP webfrontend>.azurecontainerapps.io/


ÉXITO: Tu aplicación fue desplegada en Azure en 1 minuto 39 segundos.
Puedes ver los recursos creados bajo el grupo de recursos <TU GRUPO DE RECURSOS> en el Portal de Azure:
<ENLACE A LA VISTA GENERAL DEL GRUPO DE RECURSOS>

ÉXITO: Tu flujo de trabajo de up para aprovisionar y desplegar en Azure se completó en 3 minutos 50 segundos.
```

Primero, los proyectos se empaquetarán en contenedores durante la fase de `azd package`, seguido por la fase de `azd provision` durante la cual se aprovisionan todos los recursos de Azure que la aplicación necesitará.

Una vez que la `provision` esté completa, se llevará a cabo `azd deploy`. Durante esta fase, los proyectos se envían como contenedores a una instancia de Azure Container Registry, y luego se utilizan para crear nuevas revisiones de Azure Container Apps en las cuales se alojará el código.

En este punto, la aplicación ha sido desplegada y configurada, y puedes abrir el portal de Azure y explorar los recursos.

## Limpiar recursos

Ejecuta el siguiente comando de Azure CLI para eliminar el grupo de recursos cuando ya no necesites los recursos de Azure que creaste. Eliminar el grupo de recursos también elimina los recursos contenidos dentro de él.

```console
az group delete --name <nombre-de-tu-grupo-de-recursos>
```

**Siguiente**: [Módulo #10: Gestión Avanzada de Contenedores](../Lesson-10-Container-Management/README.md)
