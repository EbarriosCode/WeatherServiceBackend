using Microsoft.Extensions.Diagnostics.HealthChecks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WeatherService.Infrastructure.HealthChecks
{
    public class OpenMeteoGeoCodingHealthCheck : IHealthCheck
    {
        private readonly HttpClient _httpClient;

        public OpenMeteoGeoCodingHealthCheck(HttpClient httpClient)
        {
            this._httpClient = httpClient;
        }

        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            try
            {
                var response = await this._httpClient.GetAsync("https://geocoding-api.open-meteo.com/v1/search?name=Guatemala&count=1", cancellationToken);

                return response.IsSuccessStatusCode
                    ? HealthCheckResult.Healthy("Open-Meteo geocoding API is reachable.")
                    : HealthCheckResult.Degraded($"Open-Meteo geocoding API returned {(int)response.StatusCode}.");
            }
            catch (Exception ex)
            {
                return HealthCheckResult.Unhealthy("Open-Meteo geocoding API is unreachable.", ex);
            }
        }
    }
}
