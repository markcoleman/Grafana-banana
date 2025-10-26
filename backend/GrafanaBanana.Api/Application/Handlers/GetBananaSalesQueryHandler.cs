using GrafanaBanana.Api.Application.Queries;
using GrafanaBanana.Api.Databricks;
using GrafanaBanana.Api.Domain.Repositories;
using MediatR;

namespace GrafanaBanana.Api.Application.Handlers;

/// <summary>
/// Handler for GetBananaSalesQuery
/// Implements the business logic for retrieving banana sales data
/// Following CQRS and Mediator patterns
/// </summary>
public class GetBananaSalesQueryHandler : IRequestHandler<GetBananaSalesQuery, List<BananaSalesData>>
{
    private readonly IBananaAnalyticsRepository _repository;
    private readonly ILogger<GetBananaSalesQueryHandler> _logger;

    public GetBananaSalesQueryHandler(
        IBananaAnalyticsRepository repository,
        ILogger<GetBananaSalesQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<List<BananaSalesData>> Handle(GetBananaSalesQuery request, CancellationToken cancellationToken)
    {
        // Sanitize region parameter for logging to prevent log forging
        var region = request.Region ?? "Global";
        var sanitizedRegion = region.Replace("\n", "").Replace("\r", "");
        _logger.LogInformation("Handling GetBananaSalesQuery for region {Region}", sanitizedRegion);

        var sales = await _repository.GetSalesDataAsync(region, cancellationToken);

        _logger.LogInformation("Retrieved {Count} sales records", sales.Count);

        return sales;
    }
}
