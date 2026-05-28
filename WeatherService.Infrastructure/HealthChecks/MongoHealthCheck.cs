using Microsoft.Extensions.Diagnostics.HealthChecks;
using MongoDB.Driver;

namespace WeatherService.Infrastructure.HealthChecks
{
    public class MongoHealthCheck : IHealthCheck
    {
        private readonly IMongoClient _mongoClient;

        public MongoHealthCheck(IMongoClient mongoClient)
        {
            this._mongoClient = mongoClient;
        }

        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            try
            {
                await this._mongoClient.ListDatabaseNamesAsync(cancellationToken);

                return HealthCheckResult.Healthy("MongoDB is reachable.");
            }
            catch (Exception ex)
            {
                return HealthCheckResult.Unhealthy("MongoDB is unreachable.", ex);
            }
        }
    }
}
