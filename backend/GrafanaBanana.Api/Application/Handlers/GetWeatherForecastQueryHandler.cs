using GrafanaBanana.Api.Application.Queries;
using GrafanaBanana.Api.Domain.Entities;
using GrafanaBanana.Api.Domain.Repositories;
using MediatR;

namespace GrafanaBanana.Api.Application.Handlers;

/// <summary>
/// Handler for GetWeatherForecastQuery
/// Implements the business logic for retrieving weather forecasts
/// Following CQRS and Mediator patterns
/// </summary>
public class GetWeatherForecastQueryHandler : IRequestHandler<GetWeatherForecastQuery, IEnumerable<WeatherForecast>>
{
    private readonly IWeatherForecastRepository _repository;
    private readonly ILogger<GetWeatherForecastQueryHandler> _logger;

    public GetWeatherForecastQueryHandler(
        IWeatherForecastRepository repository,
        ILogger<GetWeatherForecastQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<IEnumerable<WeatherForecast>> Handle(GetWeatherForecastQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Handling GetWeatherForecastQuery for {Days} days", request.Days);

        var forecasts = await _repository.GetForecastsAsync(request.Days, cancellationToken);

        _logger.LogInformation("Successfully retrieved {Count} weather forecast entries", forecasts.Count());

        return forecasts;
    }
}
