namespace WeatherService.Application.DTOs
{
    public record WeatherDto(
        double Temperature,
        double WindSpeed,
        double WindDirection,
        DateTime Sunrise,
        double Latitude,
        double Longitude,
        DateTime FetchedAt,
        bool FromCache
    );
}
