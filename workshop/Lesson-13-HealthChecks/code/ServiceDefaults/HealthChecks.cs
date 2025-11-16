using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;

namespace ServiceDefaults;

internal static class HealthChecks
{

	public static WebApplication MapDefaultEndpoints(this WebApplication app)
	{
		// Adding health checks endpoints to applications in non-development environments has security implications.
		// See https://aka.ms/dotnet/aspire/healthchecks for details before enabling these endpoints in non-development environments.
		if (app.Environment.IsDevelopment())
		{

			var healthChecks = app.MapGroup("");

			// All health checks must pass for app to be considered ready to accept traffic after starting
			healthChecks.MapHealthChecks("/health", new HealthCheckOptions
			{
				ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
			});

			// Only health checks tagged with the "live" tag must pass for app to be considered alive
			healthChecks.MapHealthChecks("/alive", new HealthCheckOptions
			{
				Predicate = r => r.Tags.Contains("live"),
				ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
			});

			// Add the health checks endpoint for the HealthChecksUI
			var healthChecksUrls = app.Configuration["HEALTHCHECKSUI_URLS"];
			if (!string.IsNullOrWhiteSpace(healthChecksUrls))
			{
				var pathToHostsMap = GetPathToHostsMap(healthChecksUrls);

				foreach (var path in pathToHostsMap.Keys)
				{
					// Ensure that the HealthChecksUI endpoint is only accessible from configured hosts, e.g. localhost:12345, hub.docker.internal, etc.
					// as it contains more detailed information about the health of the app including the types of dependencies it has.

					healthChecks.MapHealthChecks(path, new() { ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse })
							// This ensures that the HealthChecksUI endpoint is only accessible from the configured health checks URLs.
							// See this documentation to learn more about restricting access to health checks endpoints via routing:
							// https://learn.microsoft.com/aspnet/core/host-and-deploy/health-checks?view=aspnetcore-8.0#use-health-checks-routing
							.RequireHost(pathToHostsMap[path]);
				}
			}

		}

		return app;
	}

	private static Dictionary<string, string[]> GetPathToHostsMap(string healthChecksUrls)
	{
		// Given a value like "localhost:12345/healthz;hub.docker.internal:12345/healthz" return a dictionary like:
		// { { "healthz", [ "localhost:12345", "hub.docker.internal:12345" ] } }

		var uris = healthChecksUrls.Split(';', StringSplitOptions.RemoveEmptyEntries)
				.Select(url => new Uri(url, UriKind.Absolute))
				.GroupBy(uri => uri.AbsolutePath, uri => uri.Authority)
				.ToDictionary(g => g.Key, g => g.ToArray());

		return uris;

	}


}
