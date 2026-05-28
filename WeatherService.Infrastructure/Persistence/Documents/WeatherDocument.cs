using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace WeatherService.Infrastructure.Persistence.Documents
{
    public class WeatherDocument
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = default!;

        [BsonElement("cacheKey")]
        public string CacheKey { get; set; } = default!;

        [BsonElement("latitude")]
        public double Latitude { get; set; }

        [BsonElement("longitude")]
        public double Longitude { get; set; }

        [BsonElement("temperature")]
        public double Temperature { get; set; }

        [BsonElement("windSpeed")]
        public double WindSpeed { get; set; }

        [BsonElement("windDirection")]
        public double WindDirection { get; set; }

        [BsonElement("sunrise")]
        public DateTime Sunrise { get; set; }

        [BsonElement("fetchedAt")]
        public DateTime FetchedAt { get; set; }
    }
}
