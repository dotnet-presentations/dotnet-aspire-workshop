using Aspire.Hosting.Lifecycle;
using System.Diagnostics;

namespace Aspire.Hosting;

/// <summary>
/// A container-based resource for the HealthChecksUI container.
/// See https://github.com/Xabaril/AspNetCore.Diagnostics.HealthChecks?tab=readme-ov-file#HealthCheckUI
/// </summary>
/// <param name="name">The resource name.</param>
public class HealthChecksUIResource(string name) : ContainerResource(name), IResourceWithServiceDiscovery
{
	/// <summary>
	/// The projects to be monitored by the HealthChecksUI container.
	/// </summary>
	public IList<MonitoredProject> MonitoredProjects { get; } = [];

	/// <summary>
	/// Known environment variables for the HealthChecksUI container that can be used to configure the container.
	/// Taken from https://github.com/Xabaril/AspNetCore.Diagnostics.HealthChecks/blob/master/doc/ui-docker.md#environment-variables-table
	/// </summary>
	public static class KnownEnvVars
	{
		public const string UiPath = "ui_path";
		// These keys are taken from https://github.com/Xabaril/AspNetCore.Diagnostics.HealthChecks?tab=readme-ov-file#sample-2-configuration-using-appsettingsjson
		public const string HealthChecksConfigSection = "HealthChecksUI__HealthChecks";
		public const string HealthCheckName = "Name";
		public const string HealthCheckUri = "Uri";

		internal static string GetHealthCheckNameKey(int index) => $"{HealthChecksConfigSection}__{index}__{HealthCheckName}";

		internal static string GetHealthCheckUriKey(int index) => $"{HealthChecksConfigSection}__{index}__{HealthCheckUri}";
	}
}

/// <summary>
/// Represents a project to be monitored by a <see cref="HealthChecksUIResource"/>.
/// </summary>
public class MonitoredProject(IResourceBuilder<ProjectResource> project, string endpointName, string probePath)
{
	private string? _name;

	/// <summary>
	/// The project to be monitored.
	/// </summary>
	public IResourceBuilder<ProjectResource> Project { get; } = project ?? throw new ArgumentNullException(nameof(project));

	/// <summary>
	/// The name of the endpoint the project serves health check details from. If it doesn't exist it will be added.
	/// </summary>
	public string EndpointName { get; } = endpointName ?? throw new ArgumentNullException(nameof(endpointName));

	/// <summary>
	/// The name of the project to be displayed in the HealthChecksUI dashboard. Defaults to the project resource's name.
	/// </summary>
	public string Name
	{
		get => _name ?? Project.Resource.Name;
		set { _name = value; }
	}

	/// <summary>
	/// The request path the project serves health check details for the HealthChecksUI dashboard from.
	/// </summary>
	public string ProbePath { get; set; } = probePath ?? throw new ArgumentNullException(nameof(probePath));
}

internal class HealthChecksUILifecycleHook(DistributedApplicationExecutionContext executionContext) : IDistributedApplicationLifecycleHook
{
	private const string HEALTHCHECKSUI_URLS = "HEALTHCHECKSUI_URLS";

	public Task BeforeStartAsync(DistributedApplicationModel appModel, CancellationToken cancellationToken = default)
	{
		// Configure each project referenced by a Health Checks UI resource
		var healthChecksUIResources = appModel.Resources.OfType<HealthChecksUIResource>();

		foreach (var healthChecksUIResource in healthChecksUIResources)
		{
			foreach (var monitoredProject in healthChecksUIResource.MonitoredProjects)
			{
				var project = monitoredProject.Project;

				// Add the health check endpoint if it doesn't exist
				var healthChecksEndpoint = project.GetEndpoint(monitoredProject.EndpointName);
				if (!healthChecksEndpoint.Exists)
				{
					project.WithHttpEndpoint(name: monitoredProject.EndpointName);
					Debug.Assert(healthChecksEndpoint.Exists, "The health check endpoint should exist after adding it.");
				}

				// Set environment variable to configure the URLs the health check endpoint is accessible from
				project.WithEnvironment(context =>
				{
					var probePath = monitoredProject.ProbePath.TrimStart('/');
					var healthChecksEndpointsExpression = ReferenceExpression.Create($"{healthChecksEndpoint}/{probePath}");

					if (context.ExecutionContext.IsRunMode)
					{
						// Running during dev inner-loop
						var containerHost = healthChecksUIResource.GetEndpoint("http").ContainerHost;
						var fromContainerUriBuilder = new UriBuilder(healthChecksEndpoint.Url)
						{
							Host = containerHost,
							Path = monitoredProject.ProbePath
						};

						healthChecksEndpointsExpression = ReferenceExpression.Create($"{healthChecksEndpointsExpression};{fromContainerUriBuilder.ToString()}");
					}

					context.EnvironmentVariables.Add(HEALTHCHECKSUI_URLS, healthChecksEndpointsExpression);
				});
			}
		}

		if (executionContext.IsPublishMode)
		{
			ConfigureHealthChecksUIContainers(appModel.Resources, isPublishing: true);
		}

		return Task.CompletedTask;
	}

