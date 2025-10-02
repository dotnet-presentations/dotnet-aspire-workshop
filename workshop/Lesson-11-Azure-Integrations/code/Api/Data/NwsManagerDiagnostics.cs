using System.Diagnostics;
using System.Diagnostics.Metrics;

namespace Api.Diagnostics
{
    public class NwsManagerDiagnostics
    {
        private static readonly Meter meter = new Meter("NwsManagerMetrics", "1.0");
        public static readonly Counter<int> forecastRequestCounter = meter.CreateCounter<int>("forecast_requests_total", "Total number of forecast requests");
        public static readonly Histogram<double> forecastRequestDuration = meter.CreateHistogram<double>("forecast_request_duration_seconds", "Histogram of forecast request durations");
        public static readonly Counter<int> failedRequestCounter = meter.CreateCounter<int>("failed_requests_total", "Total number of failed requests");
        public static readonly Counter<int> cacheHitCounter = meter.CreateCounter<int>("cache_hits_total", "Total number of cache hits");
        public static readonly Counter<int> cacheMissCounter = meter.CreateCounter<int>("cache_misses_total", "Total number of cache misses");
        public static readonly ActivitySource activitySource = new ActivitySource("NwsManager");
    }
}
