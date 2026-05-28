using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using WeatherService.Application.DTOs;
using WeatherService.Application.Interfaces;
using WeatherService.Application.Settings;
using WeatherService.Domain.Entities;
using WeatherService.Domain.Interfaces;
using WeatherService.Domain.ValueObjects;

namespace WeatherService.Application.Services
{
    public class WeatherServiceImp : IWeatherService
    {
        private readonly ILogger<WeatherServiceImp> _logger;
        private readonly IWeatherRepository _mongoRepository;
        private readonly IWeatherExternalClient _weatherExternalClient;
        private readonly IGeoCodingExternalClient _geoCodingExternalClient;
        private readonly CacheSettings _cacheSettings;

        public WeatherServiceImp(ILogger<WeatherServiceImp> logger, IWeatherRepository mongoRepository, IWeatherExternalClient weatherExternalClient, IGeoCodingExternalClient geoCodingExternalClient, IOptions<CacheSettings> cacheSettings)
        {
            this._logger = logger;
            this._mongoRepository = mongoRepository;
            this._weatherExternalClient = weatherExternalClient;
            this._geoCodingExternalClient = geoCodingExternalClient;
            this._cacheSettings = cacheSettings.Value;
        }

        public async Task<WeatherDto> GetByCoordinatesAsync(double latitude, double longitude, CancellationToken cancellationToken = default)
        {
            var coordinates = Coordinates.Create(latitude, longitude);

            var cached = await this._mongoRepository.GetByCoordinatesAsync(coordinates, cancellationToken);

            if (cached is not null && !IsExpired(cached))
            {
                this._logger.LogInformation("Cache hit for coordinates {CacheKey}", coordinates.ToCacheKey());

                return MapToDto(cached, fromCache: true);
            }

            if (cached is not null)
                this._logger.LogInformation("Cache expired for coordinates {CacheKey}, refreshing from external API", coordinates.ToCacheKey());
            else
                this._logger.LogInformation("Cache miss for coordinates {CacheKey}, fetching from external API", coordinates.ToCacheKey());

            var fresh = await this._weatherExternalClient.FetchAsync(coordinates, cancellationToken);
            await this._mongoRepository.SaveAsync(fresh, cancellationToken);

            return MapToDto(fresh, fromCache: false);
        }

        public async Task<WeatherDto> GetByCityAsync(string cityName, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(cityName))
                throw new ArgumentException("City name cannot be empty.", nameof(cityName));

            var coordinates = await this._geoCodingExternalClient.GetCoordinatesByCityAsync(cityName, cancellationToken);

            // Reuse the same cache-aside logic after resolving coordinates
            return await GetByCoordinatesAsync(coordinates.Latitude, coordinates.Longitude, cancellationToken);
        }

        private bool IsExpired(WeatherRecord record)
        {
            var recordAge = DateTime.UtcNow - record.FetchedAt;
            var cacheTtl = TimeSpan.FromHours(this._cacheSettings.TtlHours);

            return recordAge > cacheTtl;
        }

        private static WeatherDto MapToDto(WeatherRecord record, bool fromCache) =>
            new(
                Temperature: record.Temperature,
                WindSpeed: record.WindSpeed,
                WindDirection: record.WindDirection,
                Sunrise: record.Sunrise,
                Latitude: record.Latitude,
                Longitude: record.Longitude,
                FetchedAt: record.FetchedAt,
                FromCache: fromCache
            );
    }

}
