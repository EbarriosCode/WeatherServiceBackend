using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using WeatherService.Application.Interfaces;
using WeatherService.Domain.Interfaces;
using WeatherService.Infrastructure.Configurations;
using WeatherService.Infrastructure.ExternalClients.OpenMeteo;
using WeatherService.Infrastructure.Persistence.Repositories;

namespace WeatherService.Infrastructure
{
    public static class DependencyContainer
    {
        public static IServiceCollection AddInfrastructureDependencies(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<MongoSettings>(configuration.GetSection(MongoSettings.SectionName));
            services.Configure<OpenMeteoSettings>(configuration.GetSection(OpenMeteoSettings.SectionName));

            // MongoDB
            services.AddSingleton<IMongoClient>(sp =>
            {
                var settings = configuration.GetSection(MongoSettings.SectionName).Get<MongoSettings>()!;

                return new MongoClient(settings.ConnectionString);
            });

            services.AddScoped<IWeatherRepository, WeatherMongoRepositoryImp>();

            // HTTP Clients — named and typed for each external service
            services.AddHttpClient<IWeatherExternalClient, OpenMeteoWeatherClientImp>((serviceProvider, client) =>
            {                
                var settings = serviceProvider.GetRequiredService<IOptions<OpenMeteoSettings>>().Value;
                client.BaseAddress = new Uri(settings.BaseUrl);
            });
            
            services.AddHttpClient<IGeoCodingExternalClient, OpenMeteoGeoCodingClientImp>((serviceProvider, client) =>
            {
                var settings = serviceProvider.GetRequiredService<IOptions<OpenMeteoSettings>>().Value;
                client.BaseAddress = new Uri(settings.GeoCodingBaseUrl);
            });

            return services;
        }
    }
}
