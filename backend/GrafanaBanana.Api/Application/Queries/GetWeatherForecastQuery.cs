using GrafanaBanana.Api.Domain.Entities;
using MediatR;

namespace GrafanaBanana.Api.Application.Queries;

/// <summary>
/// Query to retrieve weather forecasts.
/// Implements the CQRS (Command Query Responsibility Segregation) pattern for read operations.
/// </summary>
/// <param name="Days">The number of days to forecast. Defaults to 5 days.</param>
/// <remarks>
/// This is a query (read operation) in the CQRS pattern. Key characteristics:
/// - Immutable (record type)
/// - Returns data without side effects
/// - Processed by a dedicated handler
/// - Decoupled from the delivery mechanism (API, CLI, etc.)
/// 
/// Benefits of CQRS:
/// - Clear separation between reads and writes
/// - Easier to optimize queries independently
/// - Better testability
/// - Supports different models for reads and writes
/// </remarks>
/// <example>
/// Usage in an API endpoint:
/// <code>
/// app.MapGet("/weatherforecast", async (IMediator mediator) =>
/// {
///     var query = new GetWeatherForecastQuery(Days: 5);
///     return await mediator.Send(query);
/// });
/// </code>
/// </example>
public record GetWeatherForecastQuery(int Days = 5) : IRequest<IEnumerable<WeatherForecast>>;
