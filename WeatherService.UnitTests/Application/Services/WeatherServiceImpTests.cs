using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using WeatherService.Application.Interfaces;
using WeatherService.Application.Services;
using WeatherService.Application.Settings;
using WeatherService.Domain.Entities;
using WeatherService.Domain.Interfaces;
using WeatherService.Domain.ValueObjects;

namespace WeatherService.UnitTests.Application.Services
{
    public class WeatherServiceImpTests
    {
        private readonly Mock<ILogger<WeatherServiceImp>> _loggerMock;
        private readonly Mock<IWeatherRepository> _mongoRepositoryMock;
        private readonly Mock<IWeatherExternalClient> _externalClientMock;
        private readonly Mock<IGeoCodingExternalClient> _geoCodingExternalClientMock;
        private readonly WeatherServiceImp _sut;

        private const double Lat = 14.64;
        private const double Lon = -90.51;

        public WeatherServiceImpTests()
        {
            this._loggerMock = new Mock<ILogger<WeatherServiceImp>>();
            this._mongoRepositoryMock = new Mock<IWeatherRepository>();
            this._externalClientMock = new Mock<IWeatherExternalClient>();
            this._geoCodingExternalClientMock = new Mock<IGeoCodingExternalClient>();

            var cacheSettings = Options.Create(new CacheSettings { TtlHours = 3 });

            this._sut = new WeatherServiceImp(this._loggerMock.Object,
                                              this._mongoRepositoryMock.Object,
                                              this._externalClientMock.Object,
                                              this._geoCodingExternalClientMock.Object,
                                              cacheSettings);
        }

        [Fact]
        public async Task GetByCoordinatesAsync_WhenCacheHit_ReturnsCachedRecord_WithoutCallingExternal()
        {
            var cached = BuildRecord(fetchedAt: DateTime.UtcNow.AddHours(-1)); // fresh, within TTL
            this._mongoRepositoryMock
                .Setup(r => r.GetByCoordinatesAsync(It.IsAny<Coordinates>(), default))
                .ReturnsAsync(cached);

            var result = await this._sut.GetByCoordinatesAsync(Lat, Lon);

            Assert.True(result.FromCache);
            Assert.Equal(cached.Temperature, result.Temperature);
            this._externalClientMock.Verify(c => c.FetchAsync(It.IsAny<Coordinates>(), default), Times.Never);
            this._mongoRepositoryMock.Verify(r => r.SaveAsync(It.IsAny<WeatherRecord>(), default), Times.Never);
        }

        [Fact]
        public async Task GetByCoordinatesAsync_WhenCacheMiss_CallsExternalAndSaves()
        {
            this._mongoRepositoryMock
                .Setup(r => r.GetByCoordinatesAsync(It.IsAny<Coordinates>(), default))
                .ReturnsAsync((WeatherRecord?)null);

            var fresh = BuildRecord(fetchedAt: DateTime.UtcNow);

            this._externalClientMock
                .Setup(c => c.FetchAsync(It.IsAny<Coordinates>(), default))
                .ReturnsAsync(fresh);

            var result = await this._sut.GetByCoordinatesAsync(Lat, Lon);

            Assert.False(result.FromCache);
            Assert.Equal(fresh.Temperature, result.Temperature);
            this._externalClientMock.Verify(c => c.FetchAsync(It.IsAny<Coordinates>(), default), Times.Once);
            this._mongoRepositoryMock.Verify(r => r.SaveAsync(fresh, default), Times.Once);
        }

        [Fact]
        public async Task GetByCoordinatesAsync_WhenCacheExpired_RefreshesFromExternal()
        {
            // Cached record is 5 hours old, TTL is 3h → expired
            var expired = BuildRecord(fetchedAt: DateTime.UtcNow.AddHours(-5));
            this._mongoRepositoryMock
                .Setup(r => r.GetByCoordinatesAsync(It.IsAny<Coordinates>(), default))
                .ReturnsAsync(expired);

            var fresh = BuildRecord(fetchedAt: DateTime.UtcNow);
            this._externalClientMock
                .Setup(c => c.FetchAsync(It.IsAny<Coordinates>(), default))
                .ReturnsAsync(fresh);

            var result = await _sut.GetByCoordinatesAsync(Lat, Lon);

            Assert.False(result.FromCache);
            this._externalClientMock.Verify(c => c.FetchAsync(It.IsAny<Coordinates>(), default), Times.Once);
        }

        [Fact]
        public async Task GetByCoordinatesAsync_WithInvalidLatitude_ThrowsArgumentOutOfRangeException()
        {
            await Assert.ThrowsAsync<ArgumentOutOfRangeException>(
                () => this._sut.GetByCoordinatesAsync(latitude: 999, longitude: Lon));
        }

        [Fact]
        public async Task GetByCityAsync_ResolvesCoordinates_ThenAppliesCacheAsideLogic()
        {
            var resolvedCoords = Coordinates.Create(Lat, Lon);
            this._geoCodingExternalClientMock
                .Setup(g => g.GetCoordinatesByCityAsync("Guatemala City", default))
                .ReturnsAsync(resolvedCoords);

            this._mongoRepositoryMock
                .Setup(r => r.GetByCoordinatesAsync(It.IsAny<Coordinates>(), default))
                .ReturnsAsync((WeatherRecord?)null);

            var fresh = BuildRecord(fetchedAt: DateTime.UtcNow);
            this._externalClientMock
                .Setup(c => c.FetchAsync(It.IsAny<Coordinates>(), default))
                .ReturnsAsync(fresh);

            var result = await this._sut.GetByCityAsync("Guatemala City");

            Assert.False(result.FromCache);
            this._geoCodingExternalClientMock.Verify(g => g.GetCoordinatesByCityAsync("Guatemala City", default), Times.Once);
            this._externalClientMock.Verify(c => c.FetchAsync(It.IsAny<Coordinates>(), default), Times.Once);
        }

        [Fact]
        public async Task GetByCityAsync_WithEmptyCityName_ThrowsArgumentException()
        {
            await Assert.ThrowsAsync<ArgumentException>(
                () => this._sut.GetByCityAsync("   "));
        }

        private static WeatherRecord BuildRecord(DateTime fetchedAt) =>
        new()
        {
            Id = "test-id",
            Latitude = Lat,
            Longitude = Lon,
            Temperature = 25.0,
            WindSpeed = 10.0,
            WindDirection = 180.0,
            Sunrise = DateTime.UtcNow.Date.AddHours(6),
            FetchedAt = fetchedAt
        };
    }
}
