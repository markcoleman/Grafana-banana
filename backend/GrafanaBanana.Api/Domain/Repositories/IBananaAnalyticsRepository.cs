using GrafanaBanana.Api.Databricks;

namespace GrafanaBanana.Api.Domain.Repositories;

/// <summary>
/// Repository interface for banana analytics data access
/// Abstracts the Databricks service implementation
/// </summary>
public interface IBananaAnalyticsRepository
{
    Task<BananaAnalytics> GetBananaAnalyticsAsync(CancellationToken cancellationToken = default);
    Task<List<BananaProduction>> GetProductionDataAsync(int year, CancellationToken cancellationToken = default);
    Task<List<BananaSalesData>> GetSalesDataAsync(string region, CancellationToken cancellationToken = default);
}
