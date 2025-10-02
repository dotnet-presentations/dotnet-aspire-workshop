namespace Api.Data;

public class ForecastResponse
{
    public string? Id { get; set; }
    public string? Type { get; set; }
    public ForecastProperties? Properties { get; set; }

    public class ForecastProperties
    {
        public string? Geometry { get; set; }
        public string? Zone { get; set; }
        public DateTime Updated { get; set; }
        public List<Period>? Periods { get; set; }
    }

    public class Period
    {
        public int Number { get; set; }
        public string? Name { get; set; }
        public string? DetailedForecast { get; set; }

        public static explicit operator Forecast(Period period)
            => new(period.Name ?? string.Empty, period.DetailedForecast ?? string.Empty);
    }
}
