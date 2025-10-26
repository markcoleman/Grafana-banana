using GrafanaBanana.Api.Domain.Entities;
using GrafanaBanana.Api.Domain.Repositories;

namespace GrafanaBanana.Api.Infrastructure.Repositories;

/// <summary>
/// Repository implementation for weather forecast data
/// Implements in-memory data generation for demonstration
/// In a real application, this would connect to a database or external API
/// </summary>
public class WeatherForecastRepository : IWeatherForecastRepository
{
    private readonly ILogger<WeatherForecastRepository> _logger;
    private static readonly string[] Summaries = new[]
    {
        "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
    };

    public WeatherForecastRepository(ILogger<WeatherForecastRepository> logger)
    {
        _logger = logger;
    }

    public Task<IEnumerable<WeatherForecast>> GetForecastsAsync(int days, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Generating {Days} weather forecasts", days);

        var forecasts = Enumerable.Range(1, days).Select(index =>
            WeatherForecast.Create(
                DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                Random.Shared.Next(-20, 55),
                Summaries[Random.Shared.Next(Summaries.Length)]
            ))
            .ToList();

        return Task.FromResult<IEnumerable<WeatherForecast>>(forecasts);
    }
}
