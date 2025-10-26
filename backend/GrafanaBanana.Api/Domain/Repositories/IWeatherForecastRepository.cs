using GrafanaBanana.Api.Domain.Entities;

namespace GrafanaBanana.Api.Domain.Repositories;

/// <summary>
/// Repository interface for weather forecast data access
/// Following Repository Pattern for abstraction of data access logic
/// </summary>
public interface IWeatherForecastRepository
{
    Task<IEnumerable<WeatherForecast>> GetForecastsAsync(int days, CancellationToken cancellationToken = default);
}
