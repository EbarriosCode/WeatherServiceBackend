using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using WeatherService.Application.Interfaces;
using WeatherService.Application.Services;
using WeatherService.Application.Settings;

namespace WeatherService.Application
{
    public static class DependencyContainer
    {
        public static IServiceCollection AddApplicationDependencies(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<CacheSettings>(configuration.GetSection(CacheSettings.SectionName));
            services.AddScoped<IWeatherService, WeatherServiceImp>();

            return services;
        }
    }
}
