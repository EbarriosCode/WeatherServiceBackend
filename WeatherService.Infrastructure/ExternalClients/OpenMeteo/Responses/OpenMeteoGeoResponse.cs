using System.Text.Json.Serialization;

namespace WeatherService.Infrastructure.ExternalClients.OpenMeteo.Responses
{
    public class OpenMeteoGeoResponse
    {
        [JsonPropertyName("results")]
        public List<GeoResult> Results { get; init; } = new();
    }

    public class GeoResult
    {
        [JsonPropertyName("name")]
        public string Name { get; init; } = default!;

        [JsonPropertyName("latitude")]
        public double Latitude { get; init; }

        [JsonPropertyName("longitude")]
        public double Longitude { get; init; }

        [JsonPropertyName("country")]
        public string Country { get; init; } = default!;
    }
}
