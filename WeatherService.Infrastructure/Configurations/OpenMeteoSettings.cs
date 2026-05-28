namespace WeatherService.Infrastructure.Configurations
{
    public class OpenMeteoSettings
    {
        public const string SectionName = "OpenMeteo";

        public string BaseUrl { get; init; } = default!;
        public string GeoCodingBaseUrl { get; init; } = default!;
    }
}
