using WeatherService.Domain.ValueObjects;

namespace WeatherService.UnitTests.Domain.ValueObjects
{
    public class CoordinatesTests
    {
        [Theory]
        [InlineData(0, 0)]
        [InlineData(-90, -180)] // Absolute minimum boundary
        [InlineData(90, 180)]   // Absolute maximum boundary
        [InlineData(40.7128, -74.0060)] // Nueva York
        public void Create_WithValidCoordinates_ShouldReturnCoordinatesInstance(double latitude, double longitude)
        {         
            var coordinates = Coordinates.Create(latitude, longitude);
            
            Assert.NotNull(coordinates);
            Assert.Equal(latitude, coordinates.Latitude);
            Assert.Equal(longitude, coordinates.Longitude);
        }

        [Theory]
        [InlineData(-90.01)]
        [InlineData(90.01)]
        [InlineData(-150)]
        [InlineData(120)]
        public void Create_WithInvalidLatitude_ShouldThrowArgumentOutOfRangeException(double invalidLatitude)
        {
            double validLongitude = 0;

            var exception = Assert.Throws<ArgumentOutOfRangeException>(() =>
                Coordinates.Create(invalidLatitude, validLongitude));

            Assert.Equal("latitude", exception.ParamName);
            Assert.Contains("Latitude must be between -90 and 90.", exception.Message);
        }

        [Theory]
        [InlineData(-180.01)]
        [InlineData(180.01)]
        [InlineData(-250)]
        [InlineData(300)]
        public void Create_WithInvalidLongitude_ShouldThrowArgumentOutOfRangeException(double invalidLongitude)
        {
            double validLatitude = 0;

            var exception = Assert.Throws<ArgumentOutOfRangeException>(() =>
                Coordinates.Create(validLatitude, invalidLongitude));

            Assert.Equal("longitude", exception.ParamName);
            Assert.Contains("Longitude must be between -180 and 180.", exception.Message);
        }

        [Theory]
        [InlineData(40.71284, 40.71)]  // Rounding down
        [InlineData(40.71684, 40.72)]  // Rounding up
        public void RoundedProperties_WhenAccessed_ShouldRoundToTwoDecimals(double rawValue, double expectedValue)
        {
            var coordinates = Coordinates.Create(rawValue, rawValue);

            Assert.Equal(expectedValue, coordinates.RoundedLatitude);
            Assert.Equal(expectedValue, coordinates.RoundedLongitude);
        }

        [Fact]
        public void ToCacheKey_WhenCalled_ShouldReturnFormattedStringWithRoundedValues()
        {
            var coordinates = Coordinates.Create(40.7168, -74.0060);
            var expectedKey = "40.72_-74.01";

            var result = coordinates.ToCacheKey();

            Assert.Equal(expectedKey, result);
        }

        [Fact]
        public void ToString_WhenCalled_ShouldReturnFormattedStringWithRawValues()
        {
            var coordinates = Coordinates.Create(40.7128, -74.0060);
            var expectedString = "(40.7128, -74.006)";

            var result = coordinates.ToString();

            Assert.Equal(expectedString, result);
        }
    }
}
