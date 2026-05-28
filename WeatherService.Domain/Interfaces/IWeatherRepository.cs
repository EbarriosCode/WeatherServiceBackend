using WeatherService.Domain.Entities;
using WeatherService.Domain.ValueObjects;

namespace WeatherService.Domain.Interfaces
{
    public interface IWeatherRepository
    {
        Task<WeatherRecord?> GetByCoordinatesAsync(Coordinates coordinates, CancellationToken cancellationToken = default);
        Task SaveAsync(WeatherRecord record, CancellationToken cancellationToken = default);
    }
}
