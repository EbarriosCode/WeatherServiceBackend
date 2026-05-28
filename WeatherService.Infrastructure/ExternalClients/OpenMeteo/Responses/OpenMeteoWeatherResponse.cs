using System.Text.Json.Serialization;

namespace WeatherService.Infrastructure.ExternalClients.OpenMeteo.Responses
{
    public class OpenMeteoWeatherResponse
    {
        [JsonPropertyName("latitude")]
        public double Latitude { get; init; }

        [JsonPropertyName("longitude")]
        public double Longitude { get; init; }

        [JsonPropertyName("current")]
        public CurrentWeather CurrentWeather { get; init; } = default!;

        [JsonPropertyName("daily")]
        public DailyData Daily { get; init; } = default!;
    }

    public class CurrentWeather
    {
        [JsonPropertyName("time")]
        public string Time { get; init; } = default!;

        [JsonPropertyName("interval")]
        public int Interval { get; init; }

        [JsonPropertyName("temperature_2m")]
        public double Temperature { get; init; }

        [JsonPropertyName("wind_speed_10m")]
        public double WindSpeed { get; init; }

        [JsonPropertyName("wind_direction_10m")]
        public double WindDirection { get; init; }        
    }

    public class DailyData
    {
        [JsonPropertyName("sunrise")]
        public List<string> Sunrise { get; init; } = new();
    }
}
