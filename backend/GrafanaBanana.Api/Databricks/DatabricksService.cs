using System.Diagnostics;
using Microsoft.Extensions.Options;

namespace GrafanaBanana.Api.Databricks;

/// <summary>
/// Service for interacting with Databricks to retrieve banana analytics
/// In mock mode, generates realistic-looking data for demonstration
/// </summary>
public interface IDatabricksService
{
    Task<BananaAnalytics> GetBananaAnalyticsAsync(CancellationToken cancellationToken = default);
    Task<List<BananaProduction>> GetProductionDataAsync(int year, CancellationToken cancellationToken = default);
    Task<List<BananaSalesData>> GetSalesDataAsync(string region, CancellationToken cancellationToken = default);
}

public class DatabricksService : IDatabricksService
{
    private readonly DatabricksSettings _settings;
    private readonly ILogger<DatabricksService> _logger;
    private readonly ActivitySource _activitySource;

    private static readonly string[] Regions = { "Latin America", "Southeast Asia", "East Africa", "Caribbean", "Pacific Islands", "West Africa" };
    private static readonly string[] Varieties = { "Cavendish", "Plantain", "Red Banana", "Lady Finger", "Blue Java", "Burro" };
    private static readonly string[] Countries = { "Ecuador", "Philippines", "India", "China", "Brazil", "Indonesia", "Tanzania", "Costa Rica", "Colombia", "Guatemala" };

    public DatabricksService(
        IOptions<DatabricksSettings> settings,
        ILogger<DatabricksService> logger)
    {
        _settings = settings.Value;
        _logger = logger;
        _activitySource = new ActivitySource("GrafanaBanana.Databricks");
    }

    public async Task<BananaAnalytics> GetBananaAnalyticsAsync(CancellationToken cancellationToken = default)
    {
        using var activity = _activitySource.StartActivity("GetBananaAnalytics");
        activity?.SetTag("databricks.mock_mode", _settings.MockMode);

        _logger.LogInformation("Fetching banana analytics from Databricks (Mock Mode: {MockMode})", _settings.MockMode);

        try
        {
            if (_settings.MockMode)
            {
                activity?.AddEvent(new ActivityEvent("Generating mock data"));
                return await GenerateMockAnalyticsAsync(cancellationToken);
            }
            else
            {
                activity?.AddEvent(new ActivityEvent("Querying Databricks"));
                // In real implementation, this would call Databricks SQL API
                throw new NotImplementedException("Real Databricks connection not implemented. Set MockMode to true.");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching banana analytics from Databricks");
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            throw;
        }
    }

    public async Task<List<BananaProduction>> GetProductionDataAsync(int year, CancellationToken cancellationToken = default)
    {
        using var activity = _activitySource.StartActivity("GetProductionData");
        activity?.SetTag("databricks.query_year", year);
        activity?.SetTag("databricks.mock_mode", _settings.MockMode);

        _logger.LogInformation("Fetching banana production data for year {Year}", year);

        await Task.Delay(Random.Shared.Next(50, 200), cancellationToken); // Simulate query time

        var productions = new List<BananaProduction>();
        
        foreach (var region in Regions)
        {
            for (int month = 1; month <= 12; month++)
            {
                var variety = Varieties[Random.Shared.Next(Varieties.Length)];
                productions.Add(new BananaProduction(
                    Region: region,
                    Year: year,
                    Month: month,
                    TonsProduced: Random.Shared.Next(10000, 500000),
                    AverageQualityScore: Math.Round(Random.Shared.NextDouble() * 2 + 3, 2), // 3.0 to 5.0
                    VarietyName: variety,
                    ExportPercentage: Math.Round(Random.Shared.NextDouble() * 60 + 20, 2) // 20% to 80%
                ));
            }
        }

        activity?.SetTag("databricks.records_returned", productions.Count);
        _logger.LogInformation("Retrieved {Count} production records", productions.Count);

        return productions;
    }

    public async Task<List<BananaSalesData>> GetSalesDataAsync(string region, CancellationToken cancellationToken = default)
    {
        using var activity = _activitySource.StartActivity("GetSalesData");
        activity?.SetTag("databricks.query_region", region);
        activity?.SetTag("databricks.mock_mode", _settings.MockMode);

        // Sanitize region parameter for logging to prevent log forging
        var sanitizedRegion = region?.Replace("\n", "").Replace("\r", "") ?? "unknown";
        _logger.LogInformation("Fetching banana sales data for region {Region}", sanitizedRegion);

        await Task.Delay(Random.Shared.Next(50, 150), cancellationToken); // Simulate query time

        var sales = Countries.Select(country => new BananaSalesData(
            Country: country,
            TotalSales: Math.Round(Random.Shared.NextDouble() * 10000000 + 500000, 2),
            TotalBunches: Random.Shared.Next(100000, 5000000),
            AveragePrice: Math.Round(Random.Shared.NextDouble() * 3 + 1, 2), // $1 to $4
            MarketShare: Math.Round(Random.Shared.NextDouble() * 25 + 5, 2) // 5% to 30%
        )).ToList();

        activity?.SetTag("databricks.records_returned", sales.Count);
        _logger.LogInformation("Retrieved {Count} sales records", sales.Count);

        return sales;
    }

    private async Task<BananaAnalytics> GenerateMockAnalyticsAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Generating mock banana analytics data");

        // Simulate Databricks query execution time
        await Task.Delay(Random.Shared.Next(100, 500), cancellationToken);

        var currentYear = DateTime.UtcNow.Year;
        var productions = await GetProductionDataAsync(currentYear, cancellationToken);
        var sales = await GetSalesDataAsync("Global", cancellationToken);

        var totalProduction = productions.Sum(p => p.TonsProduced);
        var avgQuality = Math.Round(productions.Average(p => p.AverageQualityScore), 2);
        var totalRevenue = sales.Sum(s => s.TotalSales);
        var topRegion = productions
            .GroupBy(p => p.Region)
            .OrderByDescending(g => g.Sum(p => p.TonsProduced))
            .First()
            .Key;
        var topVariety = productions
            .GroupBy(p => p.VarietyName)
            .OrderByDescending(g => g.Count())
            .First()
            .Key;

        var summary = new BananaAnalyticsSummary(
            TotalProductionTons: totalProduction,
            GlobalAverageQuality: avgQuality,
            TotalRevenue: totalRevenue,
            CountriesServed: sales.Count,
            TopProducingRegion: topRegion,
            MostPopularVariety: topVariety
        );

        var analytics = new BananaAnalytics(
            GeneratedAt: DateTime.UtcNow,
            DataSource: "Databricks SQL Warehouse (Mock)",
            Productions: productions.Take(20).ToList(), // Return top 20 for display
            Sales: sales,
            Summary: summary
        );

        _logger.LogInformation(
            "Generated mock analytics: {ProductionTons} tons, {Revenue} revenue, {Countries} countries",
            totalProduction, totalRevenue, sales.Count);

        return analytics;
    }
}
