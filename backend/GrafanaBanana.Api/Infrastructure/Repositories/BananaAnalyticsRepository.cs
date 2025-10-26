using GrafanaBanana.Api.Databricks;
using GrafanaBanana.Api.Domain.Repositories;

namespace GrafanaBanana.Api.Infrastructure.Repositories;

/// <summary>
/// Repository implementation for banana analytics data
/// Delegates to the existing DatabricksService while providing a repository abstraction
/// This follows the Adapter pattern to integrate legacy code with new architecture
/// </summary>
public class BananaAnalyticsRepository : IBananaAnalyticsRepository
{
    private readonly IDatabricksService _databricksService;
    private readonly ILogger<BananaAnalyticsRepository> _logger;

    public BananaAnalyticsRepository(
        IDatabricksService databricksService,
        ILogger<BananaAnalyticsRepository> logger)
    {
        _databricksService = databricksService;
        _logger = logger;
    }

    public async Task<BananaAnalytics> GetBananaAnalyticsAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Fetching banana analytics from Databricks service");
        return await _databricksService.GetBananaAnalyticsAsync(cancellationToken);
    }

    public async Task<List<BananaProduction>> GetProductionDataAsync(int year, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Fetching banana production data for year {Year}", year);
        return await _databricksService.GetProductionDataAsync(year, cancellationToken);
    }

    public async Task<List<BananaSalesData>> GetSalesDataAsync(string region, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Fetching banana sales data for region {Region}", region);
        return await _databricksService.GetSalesDataAsync(region, cancellationToken);
    }
}
