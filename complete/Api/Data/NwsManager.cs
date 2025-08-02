using System.Text.Json;
using System.Web;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Extensions.Caching.Memory;
using Api.Data;
using Api.Diagnostics;
using System.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.OutputCaching;
using Microsoft.AspNetCore.Mvc;

namespace Api
{
	public class NwsManager(
							HttpClient httpClient,
							IMemoryCache cache,
							IWebHostEnvironment webHostEnvironment,
							ILogger<NwsManager> logger)
	{
		private static readonly JsonSerializerOptions options = new(JsonSerializerDefaults.Web);

		public async Task<Zone[]?> GetZonesAsync()
		{
			using var activity = NwsManagerDiagnostics.activitySource.StartActivity("GetZonesAsync");

			logger.LogInformation("🚀 Starting zones retrieval with {CacheExpiration} cache expiration", TimeSpan.FromHours(1));

			return await cache.GetOrCreateAsync("zones", async entry =>
			{
				entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1);

				var zonesFilePath = Path.Combine(webHostEnvironment.WebRootPath, "zones.json");
				if (!File.Exists(zonesFilePath))
				{
					logger.LogWarning("⚠️ Zones file not found at {ZonesFilePath}", zonesFilePath);
					activity?.SetTag("cache.hit", false);
					return [];
				}

				using var zonesJson = File.OpenRead(zonesFilePath);
				var zones = await JsonSerializer.DeserializeAsync<ZonesResponse>(zonesJson, options);

				if (zones?.Features == null)
				{
					logger.LogWarning("⚠️ Failed to deserialize zones from file");
					activity?.SetTag("cache.hit", false);
					return [];
				}

				var filteredZones = zones.Features
									.Where(f => f.Properties?.ObservationStations?.Count > 0)
									.Select(f => (Zone)f)
									.Distinct()
									.ToArray();

				logger.LogInformation(
									"📊 Retrieved {TotalZones} zones, {FilteredZones} after filtering observation stations",
									zones.Features.Count,
									filteredZones.Length
							);

				activity?.SetTag("cache.hit", true);

				return filteredZones;
			});
		}

		private static int forecastCount = 0;

		public async Task<Forecast[]> GetForecastByZoneAsync(string zoneId)
		{
			using var logScope = logger.BeginScope(new Dictionary<string, object>
			{
				["ZoneId"] = zoneId,
				["RequestNumber"] = Interlocked.Increment(ref forecastCount)
			});

			NwsManagerDiagnostics.forecastRequestCounter.Add(1);
			var stopwatch = Stopwatch.StartNew();

			using var activity = NwsManagerDiagnostics.activitySource.StartActivity("GetForecastByZoneAsync");
			activity?.SetTag("zone.id", zoneId);

			logger.LogInformation("🚀 Starting forecast request for zone {ZoneId}", zoneId);

			// Create an exception every 5 calls to simulate an error for testing
			if (forecastCount % 5 == 0)
			{
				logger.LogError(
						"❌ Simulated error on request {RequestCount} for zone {ZoneId}",
						forecastCount,
						zoneId
				);
				NwsManagerDiagnostics.failedRequestCounter.Add(1);
				activity?.SetTag("request.success", false);
				throw new Exception("Random exception thrown by NwsManager.GetForecastAsync");
			}

			try
			{
				var zoneIdSegment = HttpUtility.UrlEncode(zoneId);
				var zoneUrl = $"/zones/forecast/{zoneIdSegment}/forecast";

				logger.LogDebug(
						"🔍 Requesting forecast from {Url}",
						zoneUrl
				);

				var forecasts = await httpClient.GetFromJsonAsync<ForecastResponse>(zoneUrl, options);

				stopwatch.Stop();
				var duration = stopwatch.Elapsed;
				NwsManagerDiagnostics.forecastRequestDuration.Record(duration.TotalSeconds);
				activity?.SetTag("request.success", true);

				logger.LogInformation(
						"📊 Retrieved forecast for zone {ZoneId} in {Duration:N0}ms with {PeriodCount} periods",
						zoneId,
						duration.TotalMilliseconds,
						forecasts?.Properties?.Periods?.Count ?? 0
				);

				return forecasts
							 ?.Properties
							 ?.Periods
							 ?.Select(p => (Forecast)p)
							 .ToArray() ?? [];
			}
			catch (HttpRequestException ex)
			{
				logger.LogError(
						ex,
						"❌ Failed to retrieve forecast for zone {ZoneId}. Status: {StatusCode}",
						zoneId,
						ex.StatusCode
				);
				NwsManagerDiagnostics.failedRequestCounter.Add(1);
				activity?.SetTag("request.success", false);
				throw;
			}
		}
	}
}

namespace Microsoft.Extensions.DependencyInjection
{
	public static class NwsManagerExtensions
	{
		public static IServiceCollection AddNwsManager(this IServiceCollection services)
		{
			services.AddHttpClient<Api.NwsManager>(client =>
			{
				client.BaseAddress = new Uri("https://weather-api");
				client.DefaultRequestHeaders.Add("User-Agent", "Microsoft - .NET Aspire Demo");
			});

			services.AddMemoryCache();

			// Add default output caching
			services.AddOutputCache(options =>
			{
				options.AddBasePolicy(builder => builder.Tag("AllCache")
								.Cache());
			});

			return services;
		}

		public static WebApplication? MapApiEndpoints(this WebApplication app)
		{
			app.UseOutputCache();

			app.MapGet("/zones", async (Api.NwsManager manager) =>
			{
				var zones = await manager.GetZonesAsync();
				return TypedResults.Ok(zones);
			})
					.CacheOutput(policy => policy.Expire(TimeSpan.FromHours(1)))
					.WithName("GetZones")
					.WithOpenApi();

			app.MapGet("/forecast/{zoneId}", async Task<Results<Ok<Api.Forecast[]>, NotFound>> (Api.NwsManager manager, string zoneId) =>
			{
				try
				{
					var forecasts = await manager.GetForecastByZoneAsync(zoneId);
					return TypedResults.Ok(forecasts);
				}
				catch (HttpRequestException)
				{
					return TypedResults.NotFound();
				}
			})
					.CacheOutput(policy => policy.Expire(TimeSpan.FromMinutes(15)).SetVaryByRouteValue("zoneId"))
					.WithName("GetForecastByZone")
					.WithOpenApi();

			app.MapPost("/cache/invalidate", static async (
					[FromHeader(Name = "X-CacheInvalidation-Key")] string? header,
					IOutputCacheStore cacheStore,
					IConfiguration config) =>
			{
				var hasValidHeader = config.GetValue<string>("ApiCacheInvalidationKey") is { } key
						&& header == $"Key: {key}";

				if (hasValidHeader is false)
				{
					return Results.Unauthorized();
				}

				await cacheStore.EvictByTagAsync("AllCache", CancellationToken.None);

				return Results.Ok();
			});

			return app;
		}
	}
}