	public Task AfterEndpointsAllocatedAsync(DistributedApplicationModel appModel, CancellationToken cancellationToken = default)
	{
		ConfigureHealthChecksUIContainers(appModel.Resources, isPublishing: false);

		return Task.CompletedTask;
	}

	private static void ConfigureHealthChecksUIContainers(IResourceCollection resources, bool isPublishing)
	{
		var healhChecksUIResources = resources.OfType<HealthChecksUIResource>();

		foreach (var healthChecksUIResource in healhChecksUIResources)
		{
			var monitoredProjects = healthChecksUIResource.MonitoredProjects;

			// Add environment variables to configure the HealthChecksUI container with the health checks endpoints of each referenced project
			// See example configuration at https://github.com/Xabaril/AspNetCore.Diagnostics.HealthChecks?tab=readme-ov-file#sample-2-configuration-using-appsettingsjson
			for (var i = 0; i < monitoredProjects.Count; i++)
			{
				var monitoredProject = monitoredProjects[i];
				var healthChecksEndpoint = monitoredProject.Project.GetEndpoint(monitoredProject.EndpointName);

				// Set health check name
				var nameEnvVarName = HealthChecksUIResource.KnownEnvVars.GetHealthCheckNameKey(i);
				healthChecksUIResource.Annotations.Add(
						new EnvironmentCallbackAnnotation(
								nameEnvVarName,
								() => monitoredProject.Name));

				// Set health check URL
				var probePath = monitoredProject.ProbePath.TrimStart('/');
				var urlEnvVarName = HealthChecksUIResource.KnownEnvVars.GetHealthCheckUriKey(i);

				healthChecksUIResource.Annotations.Add(
						new EnvironmentCallbackAnnotation(
								context => context[urlEnvVarName] = isPublishing
										? ReferenceExpression.Create($"{healthChecksEndpoint}/{probePath}")
										: new HostUrl($"{healthChecksEndpoint.Url}/{probePath}")));
			}
		}
	}
}



public static class HealthChecksUIExtensions
{
	/// <summary>
	/// Adds a HealthChecksUI container to the application model.
	/// </summary>
	/// <param name="builder">The builder.</param>
	/// <param name="name">The resource name.</param>
	/// <param name="port">The host port to expose the container on.</param>
	/// <param name="tag">The tag to use for the container image. Defaults to <c>"5.0.0"</c>.</param>
	/// <returns>The resource builder.</returns>
	public static IResourceBuilder<HealthChecksUIResource> AddHealthChecksUI(
			this IDistributedApplicationBuilder builder,
			string name,
			int? port = null)
	{
		builder.Services.TryAddLifecycleHook<HealthChecksUILifecycleHook>();

		var resource = new HealthChecksUIResource(name);

		return builder
				.AddResource(resource)
				.WithImage(HealthChecksUIDefaults.ContainerImageName, HealthChecksUIDefaults.ContainerImageTag)
				.WithImageRegistry(HealthChecksUIDefaults.ContainerRegistry)
				.WithEnvironment(HealthChecksUIResource.KnownEnvVars.UiPath, "/")
				.WithHttpEndpoint(port: port, targetPort: HealthChecksUIDefaults.ContainerPort);
	}

	/// <summary>
	/// Adds a reference to a project that will be monitored by the HealthChecksUI container.
	/// </summary>
	/// <param name="builder">The builder.</param>
	/// <param name="project">The project.</param>
	/// <param name="endpointName">
	/// The name of the HTTP endpoint the <see cref="ProjectResource"/> serves health check details from.
	/// The endpoint will be added if it is not already defined on the <see cref="ProjectResource"/>.
	/// </param>
	/// <param name="probePath">The request path the project serves health check details from.</param>
	/// <returns>The resource builder.</returns>
	public static IResourceBuilder<HealthChecksUIResource> WithReference(
			this IResourceBuilder<HealthChecksUIResource> builder,
			IResourceBuilder<ProjectResource> project,
			string endpointName = HealthChecksUIDefaults.EndpointName,
			string probePath = HealthChecksUIDefaults.ProbePath)
	{
		var monitoredProject = new MonitoredProject(project, endpointName: endpointName, probePath: probePath);
		builder.Resource.MonitoredProjects.Add(monitoredProject);

		return builder;
	}


}

/// <summary>
/// Default values used by <see cref="HealthChecksUIResource">.
/// </summary>
public static class HealthChecksUIDefaults
{
	/// <summary>
	/// The default container registry to pull the HealthChecksUI container image from.
	/// </summary>
	public const string ContainerRegistry = "docker.io";

	/// <summary>
	/// The default container image name to use for the HealthChecksUI container.
	/// </summary>
	public const string ContainerImageName = "xabarilcoding/healthchecksui";

	/// <summary>
	/// The default container image tag to use for the HealthChecksUI container.
	/// </summary>
	public const string ContainerImageTag = "5.0.0";

	/// <summary>
	/// The target port the HealthChecksUI container listens on.
	/// </summary>
	public const int ContainerPort = 80;

	/// <summary>
	/// The default request path projects serve health check details from.
	/// </summary>
	public const string ProbePath = "/health";

	/// <summary>
	/// The default name of the HTTP endpoint projects serve health check details from.
	/// </summary>
	public const string EndpointName = "healthchecks";
}
