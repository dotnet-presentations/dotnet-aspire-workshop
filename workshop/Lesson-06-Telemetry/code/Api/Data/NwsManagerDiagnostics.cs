using System.Diagnostics;
using System.Diagnostics.Metrics;

namespace Api.Diagnostics;

public static class NwsManagerDiagnostics
{
    private static readonly Meter meter = new("NwsManagerMetrics", "1.0");

    public static readonly Counter<int> forecastRequestCounter = meter.CreateCounter<int>(
        name: "forecast_requests_total",
        unit: "requests",
        description: "Total number of forecast requests");

    public static readonly Histogram<double> forecastRequestDuration = meter.CreateHistogram<double>(
        name: "forecast_request_duration_seconds",
        unit: "s",
        description: "Histogram of forecast request durations in seconds");

    public static readonly Counter<int> failedRequestCounter = meter.CreateCounter<int>(
        name: "failed_requests_total",
        unit: "requests",
        description: "Total number of failed forecast requests");

    public static readonly Counter<int> cacheHitCounter = meter.CreateCounter<int>(
        name: "cache_hits_total",
        unit: "items",
        description: "Total number of cache hits");

    public static readonly Counter<int> cacheMissCounter = meter.CreateCounter<int>(
        name: "cache_misses_total",
        unit: "items",
        description: "Total number of cache misses");

    public static readonly ActivitySource activitySource = new("NwsManager");
}
