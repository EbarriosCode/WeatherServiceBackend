using Microsoft.Extensions.Options;
using MongoDB.Driver;
using WeatherService.Domain.Entities;
using WeatherService.Domain.Interfaces;
using WeatherService.Domain.ValueObjects;
using WeatherService.Infrastructure.Configurations;
using WeatherService.Infrastructure.Persistence.Documents;
using WeatherService.Infrastructure.Persistence.Mappers;

namespace WeatherService.Infrastructure.Persistence.Repositories
{
    public class WeatherMongoRepositoryImp : IWeatherRepository
    {
        private readonly IMongoCollection<WeatherDocument> _collection;

        public WeatherMongoRepositoryImp(IMongoClient mongoClient, IOptions<MongoSettings> mongoSettings)
        {
            var settings = mongoSettings.Value;
            var database = mongoClient.GetDatabase(settings.DatabaseName);
            this._collection = database.GetCollection<WeatherDocument>(settings.CollectionName);

            EnsureIndexes();
        }

        public async Task<WeatherRecord?> GetByCoordinatesAsync(Coordinates coordinates, CancellationToken cancellationToken = default)
        {
            var cacheKey = coordinates.ToCacheKey();
            var filter = Builders<WeatherDocument>.Filter.Eq(d => d.CacheKey, cacheKey);
            var document = await this._collection.Find(filter).FirstOrDefaultAsync(cancellationToken);
            
            return document is null ? null : WeatherMapper.ToDomain(document);
        }

        public async Task SaveAsync(WeatherRecord record, CancellationToken cancellationToken = default)
        {
            var coordinates = Coordinates.Create(record.Latitude, record.Longitude);
            var cacheKey = coordinates.ToCacheKey();
            
            var existingDocument = await this._collection.Find(Builders<WeatherDocument>.Filter.Eq(d => d.CacheKey, cacheKey))
                                                         .FirstOrDefaultAsync(cancellationToken);

            var document = WeatherMapper.ToDocument(record, cacheKey);

            if (existingDocument is not null)
                document.Id = existingDocument.Id;

            var filter = Builders<WeatherDocument>.Filter.Eq(d => d.CacheKey, cacheKey);
            var options = new ReplaceOptions { IsUpsert = true };

            await this._collection.ReplaceOneAsync(filter, document, options, cancellationToken);
        }

        private void EnsureIndexes()
        {
            var indexKeys = Builders<WeatherDocument>.IndexKeys.Ascending(d => d.CacheKey);
            var indexOptions = new CreateIndexOptions { Unique = true };
            var indexModel = new CreateIndexModel<WeatherDocument>(indexKeys, indexOptions);

            this._collection.Indexes.CreateOne(indexModel);
        }
    }
}
