namespace WeatherService.Domain.ValueObjects
{
    public sealed class Coordinates
    {
        public double Latitude { get; }
        public double Longitude { get; }

        public double RoundedLatitude => Math.Round(Latitude, 2);
        public double RoundedLongitude => Math.Round(Longitude, 2);

        private Coordinates(double latitude, double longitude)
        {
            Latitude = latitude;
            Longitude = longitude;
        }

        public static Coordinates Create(double latitude, double longitude)
        {
            if (latitude < -90 || latitude > 90)
                throw new ArgumentOutOfRangeException(nameof(latitude), "Latitude must be between -90 and 90.");

            if (longitude < -180 || longitude > 180)
                throw new ArgumentOutOfRangeException(nameof(longitude), "Longitude must be between -180 and 180.");

            return new Coordinates(latitude, longitude);
        }

        // Used as MongoDB cache key
        public string ToCacheKey() => $"{RoundedLatitude}_{RoundedLongitude}";
        public override string ToString() => $"({Latitude}, {Longitude})";
    }
}
