namespace GrafanaBanana.Api.Databricks;

/// <summary>
/// Configuration settings for Databricks connection
/// </summary>
public class DatabricksSettings
{
    public string ServerHostname { get; set; } = string.Empty;
    public string HttpPath { get; set; } = string.Empty;
    public string AccessToken { get; set; } = string.Empty;
    public bool MockMode { get; set; } = true;
    public int QueryTimeout { get; set; } = 30;
}

/// <summary>
/// Represents banana production data from Databricks analytics
/// </summary>
public record BananaProduction(
    string Region,
    int Year,
    int Month,
    double TonsProduced,
    double AverageQualityScore,
    string VarietyName,
    double ExportPercentage
);

/// <summary>
/// Represents banana sales analytics
/// </summary>
public record BananaSalesData(
    string Country,
    double TotalSales,
    int TotalBunches,
    double AveragePrice,
    double MarketShare
);

/// <summary>
/// Aggregated banana analytics dashboard data
/// </summary>
public record BananaAnalytics(
    DateTime GeneratedAt,
    string DataSource,
    List<BananaProduction> Productions,
    List<BananaSalesData> Sales,
    BananaAnalyticsSummary Summary
);

/// <summary>
/// Summary statistics for banana analytics
/// </summary>
public record BananaAnalyticsSummary(
    double TotalProductionTons,
    double GlobalAverageQuality,
    double TotalRevenue,
    int CountriesServed,
    string TopProducingRegion,
    string MostPopularVariety
);
