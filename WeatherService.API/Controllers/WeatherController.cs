using Microsoft.AspNetCore.Mvc;
using WeatherService.Application.DTOs;
using WeatherService.Application.Interfaces;

namespace WeatherService.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class WeatherController : ControllerBase
    {
        private readonly IWeatherService _weatherService;

        public WeatherController(IWeatherService weatherService)
        {
            this._weatherService = weatherService;
        }

        /// <summary>
        /// Returns weather data for the given coordinates.
        /// Results are cached in MongoDB. The first call fetches from Open-Meteo;
        /// subsequent calls with the same coordinates return the cached result until TTL expires.
        /// </summary>
        /// <param name="latitude">Latitude (-90 to 90)</param>
        /// <param name="longitude">Longitude (-180 to 180)</param>
        /// <param name="cancellationToken"></param>
        [HttpGet("by-coordinates")]
        [ProducesResponseType(typeof(WeatherDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status502BadGateway)]
        public async Task<ActionResult<WeatherDto>> GetByCoordinates(
            [FromQuery] double latitude,
            [FromQuery] double longitude,
            CancellationToken cancellationToken)
        {
            var result = await this._weatherService.GetByCoordinatesAsync(latitude, longitude, cancellationToken);

            return Ok(result);
        }

        /// <summary>
        /// Returns weather data for the given city name.
        /// Internally resolves the city to coordinates using Open-Meteo Geocoding API,
        /// then applies the same cache-aside logic as the coordinates endpoint.
        /// </summary>
        /// <param name="city">City name (e.g. "Guatemala City")</param>
        /// <param name="cancellationToken"></param>
        [HttpGet("by-city")]
        [ProducesResponseType(typeof(WeatherDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status502BadGateway)]
        public async Task<ActionResult<WeatherDto>> GetByCity(
            [FromQuery] string city,
            CancellationToken cancellationToken)
        {
            var result = await this._weatherService.GetByCityAsync(city, cancellationToken);

            return Ok(result);
        }
    }
}
