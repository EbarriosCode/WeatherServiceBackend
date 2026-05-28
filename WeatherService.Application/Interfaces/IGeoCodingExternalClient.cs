using WeatherService.Domain.ValueObjects;

namespace WeatherService.Application.Interfaces
{
    public interface IGeoCodingExternalClient
    {
        Task<Coordinates> GetCoordinatesByCityAsync(string cityName, CancellationToken cancellationToken = default);
    }
}
