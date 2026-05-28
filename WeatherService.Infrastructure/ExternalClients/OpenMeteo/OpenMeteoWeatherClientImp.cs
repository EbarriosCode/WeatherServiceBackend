using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net.Http.Json;
using WeatherService.Domain.Entities;
using WeatherService.Domain.Interfaces;
using WeatherService.Domain.ValueObjects;
using WeatherService.Infrastructure.Configurations;
using WeatherService.Infrastructure.ExternalClients.OpenMeteo.Responses;

namespace WeatherService.Infrastructure.ExternalClients.OpenMeteo
{
    public class OpenMeteoWeatherClientImp : IWeatherExternalClient
    {
        private readonly ILogger<OpenMeteoWeatherClientImp> _logger;
        private readonly HttpClient _httpClient;

        public OpenMeteoWeatherClientImp(ILogger<OpenMeteoWeatherClientImp> logger, HttpClient httpClient, IOptions<OpenMeteoSettings> settings)
        {
            this._logger = logger;
            this._httpClient = httpClient;
        }

        public async Task<WeatherRecord> FetchAsync(Coordinates coordinates, CancellationToken cancellationToken = default)
        {
            this._logger.LogInformation("Fetching weather from Open-Meteo-Weather for coordinates {CacheKey}", coordinates.ToCacheKey());

            var url = BuildRelativeUrl(coordinates);

            var data = await this._httpClient.GetFromJsonAsync<OpenMeteoWeatherResponse>(url, cancellationToken)
                       ?? throw new InvalidOperationException("Empty response from Open-Meteo weather API.");

            this._logger.LogInformation("Successfully fetched weather from Open-Meteo-Weather for {CacheKey}", coordinates.ToCacheKey());

            return MapToEntity(data, coordinates);
        }

        private static string BuildRelativeUrl(Coordinates coordinates)
         =>
            $"forecast" +
            $"?latitude={coordinates.Latitude}" +
            $"&longitude={coordinates.Longitude}" +
            $"&current=temperature_2m,wind_speed_10m,wind_direction_10m" +
            $"&daily=sunrise" +
            $"&timezone=auto";

        private static WeatherRecord MapToEntity(OpenMeteoWeatherResponse data, Coordinates coordinates)
        {
            var sunriseRaw = data.Daily.Sunrise.FirstOrDefault() ?? throw new InvalidOperationException("Sunrise data missing from Open-Meteo response.");

            return new WeatherRecord
            {
                Id = string.Empty,
                Latitude = coordinates.Latitude,
                Longitude = coordinates.Longitude,
                Temperature = data.CurrentWeather.Temperature,
                WindSpeed = data.CurrentWeather.WindSpeed,
                WindDirection = data.CurrentWeather.WindDirection,
                Sunrise = DateTime.Parse(sunriseRaw),
                FetchedAt = DateTime.UtcNow
            };
        }
    }
}
