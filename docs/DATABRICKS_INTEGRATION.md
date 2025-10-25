# Databricks Banana Analytics Integration 🍌🧱

This document describes the fun Databricks integration added to Grafana-banana, featuring a banana-themed analytics dashboard.

## Overview

The Databricks integration provides mock banana production and sales analytics, simulating queries to a Databricks SQL Warehouse. This demonstrates how to integrate Databricks data sources into a full-stack application with comprehensive observability.

## Features

### Backend API Endpoints

#### 1. Get Banana Analytics Dashboard
```
GET /api/databricks/banana-analytics
```

Returns comprehensive banana analytics including production data, sales data, and summary statistics.

**Response Example:**
```json
{
  "generatedAt": "2025-10-25T23:39:01Z",
  "dataSource": "Databricks SQL Warehouse (Mock)",
  "summary": {
    "totalProductionTons": 17395137,
    "globalAverageQuality": 4.04,
    "totalRevenue": 62642117.30,
    "countriesServed": 10,
    "topProducingRegion": "Latin America",
    "mostPopularVariety": "Blue Java"
  },
  "productions": [...],
  "sales": [...]
}
```

#### 2. Get Production Data by Year
```
GET /api/databricks/production/{year}
```

Returns detailed banana production data for a specific year, broken down by region, month, and variety.

**Example:** `GET /api/databricks/production/2025`

#### 3. Get Sales Data
```
GET /api/databricks/sales?region={region}
```

Returns banana sales data by country, optionally filtered by region.

**Example:** `GET /api/databricks/sales?region=Global`

### Frontend Dashboard

The Angular frontend displays an interactive Banana Analytics dashboard with:

- **Summary Metrics**:
  - 🏭 Total Production (in tons)
  - 💰 Total Revenue
  - ⭐ Quality Score (1-5 scale)

- **Additional Statistics**:
  - 🌍 Countries Served
  - 📍 Top Producing Region
  - 🥇 Most Popular Variety

- **Top Sales by Country**:
  - Sales amounts and bunch counts
  - Market share percentages
  - Interactive cards with hover effects

## Configuration

Databricks settings are configured in `appsettings.json`:

```json
{
  "Databricks": {
    "ServerHostname": "your-databricks-workspace.cloud.databricks.com",
    "HttpPath": "/sql/1.0/warehouses/your-warehouse-id",
    "AccessToken": "your-databricks-token",
    "MockMode": true,
    "QueryTimeout": 30
  }
}
```

### Configuration Options

- **ServerHostname**: Databricks workspace hostname
- **HttpPath**: SQL Warehouse HTTP path
- **AccessToken**: Personal access token for authentication
- **MockMode**: When `true`, generates mock data instead of querying Databricks
- **QueryTimeout**: Query timeout in seconds (default: 30)

## Mock Data

In mock mode (default), the service generates realistic banana production and sales data including:

### Regions
- Latin America
- Southeast Asia
- East Africa
- Caribbean
- Pacific Islands
- West Africa

### Banana Varieties
- Cavendish (most common commercial variety)
- Plantain (cooking banana)
- Red Banana
- Lady Finger
- Blue Java (ice cream banana)
- Burro

### Countries
- Ecuador
- Philippines
- India
- China
- Brazil
- Indonesia
- Tanzania
- Costa Rica
- Colombia
- Guatemala

## Observability

The Databricks integration includes full observability support:

### Metrics

Custom Prometheus metrics tracked:

```
# Total number of Databricks queries
databricks_queries_total{query_type="full_analytics|production|sales"}

# Query duration in milliseconds
databricks_query_duration_ms{query_type="full_analytics|production|sales"}
```

Available at: `http://localhost:5000/metrics`

### Distributed Tracing

OpenTelemetry traces for all Databricks operations:

- Activity source: `GrafanaBanana.Databricks`
- Spans include query type, region, year parameters
- Automatic error tracking and status codes

### Structured Logging

All Databricks operations are logged with Serilog:

```
[INFO] Fetching banana analytics from Databricks (Mock Mode: True)
[INFO] Retrieved 72 production records
[INFO] Generated mock analytics: 17395137 tons, 62642117.3 revenue, 10 countries
```

## Architecture

### Backend Structure

```
backend/GrafanaBanana.Api/
├── Databricks/
│   ├── Models.cs              # Data models and records
│   └── DatabricksService.cs   # Service implementation
└── Program.cs                 # API endpoint registration
```

