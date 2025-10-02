using System.Text.Json;
using System.Web;

namespace MyWeatherHub;

public class NwsManager(HttpClient client)
{
    private static readonly JsonSerializerOptions options = new(JsonSerializerDefaults.Web);

    public async Task<Zone[]> GetZonesAsync()
    {
        var zones = await client.GetFromJsonAsync<Zone[]>("zones", options);

        return zones ?? [];
    }

    public async Task<Forecast[]> GetForecastByZoneAsync(string zoneId)
    {
        var forecast = await client.GetFromJsonAsync<Forecast[]>($"forecast/{HttpUtility.UrlEncode(zoneId)}", options);

        return forecast ?? [];
    }
}

public record Zone(string Key, string Name, string State);

public record Forecast(string Name, string DetailedForecast);
