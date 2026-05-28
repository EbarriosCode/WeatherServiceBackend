namespace WeatherService.Application.Settings
{
    public class CacheSettings
    {
        public const string SectionName = "Cache";

        /// <summary>Hours before a cached weather record is considered stale and refreshed from the external API.</summary>
        public int TtlHours { get; init; } = default!;
    }
}
