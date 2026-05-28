using WeatherService.Application.DTOs;

namespace WeatherService.Application.Interfaces
{
    public interface IWeatherService
    {
        Task<WeatherDto> GetByCoordinatesAsync(double latitude, double longitude, CancellationToken cancellationToken = default);
        Task<WeatherDto> GetByCityAsync(string cityName, CancellationToken cancellationToken = default);
    }
}
