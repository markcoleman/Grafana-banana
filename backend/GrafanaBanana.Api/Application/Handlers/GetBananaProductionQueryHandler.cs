using GrafanaBanana.Api.Application.Queries;
using GrafanaBanana.Api.Databricks;
using GrafanaBanana.Api.Domain.Repositories;
using MediatR;

namespace GrafanaBanana.Api.Application.Handlers;

/// <summary>
/// Handler for GetBananaProductionQuery
/// Implements the business logic for retrieving banana production data
/// Following CQRS and Mediator patterns
/// </summary>
public class GetBananaProductionQueryHandler : IRequestHandler<GetBananaProductionQuery, List<BananaProduction>>
{
    private readonly IBananaAnalyticsRepository _repository;
    private readonly ILogger<GetBananaProductionQueryHandler> _logger;

    public GetBananaProductionQueryHandler(
        IBananaAnalyticsRepository repository,
        ILogger<GetBananaProductionQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<List<BananaProduction>> Handle(GetBananaProductionQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Handling GetBananaProductionQuery for year {Year}", request.Year);

        var production = await _repository.GetProductionDataAsync(request.Year, cancellationToken);

        _logger.LogInformation("Retrieved {Count} production records for year {Year}", production.Count, request.Year);

        return production;
    }
}
