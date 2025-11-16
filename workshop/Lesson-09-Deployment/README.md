# Deploy a Aspire app to Azure Container Apps

Aspire is optimized for apps that are destined to run in containerized environments. [Azure Container Apps](https://learn.microsoft.com/azure/container-apps/overview) is a fully managed environment that enables you to run microservices and containerized applications on a serverless platform. This article will walk you through creating a new Aspire solution and deploying it to Microsoft Azure Container Apps using Visual Studio and the Azure Developer CLI (`azd`).

In this example, we'll assume you're deploying the MyWeatherHub app from the previous sections. You can use the code you've built, or you can use the code in the **complete** directory. However, the general steps are the same for any Aspire app.

## Deploy the app with Visual Studio

1. In the Solution Explorer, right-click on the **AppHost** project and select **Publish** to open the **Publish** dialog.

    > Publishing Aspire requires the current version of the `azd` CLI. This should be installed with the Aspire workload, but if you get a notification that the CLI is not installed or up to date, you can follow the directions in the next part of this tutorial to install it.

1. Select **Azure Container Apps for Aspire** as the publishing target.

    ![A screenshot of the publishing dialog workflow.](../media/vs-deploy.png)

1. On the **AzDev Environment** step, select your desired **Subscription** and **Location** values and then enter an **Environment name** such as _aspire-weather_. The environment name determines the naming of Azure Container Apps environment resources.
1. Select **Finish** to create the environment, then **Close** to exit the dialog workflow and view the deployment environment summary.
1. Select **Publish** to provision and deploy the resources on Azure.

    > This process may take several minutes to complete. Visual Studio provides status updates on the deployment progress in the output logs and you can learn a lot about how publishing works by watching these updates! You'll see that the process involves creating a resource group, an Azure Container Registry, a Log Analytics workspace, and an Azure Container Apps environment. The app is then deployed to the Azure Container Apps environment.

1. When the publish completes, Visual Studio displays the resource URLs at the bottom of the environment screen. Use these links to view the various deployed resources. Select the **webfrontend** URL to open a browser to the deployed app.

    ![A screenshot of the completed publishing process and deployed resources.](../media/vs-publish-complete.png)

## Deploy the app with the Azure Developer CLI (azd)

### Install the Azure Developer CLI

The process for installing `azd` varies based on your operating system, but it is widely available via `winget`, `brew`, `apt`, or directly via `curl`. To install `azd`, see [Install Azure Developer CLI](https://learn.microsoft.com/azure/developer/azure-developer-cli/install-azd).

### Experimental Feature: aspire deploy Command

> [!CAUTION]
>
> This feature is under construction and is not stable.  This information is shared for informational purposes, and we recommend you keep an eye on the [.NET Blog](https://devblogs.microsoft.com/dotnet) for updates to the Aspire CLI that will include updates to this feature.

Aspire 9.4 introduces the `aspire deploy` command (preview/feature flag) that extends publishing capabilities to actively deploy to target environments. This command provides enhanced deployment workflows with custom pre/post-deploy logic.

To enable this feature:

```bash
aspire config set features.deployCommandEnabled true
```

Then you can use:

```bash
aspire deploy
```

This command provides enhanced progress reporting, better error messaging, and supports custom deployment hooks for complex deployment scenarios.

### Initialize the template

> Prerequisites:
>
> - Make sure you're signed in: run `azd login` and select the correct Azure subscription.
> - Run the following commands from the folder that contains your AppHost (for this repo, typically the `complete` folder if you're deploying the finished sample).

1. Open a new terminal window and `cd` into the root of your Aspire project.
1. Execute the `azd init` command to initialize your project with `azd`, which will inspect the local directory structure and determine the type of app.

    ```console
    azd init
    ```

    For more information on the `azd init` command, see [azd init](https://learn.microsoft.com/azure/developer/azure-developer-cli/reference#azd-init).
1. If this is the first time you've initialized the app, `azd` prompts you for the environment name:

    ```console
    Initializing an app to run on Azure (azd init)
    
    ? Enter a new environment name: [? for help]
    ```

    Enter the desired environment name to continue. For more information on managing environments with `azd`, see [azd env](https://learn.microsoft.com/azure/developer/azure-developer-cli/reference#azd-env).
1. Select **Use code in the current directory** when `azd` prompts you with two app initialization options.

    ```console
    ? How do you want to initialize your app?  [Use arrows to move, type to filter]
    > Use code in the current directory
      Select a template
    ```

1. After scanning the directory, `azd` prompts you to confirm that it found the correct Aspire _AppHost_ project. Select the **Confirm and continue initializing my app** option.

    ```console
    Detected services:
    
      .NET (Aspire)
      Detected in: D:\source\repos\letslearn-dotnet-aspire\complete\AppHost\AppHost.csproj
    
    azd will generate the files necessary to host your app on Azure using Azure Container Apps.
    
    ? Select an option  [Use arrows to move, type to filter]
    > Confirm and continue initializing my app
      Cancel and exit
    ```

1. `azd` presents each of the projects in the Aspire solution and prompts you to identify which to deploy with HTTP ingress open publicly to all internet traffic. Select only the `myweatherhub` (using the ↓ and Space keys), since you want the API (`api`) to be private to the Azure Container Apps environment and not available publicly.

    ```console
    ? Select an option Confirm and continue initializing my app
    By default, a service can only be reached from inside the Azure Container Apps environment it is running in. Selecting a service here will also allow it to be reached from the Internet.
    ? Select which services to expose to the Internet  [Use arrows to move, space to select, <right> to all, <left> to none, type to filter]
          [ ]  api
        > [x]  myweatherhub
    ```

1. Finally, specify the the environment name, which is used to name provisioned resources in Azure and managing different environments such as `dev` and `prod`.

    ```console
    Generating files to run your app on Azure:
    
      (✓) Done: Generating ./azure.yaml
      (✓) Done: Generating ./next-steps.md
    
    SUCCESS: Your app is ready for the cloud!
    You can provision and deploy your app to Azure by running the azd up command in this directory. For more information on configuring your app, see ./next-steps.md
    ```

`azd` generates a number of files and places them into the working directory. These files are:

- _azure.yaml_: Describes the services of the app, such as Aspire AppHost project, and maps them to Azure resources.
- _.azure/config.json_: Configuration file that informs `azd` what the current active environment is.
- _.azure/aspireazddev/.env_: Contains environment specific overrides.
- _.azure/aspireazddev/config.json_: Configuration file that informs `azd` which services should have a public endpoint in this environment.

[](https://learn.microsoft.com/dotnet/aspire/deployment/azure/aca-deployment?tabs=visual-studio%2Cinstall-az-windows%2Cpowershell&pivots=azure-azd#deploy-the-app)

### Deploy the app

Once `azd` is initialized, the provisioning and deployment process can be executed as a single command, [azd up](https://learn.microsoft.com/azure/developer/azure-developer-cli/reference#azd-up).

```console
By default, a service can only be reached from inside the Azure Container Apps environment it is running in. Selecting a service here will also allow it to be reached from the Internet.
? Select which services to expose to the Internet webfrontend
? Select an Azure Subscription to use:  1. <YOUR SUBSCRIPTION>
? Select an Azure location to use: 1. <YOUR LOCATION>

Packaging services (azd package)


SUCCESS: Your application was packaged for Azure in less than a second.

Provisioning Azure resources (azd provision)
Provisioning Azure resources can take some time.

Subscription: <YOUR SUBSCRIPTION>
Location: <YOUR LOCATION>

  You can view detailed progress in the Azure Portal:
<LINK TO DEPLOYMENT>

  (✓) Done: Resource group: <YOUR RESOURCE GROUP>
  (✓) Done: Container Registry: <ID>
  (✓) Done: Log Analytics workspace: <ID>
  (✓) Done: Container Apps Environment: <ID>
  (✓) Done: Container App: <ID>

SUCCESS: Your application was provisioned in Azure in 1 minute 13 seconds.
You can view the resources created under the resource group <YOUR RESOURCE GROUP> in Azure Portal:
<LINK TO RESOURCE GROUP OVERVIEW>

Deploying services (azd deploy)

  (✓) Done: Deploying service api
  - Endpoint: <internal-only>

  (✓) Done: Deploying service myweatherhub
  - Endpoint: <YOUR UNIQUE myweatherhub APP>.azurecontainerapps.io/


SUCCESS: Your application was deployed to Azure in 1 minute 39 seconds.
You can view the resources created under the resource group <YOUR RESOURCE GROUP> in Azure Portal:
<LINK TO RESOURCE GROUP OVERVIEW>

SUCCESS: Your up workflow to provision and deploy to Azure completed in 3 minutes 50 seconds.
```

First, the projects will be packaged into containers during the `azd package` phase, followed by the `azd provision` phase during which all of the Azure resources the app will need are provisioned.

Once `provision` is complete, `azd deploy` will take place. During this phase, the projects are pushed as containers into an Azure Container Registry instance, and then used to create new revisions of Azure Container Apps in which the code will be hosted.

At this point the app has been deployed and configured, and you can open the Azure portal and explore the resources.

## Clean up resources

Run the following Azure CLI command to delete the resource group when you no longer need the Azure resources you created. Deleting the resource group also deletes the resources contained inside of it.

```console
az group delete --name <your-resource-group-name>
```

**Next**: [Module #10: Advanced Container Management](../Lesson-10-Container-Management/README.md)
