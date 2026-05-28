using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using WeatherService.API.Middleware;
using WeatherService.Application;
using WeatherService.Infrastructure;
using WeatherService.Infrastructure.Configurations;
using WeatherService.Infrastructure.HealthChecks;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new()
    {
        Title = "Weather API",
        Version = "v1",
        Description = "REST API for weather data with MongoDB caching. " +
                      "First call fetches from Open-Meteo; subsequent calls with the same parameters return cached data."
    });

    // Enable XML comments for controller summaries
    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
        options.IncludeXmlComments(xmlPath);
});

builder.Services.AddApplicationDependencies(builder.Configuration);
builder.Services.AddInfrastructureDependencies(builder.Configuration);


builder.Services.AddHealthChecks()
    .AddCheck<MongoHealthCheck>(
        name: "mongodb",
        failureStatus: HealthStatus.Unhealthy,
        tags: new[] { "database" })
    // Open-Meteo Weather API
    .AddTypeActivatedCheck<OpenMeteoHealthCheck>(
        name: "open-meteo-weather",
        failureStatus: HealthStatus.Degraded,
        tags: new[] { "external-api" })

    // Open-Meteo GeoCoding API
    .AddTypeActivatedCheck<OpenMeteoGeoCodingHealthCheck>(
        name: "open-meteo-geocoding",
        failureStatus: HealthStatus.Degraded,
        tags: new[] { "external-api" });

var app = builder.Build();

app.UseMiddleware<ExceptionHandlingMiddleware>();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapHealthChecks("/health", new HealthCheckOptions
{
    ResponseWriter = async (context, report) =>
    {
        context.Response.ContentType = "application/json";

        var result = new
        {
            status = report.Status.ToString(),
            duration = report.TotalDuration,
            checks = report.Entries.Select(e => new
            {
                name = e.Key,
                status = e.Value.Status.ToString(),
                description = e.Value.Description,
                duration = e.Value.Duration,
                tags = e.Value.Tags
            })
        };

        await context.Response.WriteAsJsonAsync(result);
    }
});

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();

public partial class Program { }