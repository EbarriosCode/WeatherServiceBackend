using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace WeatherService.Infrastructure.HealthChecks
{
    public class OpenMeteoHealthCheck : IHealthCheck
    {
        private readonly HttpClient _httpClient;

        public OpenMeteoHealthCheck(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            try
            {                
                var response = await this._httpClient.GetAsync("https://api.open-meteo.com/v1/forecast?latitude=14.64&longitude=-90.51&current_weather=true", cancellationToken);

                return response.IsSuccessStatusCode
                    ? HealthCheckResult.Healthy("Open-Meteo weather API is reachable.")
                    : HealthCheckResult.Degraded($"Open-Meteo weather API returned {(int)response.StatusCode}.");
            }
            catch (Exception ex)
            {
                return HealthCheckResult.Unhealthy("Open-Meteo weather API is unreachable.", ex);
            }
        }
    }
}
