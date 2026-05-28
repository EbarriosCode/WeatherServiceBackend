using FluentAssertions;
using System.Net;
using System.Text.Json;
using WeatherService.Application.DTOs;
using WeatherService.IntegrationTests.Controllers.Configurations;

namespace WeatherService.IntegrationTests.Controllers
{
    public class WeatherControllerTests : IClassFixture<IntegrationTestBase>
    {
        private readonly HttpClient _client;
        private readonly IntegrationTestBase _factory;

        // Guatemala City coordinates
        private const double Lat = 14.64;
        private const double Lon = -90.51;

        public WeatherControllerTests(IntegrationTestBase factory)
        {
            this._factory = factory;
            this._client = factory.Client;
        }

        public async Task InitializeAsync()
        {
            await this._factory.ResetDatabaseAsync();
        }

        public Task DisposeAsync() => Task.CompletedTask;

        [Fact]
        public async Task GetByCoordinates_ValidCoordinates_Returns200WithExpectedFields()
        {
            var response = await this._client.GetAsync($"/api/weather/by-coordinates?latitude={Lat}&longitude={Lon}");

            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var body = await DeserializeAsync(response);
            body.Should().NotBeNull();
            body!.Temperature.Should().NotBe(0);
            body.WindSpeed.Should().BeGreaterThanOrEqualTo(0);
            body.Sunrise.Should().NotBe(default);
        }

        [Fact]
        public async Task GetByCoordinates_FirstCall_ReturnsFreshData()
        {
            const double lat = 14.2700;
            const double lon = -90.7500;

            var response = await this._client.GetAsync($"/api/weather/by-coordinates?latitude={lat}&longitude={lon}");
            var body = await DeserializeAsync(response);

            body!.FromCache.Should().BeFalse();
        }

        [Fact]
        public async Task GetByCoordinates_SecondCallSameCoordinates_ReturnsCachedData()
        {
            // First call — populates cache
            await this._client.GetAsync($"/api/weather/by-coordinates?latitude={Lat}&longitude={Lon}");

            // Second call — should hit cache
            var response = await _client.GetAsync($"/api/weather/by-coordinates?latitude={Lat}&longitude={Lon}");
            var body = await DeserializeAsync(response);

            body!.FromCache.Should().BeTrue();
        }

        [Fact]
        public async Task GetByCoordinates_InvalidLatitude_Returns400()
        {
            var response = await this._client.GetAsync("/api/weather/by-coordinates?latitude=999&longitude=-90.51");

            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task GetByCoordinates_InvalidLongitude_Returns400()
        {
            var response = await this._client.GetAsync("/api/weather/by-coordinates?latitude=14.64&longitude=999");

            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task GetByCity_ValidCity_Returns200WithExpectedFields()
        {
            var response = await this._client.GetAsync("/api/weather/by-city?city=Guatemala%20City");

            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var body = await DeserializeAsync(response);
            body.Should().NotBeNull();
            body!.Temperature.Should().NotBe(0);
            body.Sunrise.Should().NotBe(default);
        }

        [Fact]
        public async Task GetByCity_UnknownCity_Returns404()
        {
            var response = await this._client.GetAsync("/api/weather/by-city?city=CiudadQueNoExisteXYZ123");

            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task GetByCity_EmptyCity_Returns400()
        {
            var response = await this._client.GetAsync("/api/weather/by-city?city=");

            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        private static async Task<WeatherDto?> DeserializeAsync(HttpResponseMessage response)
        {
            var content = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<WeatherDto>(content, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
        }
    }
}