### Frontend Structure

```
frontend/src/app/
├── databricks.service.ts      # Angular service for API calls
├── app.ts                     # Component with analytics logic
└── app.html                   # Dashboard UI template
```

## Data Models

### BananaProduction
```csharp
record BananaProduction(
    string Region,
    int Year,
    int Month,
    double TonsProduced,
    double AverageQualityScore,
    string VarietyName,
    double ExportPercentage
);
```

### BananaSalesData
```csharp
record BananaSalesData(
    string Country,
    double TotalSales,
    int TotalBunches,
    double AveragePrice,
    double MarketShare
);
```

### BananaAnalyticsSummary
```csharp
record BananaAnalyticsSummary(
    double TotalProductionTons,
    double GlobalAverageQuality,
    double TotalRevenue,
    int CountriesServed,
    string TopProducingRegion,
    string MostPopularVariety
);
```

## Real Databricks Integration

To connect to a real Databricks SQL Warehouse:

1. Update `appsettings.json` with your Databricks credentials
2. Set `MockMode` to `false`
3. Install Databricks SQL connector (if needed):
   ```bash
   dotnet add package Databricks.Sql.Client
   ```
4. Implement real query execution in `DatabricksService.cs`

### Example Real Implementation

```csharp
// In DatabricksService.cs
// NOTE: This is pseudo-code showing the general pattern
// Actual implementation depends on the Databricks connector used
if (!_settings.MockMode)
{
    using var connection = new DatabricksConnection
    {
        ServerHostname = _settings.ServerHostname,
        HttpPath = _settings.HttpPath,
        AccessToken = _settings.AccessToken
    };
    
    await connection.OpenAsync(cancellationToken);
    
    var command = connection.CreateCommand();
    command.CommandText = "SELECT * FROM banana_production WHERE year = @year";
    // Execute query and process results...
}
```

## Testing

### Manual Testing

1. Start the backend:
   ```bash
   cd backend/GrafanaBanana.Api
   dotnet run
   ```

2. Test endpoints:
   ```bash
   # Get full analytics
   curl http://localhost:5000/api/databricks/banana-analytics | jq

   # Get production data for 2025
   curl http://localhost:5000/api/databricks/production/2025 | jq

   # Get sales data
   curl http://localhost:5000/api/databricks/sales | jq
   ```

3. View metrics:
   ```bash
   curl http://localhost:5000/metrics | grep databricks
   ```

### Frontend Testing

1. Start the frontend:
   ```bash
   cd frontend
   npm start
   ```

2. Open browser to: `http://localhost:4200`
3. View the Banana Analytics dashboard above the Weather Forecast section

## Performance

- **Mock Mode**: Query latency 50-500ms (simulated)
- **Response Size**: ~30KB for full analytics
- **Caching**: None (queries executed on each request)

## Future Enhancements

Potential improvements:

- [ ] Add caching with Redis
- [ ] Implement time-series charts with production trends
- [ ] Add filtering by region, variety, and date range
- [ ] Real-time data updates with SignalR
- [ ] Export functionality (CSV, Excel)
- [ ] Predictive analytics with ML.NET
- [ ] Integration with real Databricks Delta Lake
- [ ] Advanced visualizations (maps, heat maps)

## Screenshots

The dashboard features a modern glass-morphism design with:
- Gradient color schemes
- Responsive layout
- Real-time data loading
- Smooth animations and hover effects

For screenshots of the application, see the PR description or the [docs/screenshots](../screenshots/) directory.

## API Rate Limiting

All Databricks endpoints are protected by the application's rate limiting policy:

```csharp
.RequireRateLimiting("api");
```

## Security Considerations

When connecting to real Databricks:

1. **Never commit access tokens** to source control
2. Use **Azure Key Vault** or **environment variables** for credentials
3. Implement **IP allowlisting** on Databricks workspace
4. Use **service principals** instead of personal access tokens in production
5. Enable **audit logging** for all queries
6. Set appropriate **query timeouts** to prevent resource exhaustion

## Support

For issues or questions about the Databricks integration:

1. Check application logs in `logs/grafana-banana-*.log`
2. View OpenTelemetry traces in Grafana Tempo
3. Monitor metrics in Prometheus/Grafana
4. Review the error messages in the browser console

## License

This integration follows the same MIT License as the parent Grafana-banana project.
