using GrafanaBanana.Api.Domain.Entities;

namespace GrafanaBanana.Api.Domain.Repositories;

/// <summary>
/// Repository interface for weather forecast data access.
/// Implements the Repository Pattern to abstract data access logic from business logic.
/// </summary>
/// <remarks>
/// This interface is part of the Domain Layer and defines the contract for data access.
/// The actual implementation resides in the Infrastructure Layer, following the
/// Dependency Inversion Principle - high-level modules (handlers) depend on this
/// abstraction, not on concrete implementations.
/// 
/// Benefits:
/// - Testability: Easy to mock for unit testing
/// - Flexibility: Can swap implementations (in-memory, database, API) without changing business logic
/// - Separation of Concerns: Domain layer doesn't know about data access details
/// </remarks>
/// <example>
/// Usage in a handler:
/// <code>
/// public class GetWeatherForecastQueryHandler
/// {
///     private readonly IWeatherForecastRepository _repository;
///     
///     public async Task Handle(GetWeatherForecastQuery query)
///     {
///         return await _repository.GetForecastsAsync(query.Days);
///     }
/// }
/// </code>
/// </example>
public interface IWeatherForecastRepository
{
    /// <summary>
    /// Retrieves weather forecasts for the specified number of days.
    /// </summary>
    /// <param name="days">The number of days to forecast.</param>
    /// <param name="cancellationToken">Cancellation token for async operations.</param>
    /// <returns>A collection of weather forecast entities.</returns>
    /// <remarks>
    /// This method abstracts the data source. The implementation could:
    /// - Generate random data (current implementation)
    /// - Query a database
    /// - Call an external weather API
    /// - Use cached data
    /// </remarks>
    Task<IEnumerable<WeatherForecast>> GetForecastsAsync(int days, CancellationToken cancellationToken = default);
}
