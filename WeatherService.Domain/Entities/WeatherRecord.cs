namespace WeatherService.Domain.Entities
{
    public class WeatherRecord
    {
        public string Id { get; set; } = default!;
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public double Temperature { get; set; }
        public double WindSpeed { get; set; }
        public double WindDirection { get; set; }
        public DateTime Sunrise { get; set; }
        public DateTime FetchedAt { get; init; }
    }
}
