using System.Text.Json;
using System.Web;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Extensions.Caching.Memory;
using Api.Data;
using Api.Diagnostics;
using System.Diagnostics;

namespace Api
{
    public class NwsManager(HttpClient httpClient, IMemoryCache cache, IWebHostEnvironment webHostEnvironment, ILogger<NwsManager> logger)
    {
        private static readonly JsonSerializerOptions options = new(JsonSerializerDefaults.Web);
        private static int forecastCount = 0;

        public async Task<Zone[]?> GetZonesAsync()
        {
            using var activity = NwsManagerDiagnostics.activitySource.StartActivity("GetZonesAsync");

            logger.LogInformation("🚀 Starting zones retrieval with {CacheExpiration} cache expiration", TimeSpan.FromHours(1));

            // Check if data exists in cache first
            if (cache.TryGetValue("zones", out Zone[]? cachedZones))
            {
                // Record cache hit
                NwsManagerDiagnostics.cacheHitCounter.Add(1);
                activity?.SetTag("cache.hit", true);
                
                logger.LogInformation("📈 Retrieved {ZoneCount} zones from cache (cache hit)", cachedZones?.Length ?? 0);
                return cachedZones;
            }

            return await cache.GetOrCreateAsync("zones", async entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1);
                
                // Record cache miss when we need to load from file
                NwsManagerDiagnostics.cacheMissCounter.Add(1);
                activity?.SetTag("cache.hit", false);

                var zonesFilePath = Path.Combine(webHostEnvironment.WebRootPath, "zones.json");
                if (!File.Exists(zonesFilePath))
                {
                    logger.LogWarning("⚠️ Zones file not found at {ZonesFilePath}", zonesFilePath);
                    return [];
                }

                using var zonesJson = File.OpenRead(zonesFilePath);
                var zones = await JsonSerializer.DeserializeAsync<ZonesResponse>(zonesJson, options);

                if (zones?.Features == null)
                {
                    logger.LogWarning("⚠️ Failed to deserialize zones from file");
                    return [];
                }

                var filteredZones = zones.Features
                    .Where(f => f.Properties?.ObservationStations?.Count > 0)
                    .Select(f => (Zone)f)
                    .Distinct()
                    .ToArray();

                logger.LogInformation(
                    "📊 Retrieved {TotalZones} zones, {FilteredZones} after filtering (cache miss)",
                    zones.Features.Count,
                    filteredZones.Length
                );

                return filteredZones;
            });
        }

        public async Task<Forecast[]> GetForecastByZoneAsync(string zoneId)
        {
            // Structured logging scope
            using var scope = logger.BeginScope(new Dictionary<string, object>
            {
                ["ZoneId"] = zoneId,
                ["RequestNumber"] = Interlocked.Increment(ref forecastCount)
            });

            NwsManagerDiagnostics.forecastRequestCounter.Add(1);
            var sw = Stopwatch.StartNew();

            using var activity = NwsManagerDiagnostics.activitySource.StartActivity("GetForecastByZoneAsync");
            activity?.SetTag("zone.id", zoneId);

            logger.LogInformation("Starting forecast request for zone {ZoneId}", zoneId);

            try
            {
                // Create an exception every 5 calls to simulate an error for testing
                if (forecastCount % 5 == 0)
                {
                    throw new Exception("Random exception thrown by NwsManager.GetForecastAsync");
                }

                var zoneIdSegment = HttpUtility.UrlEncode(zoneId);
                var forecasts = await httpClient.GetFromJsonAsync<ForecastResponse>($"zones/forecast/{zoneIdSegment}/forecast", options);
                sw.Stop();
                NwsManagerDiagnostics.forecastRequestDuration.Record(sw.Elapsed.TotalSeconds);
                activity?.SetTag("request.success", true);

                var result = forecasts
                    ?.Properties
                    ?.Periods
                    ?.Select(p => (Forecast)p)
                    .ToArray() ?? [];

                logger.LogInformation("Retrieved forecast for zone {ZoneId} in {ElapsedMs}ms with {PeriodCount} periods", zoneId, sw.Elapsed.TotalMilliseconds, result.Length);
                return result;
            }
            catch (HttpRequestException ex)
            {
                sw.Stop();
                NwsManagerDiagnostics.failedRequestCounter.Add(1);
                activity?.SetTag("request.success", false);
                logger.LogError(ex, "Failed HTTP request for zone {ZoneId} after {ElapsedMs}ms", zoneId, sw.Elapsed.TotalMilliseconds);
                throw;
            }
            catch (Exception ex)
            {
                sw.Stop();
                NwsManagerDiagnostics.failedRequestCounter.Add(1);
                activity?.SetTag("request.success", false);
                logger.LogError(ex, "Unexpected error fetching forecast for zone {ZoneId} after {ElapsedMs}ms", zoneId, sw.Elapsed.TotalMilliseconds);
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

            return app;
        }
    }
}