using GrafanaBanana.Api.Application.Queries;
using GrafanaBanana.Api.Databricks;
using GrafanaBanana.Api.Domain.Repositories;
using MediatR;

namespace GrafanaBanana.Api.Application.Handlers;

/// <summary>
/// Handler for GetBananaAnalyticsQuery
/// Implements the business logic for retrieving banana analytics
/// Following CQRS and Mediator patterns
/// </summary>
public class GetBananaAnalyticsQueryHandler : IRequestHandler<GetBananaAnalyticsQuery, BananaAnalytics>
{
    private readonly IBananaAnalyticsRepository _repository;
    private readonly ILogger<GetBananaAnalyticsQueryHandler> _logger;

    public GetBananaAnalyticsQueryHandler(
        IBananaAnalyticsRepository repository,
        ILogger<GetBananaAnalyticsQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<BananaAnalytics> Handle(GetBananaAnalyticsQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Handling GetBananaAnalyticsQuery");

        var analytics = await _repository.GetBananaAnalyticsAsync(cancellationToken);

        _logger.LogInformation(
            "Retrieved banana analytics: {ProductionTons} tons produced, {Revenue} revenue, {Countries} countries served",
            analytics.Summary.TotalProductionTons,
            analytics.Summary.TotalRevenue,
            analytics.Summary.CountriesServed);

        return analytics;
    }
}
