using GrafanaBanana.Api.Application.Queries;
using GrafanaBanana.Api.Domain.Entities;
using GrafanaBanana.Api.Domain.Repositories;
using MediatR;

namespace GrafanaBanana.Api.Application.Handlers;

/// <summary>
/// Handler for GetWeatherForecastQuery.
/// Implements the business logic for retrieving weather forecasts using CQRS and Mediator patterns.
/// </summary>
/// <remarks>
/// This handler demonstrates several enterprise architecture patterns:
/// 
/// 1. **Mediator Pattern**: MediatR decouples the request sender from the handler
/// 2. **CQRS Pattern**: Separates read operations from write operations
/// 3. **Dependency Injection**: Constructor injection of dependencies
/// 4. **Repository Pattern**: Uses IWeatherForecastRepository abstraction
/// 5. **Single Responsibility**: Handles only one type of query
/// 
/// The handler coordinates between:
/// - The query (what to do)
/// - The repository (where to get data)
/// - The logging (observability)
/// 
/// This separation enables:
/// - Easy unit testing (mock the repository)
/// - Flexibility (swap repository implementations)
/// - Maintainability (changes in one area don't affect others)
/// </remarks>
/// <example>
/// This handler is automatically invoked by MediatR:
/// <code>
/// // In an API endpoint
/// var forecast = await mediator.Send(new GetWeatherForecastQuery(Days: 5));
/// 
/// // MediatR automatically routes to this handler
/// // You never directly instantiate or call handlers
/// </code>
/// 
/// For unit testing:
/// <code>
/// var mockRepo = new Mock&lt;IWeatherForecastRepository&gt;();
/// mockRepo.Setup(r => r.GetForecastsAsync(5, default))
///     .ReturnsAsync(new[] { WeatherForecast.Create(...) });
/// 
/// var handler = new GetWeatherForecastQueryHandler(mockRepo.Object, mockLogger);
/// var result = await handler.Handle(new GetWeatherForecastQuery(5), default);
/// </code>
/// </example>
public class GetWeatherForecastQueryHandler : IRequestHandler<GetWeatherForecastQuery, IEnumerable<WeatherForecast>>
{
    private readonly IWeatherForecastRepository _repository;
    private readonly ILogger<GetWeatherForecastQueryHandler> _logger;

    /// <summary>
    /// Initializes a new instance of the GetWeatherForecastQueryHandler.
    /// Dependencies are injected by the IoC container.
    /// </summary>
    /// <param name="repository">Repository for accessing weather forecast data.</param>
    /// <param name="logger">Logger for structured logging and observability.</param>
    public GetWeatherForecastQueryHandler(
        IWeatherForecastRepository repository,
        ILogger<GetWeatherForecastQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <summary>
    /// Handles the GetWeatherForecastQuery by retrieving forecasts from the repository.
    /// </summary>
    /// <param name="request">The query containing the number of days to forecast.</param>
    /// <param name="cancellationToken">Cancellation token for async operations.</param>
    /// <returns>A collection of weather forecast entities.</returns>
    /// <remarks>
    /// This method is called by MediatR when a GetWeatherForecastQuery is sent.
    /// It demonstrates proper error handling, logging, and async/await patterns.
    /// </remarks>
    public async Task<IEnumerable<WeatherForecast>> Handle(GetWeatherForecastQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Handling GetWeatherForecastQuery for {Days} days", request.Days);

        var forecasts = await _repository.GetForecastsAsync(request.Days, cancellationToken);

        _logger.LogInformation("Successfully retrieved {Count} weather forecast entries", forecasts.Count());

        return forecasts;
    }
}
