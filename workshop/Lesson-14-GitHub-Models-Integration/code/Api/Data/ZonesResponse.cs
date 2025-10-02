using System.Text.Json.Serialization;

namespace Api.Data;

public class ZonesResponse
{
    public string? Type { get; set; }
    public List<Feature>? Features { get; set; }

    public class Feature
    {
        public string? Id { get; set; }
        public string? Type { get; set; }
        public Properties? Properties { get; set; }

        public static explicit operator Zone(Feature feature)
            => new(feature?.Properties?.Key ?? string.Empty, feature?.Properties?.Name ?? string.Empty, feature?.Properties?.State ?? string.Empty);

    }

    public class Properties
    {
        [JsonPropertyName("@id")]
        public string? Id { get; set; }
        [JsonPropertyName("id")]
        public string? Key { get; set; }
        public string? Type { get; set; }
        public string? Name { get; set; }
        public DateTime? EffectiveDate { get; set; }
        public DateTime? ExpirationDate { get; set; }
        public string? State { get; set; }
        public List<string>? Cwa { get; set; }
        public List<string>? ForecastOffices { get; set; }
        public List<string>? TimeZone { get; set; }
        public List<string>? ObservationStations { get; set; }
        public string? RadarStation { get; set; }
    }
}
