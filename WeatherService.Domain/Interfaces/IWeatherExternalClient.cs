using WeatherService.Domain.Entities;
using WeatherService.Domain.ValueObjects;

namespace WeatherService.Domain.Interfaces
{
    public interface IWeatherExternalClient
    {
        Task<WeatherRecord> FetchAsync(Coordinates coordinates, CancellationToken cancellationToken = default);
    }
}
