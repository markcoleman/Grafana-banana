using GrafanaBanana.Api.Domain.Entities;
using MediatR;

namespace GrafanaBanana.Api.Application.Queries;

/// <summary>
/// Query to get weather forecasts
/// Following CQRS pattern - read operation
/// </summary>
public record GetWeatherForecastQuery(int Days = 5) : IRequest<IEnumerable<WeatherForecast>>;
