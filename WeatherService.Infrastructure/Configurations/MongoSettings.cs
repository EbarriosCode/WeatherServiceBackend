namespace WeatherService.Infrastructure.Configurations
{
    public class MongoSettings
    {
        public const string SectionName = "MongoSettings";

        public string ConnectionString { get; init; } = default!;
        public string DatabaseName { get; init; } = default!;
        public string CollectionName { get; init; } = "WeatherRecords";
    }
}
