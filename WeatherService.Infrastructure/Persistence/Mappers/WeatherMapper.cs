using MongoDB.Bson;
using WeatherService.Domain.Entities;
using WeatherService.Infrastructure.Persistence.Documents;

namespace WeatherService.Infrastructure.Persistence.Mappers
{
    internal static class WeatherMapper
    {
        public static WeatherRecord ToDomain(WeatherDocument document) =>
            new()
            {
                Id = document.Id,
                Latitude = document.Latitude,
                Longitude = document.Longitude,
                Temperature = document.Temperature,
                WindSpeed = document.WindSpeed,
                WindDirection = document.WindDirection,
                Sunrise = document.Sunrise,
                FetchedAt = document.FetchedAt
            };

        public static WeatherDocument ToDocument(WeatherRecord record, string cacheKey) =>
            new()
            {
                Id = string.IsNullOrEmpty(record.Id) ? ObjectId.GenerateNewId().ToString() : record.Id,
                CacheKey = cacheKey,
                Latitude = record.Latitude,
                Longitude = record.Longitude,
                Temperature = record.Temperature,
                WindSpeed = record.WindSpeed,
                WindDirection = record.WindDirection,
                Sunrise = record.Sunrise,
                FetchedAt = record.FetchedAt
            };
    }
}
