namespace Api;

public record Zone(string Key, string Name, string State);

public record Forecast(string Name, string DetailedForecast);