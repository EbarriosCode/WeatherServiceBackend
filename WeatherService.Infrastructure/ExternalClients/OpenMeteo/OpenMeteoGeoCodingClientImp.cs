using Microsoft.Extensions.Logging;
using System.Net.Http.Json;
using WeatherService.Application.Interfaces;
using WeatherService.Domain.ValueObjects;
using WeatherService.Infrastructure.ExternalClients.OpenMeteo.Responses;

namespace WeatherService.Infrastructure.ExternalClients.OpenMeteo
{
    public class OpenMeteoGeoCodingClientImp : IGeoCodingExternalClient
    {
        private readonly ILogger<OpenMeteoGeoCodingClientImp> _logger;
        private readonly HttpClient _httpClient;

        public OpenMeteoGeoCodingClientImp(ILogger<OpenMeteoGeoCodingClientImp> logger, HttpClient httpClient)
        {
            this._logger = logger;
            this._httpClient = httpClient;
        }

        public async Task<Coordinates> GetCoordinatesByCityAsync(string cityName, CancellationToken cancellationToken = default)
        {
            this._logger.LogInformation("Getting Latitude and Longitude from Open-Meteo-GeoCoding for city {CityName}", cityName);

            var url = $"search?name={Uri.EscapeDataString(cityName)}&count=1&language=en&format=json";

            var data = await this._httpClient.GetFromJsonAsync<OpenMeteoGeoResponse>(url, cancellationToken);

            var result = data?.Results.FirstOrDefault() ?? throw new KeyNotFoundException($"City '{cityName}' could not be resolved to coordinates.");

            this._logger.LogInformation("Successfully fetched Coordinates from Open-Meteo-GeoCoding for {CityName}", cityName);

            return Coordinates.Create(result.Latitude, result.Longitude);
        }
    }
}
