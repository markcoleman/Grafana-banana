# Enterprise Architecture Patterns - Code Examples

This document provides practical, working code examples demonstrating the enterprise architecture patterns used in Grafana-banana. These examples are designed to help GitHub Copilot understand and generate code following the same patterns.

## Table of Contents

1. [CQRS Pattern Examples](#cqrs-pattern-examples)
2. [Repository Pattern Examples](#repository-pattern-examples)
3. [Domain Entity Examples](#domain-entity-examples)
4. [Dependency Injection Examples](#dependency-injection-examples)
5. [API Endpoint Examples](#api-endpoint-examples)
6. [Testing Examples](#testing-examples)

---

## CQRS Pattern Examples

### Example 1: Simple Query (Read Operation)

**Query Definition** (`Application/Queries/GetWeatherForecastQuery.cs`):
```csharp
using GrafanaBanana.Api.Domain.Entities;
using MediatR;

namespace GrafanaBanana.Api.Application.Queries;

/// <summary>
/// Query to retrieve weather forecasts.
/// Implements CQRS pattern for read operations.
/// </summary>
/// <param name="Days">Number of days to forecast (default: 5).</param>
public record GetWeatherForecastQuery(int Days = 5) : IRequest<IEnumerable<WeatherForecast>>;
```

**Handler Implementation** (`Application/Handlers/GetWeatherForecastQueryHandler.cs`):
```csharp
using GrafanaBanana.Api.Application.Queries;
using GrafanaBanana.Api.Domain.Entities;
using GrafanaBanana.Api.Domain.Repositories;
using MediatR;

namespace GrafanaBanana.Api.Application.Handlers;

/// <summary>
/// Handler for GetWeatherForecastQuery.
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

    public async Task<IEnumerable<WeatherForecast>> Handle(
        GetWeatherForecastQuery request, 
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Handling GetWeatherForecastQuery for {Days} days", request.Days);

        var forecasts = await _repository.GetForecastsAsync(request.Days, cancellationToken);

        _logger.LogInformation("Successfully retrieved {Count} weather forecast entries", forecasts.Count());

        return forecasts;
    }
}
```

### Example 2: Query with Parameter

**Query Definition** (`Application/Queries/GetBananaProductionQuery.cs`):
```csharp
using GrafanaBanana.Api.Databricks;
using MediatR;

namespace GrafanaBanana.Api.Application.Queries;

/// <summary>
/// Query to retrieve banana production data for a specific year.
/// </summary>
/// <param name="Year">The year to get production data for.</param>
public record GetBananaProductionQuery(int Year) : IRequest<BananaProduction>;
```

**Handler Implementation**:
```csharp
using GrafanaBanana.Api.Application.Queries;
using GrafanaBanana.Api.Databricks;
using GrafanaBanana.Api.Domain.Repositories;
using MediatR;

namespace GrafanaBanana.Api.Application.Handlers;

/// <summary>
/// Handler for GetBananaProductionQuery.
/// </summary>
public class GetBananaProductionQueryHandler : IRequestHandler<GetBananaProductionQuery, BananaProduction>
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

    public async Task<BananaProduction> Handle(
        GetBananaProductionQuery request, 
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Fetching banana production for year {Year}", request.Year);

        var production = await _repository.GetProductionByYearAsync(request.Year, cancellationToken);

        _logger.LogInformation("Retrieved production data for {Year}", request.Year);

        return production;
    }
}
```

### Example 3: Command (Write Operation)

**Command Definition** (`Application/Commands/CreateBananaRecordCommand.cs`):
```csharp
using MediatR;

namespace GrafanaBanana.Api.Application.Commands;

/// <summary>
/// Command to create a new banana production record.
/// Implements CQRS pattern for write operations.
/// </summary>
/// <param name="Region">The region where bananas are produced.</param>
/// <param name="ProductionTons">Amount produced in tons.</param>
/// <param name="Year">Production year.</param>
public record CreateBananaRecordCommand(
    string Region, 
    decimal ProductionTons, 
    int Year) : IRequest<Guid>;
```

**Handler Implementation**:
```csharp
using GrafanaBanana.Api.Application.Commands;
using GrafanaBanana.Api.Domain.Repositories;
using MediatR;

namespace GrafanaBanana.Api.Application.Handlers;

/// <summary>
/// Handler for CreateBananaRecordCommand.
/// </summary>
public class CreateBananaRecordCommandHandler : IRequestHandler<CreateBananaRecordCommand, Guid>
{
    private readonly IBananaAnalyticsRepository _repository;
    private readonly ILogger<CreateBananaRecordCommandHandler> _logger;

    public CreateBananaRecordCommandHandler(
        IBananaAnalyticsRepository repository,
        ILogger<CreateBananaRecordCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<Guid> Handle(
        CreateBananaRecordCommand request, 
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Creating banana production record for region {Region}", request.Region);

        // Validate business rules
        if (request.ProductionTons <= 0)
            throw new ArgumentException("Production must be greater than zero", nameof(request.ProductionTons));

        if (request.Year < 2000 || request.Year > DateTime.UtcNow.Year)
            throw new ArgumentException("Invalid year", nameof(request.Year));

        // Create and save record
        var id = await _repository.CreateProductionRecordAsync(
            request.Region, 
            request.ProductionTons, 
            request.Year, 
            cancellationToken);

        _logger.LogInformation("Created production record with ID {Id}", id);

        return id;
    }
}
```

---

## Repository Pattern Examples

### Example 1: Simple Repository Interface

**Interface** (`Domain/Repositories/IWeatherForecastRepository.cs`):
```csharp
using GrafanaBanana.Api.Domain.Entities;

namespace GrafanaBanana.Api.Domain.Repositories;

/// <summary>
/// Repository interface for weather forecast data access.
/// Part of Domain layer - defines contract without implementation details.
/// </summary>
public interface IWeatherForecastRepository
{
    /// <summary>
    /// Retrieves weather forecasts for the specified number of days.
    /// </summary>
    Task<IEnumerable<WeatherForecast>> GetForecastsAsync(
        int days, 
        CancellationToken cancellationToken = default);
}
```

**Implementation** (`Infrastructure/Repositories/WeatherForecastRepository.cs`):
```csharp
using GrafanaBanana.Api.Domain.Entities;
using GrafanaBanana.Api.Domain.Repositories;

namespace GrafanaBanana.Api.Infrastructure.Repositories;

/// <summary>
/// Repository implementation for weather forecast data.
/// Part of Infrastructure layer - handles actual data access.
/// </summary>
public class WeatherForecastRepository : IWeatherForecastRepository
{
    private readonly ILogger<WeatherForecastRepository> _logger;
    private static readonly string[] Summaries = new[]
    {
        "Freezing", "Bracing", "Chilly", "Cool", "Mild", 
        "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
    };

    public WeatherForecastRepository(ILogger<WeatherForecastRepository> logger)
    {
        _logger = logger;
    }

    public async Task<IEnumerable<WeatherForecast>> GetForecastsAsync(
        int days, 
        CancellationToken cancellationToken)
    {
        _logger.LogDebug("Generating {Days} weather forecasts", days);

        // Simulate async data access
        await Task.Delay(10, cancellationToken);

        var forecasts = Enumerable.Range(1, days).Select(index =>
        {
            var date = DateOnly.FromDateTime(DateTime.Now.AddDays(index));
            var temperatureC = Random.Shared.Next(-20, 55);
            var summary = Summaries[Random.Shared.Next(Summaries.Length)];

            return WeatherForecast.Create(date, temperatureC, summary);
        }).ToArray();

        return forecasts;
    }
}
```

### Example 2: Repository with External Service

**Interface** (`Domain/Repositories/IBananaAnalyticsRepository.cs`):
```csharp
using GrafanaBanana.Api.Databricks;

namespace GrafanaBanana.Api.Domain.Repositories;

/// <summary>
/// Repository interface for banana analytics data.
/// </summary>
public interface IBananaAnalyticsRepository
{
    Task<BananaAnalytics> GetAnalyticsAsync(CancellationToken cancellationToken = default);
    Task<BananaProduction> GetProductionByYearAsync(int year, CancellationToken cancellationToken = default);
    Task<BananaSales> GetSalesByRegionAsync(string region, CancellationToken cancellationToken = default);
}
```

**Implementation using External Service**:
```csharp
using GrafanaBanana.Api.Databricks;
using GrafanaBanana.Api.Domain.Repositories;

namespace GrafanaBanana.Api.Infrastructure.Repositories;

/// <summary>
/// Repository implementation that delegates to Databricks service.
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

    public async Task<BananaAnalytics> GetAnalyticsAsync(CancellationToken cancellationToken)
    {
        _logger.LogDebug("Fetching banana analytics from Databricks");
        return await _databricksService.GetBananaAnalyticsAsync();
    }

    public async Task<BananaProduction> GetProductionByYearAsync(
        int year, 
        CancellationToken cancellationToken)
    {
        _logger.LogDebug("Fetching banana production for year {Year}", year);
        return await _databricksService.GetBananaProductionAsync(year);
    }

    public async Task<BananaSales> GetSalesByRegionAsync(
        string region, 
        CancellationToken cancellationToken)
    {
        _logger.LogDebug("Fetching banana sales for region {Region}", region);
        return await _databricksService.GetBananaSalesAsync(region);
    }
}
```

---

## Domain Entity Examples

### Example 1: Entity with Factory Method

```csharp
namespace GrafanaBanana.Api.Domain.Entities;

/// <summary>
/// Domain entity representing a weather forecast.
/// Demonstrates encapsulation, factory method, and domain logic.
/// </summary>
public class WeatherForecast
{
    /// <summary>
    /// Gets the forecast date.
    /// </summary>
    public DateOnly Date { get; private set; }

    /// <summary>
    /// Gets the temperature in Celsius.
    /// </summary>
    public int TemperatureC { get; private set; }

    /// <summary>
    /// Gets the weather summary.
    /// </summary>
    public string? Summary { get; private set; }

    /// <summary>
    /// Gets the temperature in Fahrenheit (calculated property).
    /// Demonstrates domain logic within entity.
    /// </summary>
    public int TemperatureF => 32 + (int)(TemperatureC * 9.0 / 5.0);

    private WeatherForecast(DateOnly date, int temperatureC, string? summary)
    {
        Date = date;
        TemperatureC = temperatureC;
        Summary = summary;
    }

    /// <summary>
    /// Factory method for creating valid WeatherForecast instances.
    /// </summary>
    public static WeatherForecast Create(DateOnly date, int temperatureC, string? summary)
    {
        // Domain validation
        if (temperatureC < -273)
            throw new ArgumentException("Temperature cannot be below absolute zero", nameof(temperatureC));

        return new WeatherForecast(date, temperatureC, summary);
    }
}
```

### Example 2: Entity with Business Logic

```csharp
namespace GrafanaBanana.Api.Domain.Entities;

/// <summary>
/// Domain entity for banana shipment.
/// Demonstrates rich domain model with business rules.
/// </summary>
public class BananaShipment
{
    public Guid Id { get; private set; }
    public string Origin { get; private set; }
    public string Destination { get; private set; }
    public decimal WeightTons { get; private set; }
    public DateTime ShipmentDate { get; private set; }
    public ShipmentStatus Status { get; private set; }

    private BananaShipment(string origin, string destination, decimal weightTons)
    {
        Id = Guid.NewGuid();
        Origin = origin;
        Destination = destination;
        WeightTons = weightTons;
        ShipmentDate = DateTime.UtcNow;
        Status = ShipmentStatus.Pending;
    }

    /// <summary>
    /// Factory method with validation.
    /// </summary>
    public static BananaShipment Create(string origin, string destination, decimal weightTons)
    {
        if (string.IsNullOrWhiteSpace(origin))
            throw new ArgumentException("Origin is required", nameof(origin));

        if (string.IsNullOrWhiteSpace(destination))
            throw new ArgumentException("Destination is required", nameof(destination));

        if (weightTons <= 0)
            throw new ArgumentException("Weight must be greater than zero", nameof(weightTons));

        return new BananaShipment(origin, destination, weightTons);
    }

    /// <summary>
    /// Business logic: Mark shipment as dispatched.
    /// </summary>
    public void Dispatch()
    {
        if (Status != ShipmentStatus.Pending)
            throw new InvalidOperationException("Can only dispatch pending shipments");

        Status = ShipmentStatus.Dispatched;
    }

    /// <summary>
    /// Business logic: Mark shipment as delivered.
    /// </summary>
    public void MarkAsDelivered()
    {
        if (Status != ShipmentStatus.Dispatched)
            throw new InvalidOperationException("Can only deliver dispatched shipments");

        Status = ShipmentStatus.Delivered;
    }

    /// <summary>
    /// Business logic: Calculate shipping cost based on weight.
    /// </summary>
    public decimal CalculateShippingCost()
    {
        const decimal costPerTon = 100m;
        decimal baseCost = WeightTons * costPerTon;

        // Business rule: Discount for large shipments
        if (WeightTons > 10)
            baseCost *= 0.9m; // 10% discount

        return baseCost;
    }
}

public enum ShipmentStatus
{
    Pending,
    Dispatched,
    Delivered,
    Cancelled
}
```

---

## Dependency Injection Examples

### Example 1: Service Registration

**In `Application/DependencyInjection.cs`**:
```csharp
using GrafanaBanana.Api.Domain.Repositories;
using GrafanaBanana.Api.Infrastructure.Repositories;
using System.Reflection;

namespace GrafanaBanana.Api.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        // Register MediatR (scans for all handlers automatically)
        services.AddMediatR(cfg => 
            cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));

        // Register repositories with scoped lifetime
        services.AddScoped<IWeatherForecastRepository, WeatherForecastRepository>();
        services.AddScoped<IBananaAnalyticsRepository, BananaAnalyticsRepository>();

        return services;
    }
}
```

### Example 2: Using Services in Handlers

```csharp
public class SomeQueryHandler : IRequestHandler<SomeQuery, SomeResult>
{
    private readonly IRepository _repository;
    private readonly ILogger<SomeQueryHandler> _logger;
    private readonly IOtherService _otherService;

    // Constructor injection - all dependencies provided by DI container
    public SomeQueryHandler(
        IRepository repository,
        ILogger<SomeQueryHandler> logger,
        IOtherService otherService)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _otherService = otherService ?? throw new ArgumentNullException(nameof(otherService));
    }

    public async Task<SomeResult> Handle(SomeQuery request, CancellationToken cancellationToken)
    {
        // Use injected dependencies
        _logger.LogInformation("Processing request");
        var data = await _repository.GetDataAsync(cancellationToken);
        var processed = await _otherService.ProcessAsync(data);
        return processed;
    }
}
```

---

## API Endpoint Examples

### Example 1: Simple GET Endpoint with MediatR

```csharp
app.MapGet("/weatherforecast", async (IMediator mediator, ILogger<Program> logger) =>
{
    logger.LogInformation("Processing weather forecast request");

    try
    {
        var forecast = await mediator.Send(new GetWeatherForecastQuery(Days: 5));
        return Results.Ok(forecast);
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Error fetching weather forecast");
        return Results.Problem("Failed to fetch weather forecast");
    }
})
.WithName("GetWeatherForecast")
.WithOpenApi()
.RequireRateLimiting("api");
```

### Example 2: GET with Route Parameter

```csharp
app.MapGet("/api/databricks/production/{year:int}", async (
    int year,
    IMediator mediator,
    ILogger<Program> logger) =>
{
    logger.LogInformation("Processing banana production request for year {Year}", year);

    try
    {
        var production = await mediator.Send(new GetBananaProductionQuery(year));
        return Results.Ok(production);
    }
    catch (ArgumentException ex)
    {
        logger.LogWarning(ex, "Invalid year: {Year}", year);
        return Results.BadRequest(new { error = ex.Message });
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Error fetching production data for year {Year}", year);
        return Results.Problem($"Failed to fetch production data for year {year}");
    }
})
.WithName("GetBananaProduction")
.WithOpenApi()
.RequireRateLimiting("api");
```

### Example 3: POST Endpoint with Command

```csharp
app.MapPost("/api/banana/records", async (
    CreateBananaRecordCommand command,
    IMediator mediator,
    ILogger<Program> logger) =>
{
    logger.LogInformation("Creating banana production record");

    try
    {
        var id = await mediator.Send(command);
        return Results.Created($"/api/banana/records/{id}", new { id });
    }
    catch (ArgumentException ex)
    {
        logger.LogWarning(ex, "Invalid command data");
        return Results.BadRequest(new { error = ex.Message });
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Error creating banana record");
        return Results.Problem("Failed to create banana record");
    }
})
.WithName("CreateBananaRecord")
.WithOpenApi()
.RequireRateLimiting("api");
```

---

## Testing Examples

### Example 1: Testing a Query Handler

```csharp
using Xunit;
using Moq;
using Microsoft.Extensions.Logging;

public class GetWeatherForecastQueryHandlerTests
{
    [Fact]
    public async Task Handle_ValidRequest_ReturnsForecasts()
    {
        // Arrange
        var expectedDate = DateOnly.FromDateTime(DateTime.Now.AddDays(1));
        var expectedForecasts = new[]
        {
            WeatherForecast.Create(expectedDate, 20, "Sunny")
        };

        var mockRepository = new Mock<IWeatherForecastRepository>();
        mockRepository
            .Setup(r => r.GetForecastsAsync(5, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedForecasts);

        var mockLogger = new Mock<ILogger<GetWeatherForecastQueryHandler>>();

        var handler = new GetWeatherForecastQueryHandler(
            mockRepository.Object,
            mockLogger.Object);

        var query = new GetWeatherForecastQuery(Days: 5);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result);
        Assert.Equal(20, result.First().TemperatureC);
        Assert.Equal("Sunny", result.First().Summary);

        // Verify repository was called
        mockRepository.Verify(
            r => r.GetForecastsAsync(5, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_RepositoryThrowsException_PropagatesException()
    {
        // Arrange
        var mockRepository = new Mock<IWeatherForecastRepository>();
        mockRepository
            .Setup(r => r.GetForecastsAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Database error"));

        var mockLogger = new Mock<ILogger<GetWeatherForecastQueryHandler>>();

        var handler = new GetWeatherForecastQueryHandler(
            mockRepository.Object,
            mockLogger.Object);

        var query = new GetWeatherForecastQuery(Days: 5);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => handler.Handle(query, CancellationToken.None));
    }
}
```

### Example 2: Testing a Domain Entity

```csharp
public class WeatherForecastTests
{
    [Fact]
    public void Create_ValidParameters_CreatesInstance()
    {
        // Arrange
        var date = DateOnly.FromDateTime(DateTime.Now);
        var temperatureC = 20;
        var summary = "Sunny";

        // Act
        var forecast = WeatherForecast.Create(date, temperatureC, summary);

        // Assert
        Assert.NotNull(forecast);
        Assert.Equal(date, forecast.Date);
        Assert.Equal(temperatureC, forecast.TemperatureC);
        Assert.Equal(summary, forecast.Summary);
    }

    [Fact]
    public void Create_TemperatureBelowAbsoluteZero_ThrowsException()
    {
        // Arrange
        var date = DateOnly.FromDateTime(DateTime.Now);
        var temperatureC = -300; // Below absolute zero

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(
            () => WeatherForecast.Create(date, temperatureC, "Cold"));

        Assert.Contains("absolute zero", exception.Message);
    }

    [Fact]
    public void TemperatureF_CalculatesCorrectly()
    {
        // Arrange
        var forecast = WeatherForecast.Create(
            DateOnly.FromDateTime(DateTime.Now),
            0, // 0°C
            "Freezing");

        // Act
        var temperatureF = forecast.TemperatureF;

        // Assert
        Assert.Equal(32, temperatureF); // 0°C = 32°F
    }
}
```

---

## Summary

These examples demonstrate the core patterns used in Grafana-banana:

1. **CQRS**: Queries and Commands separated with dedicated handlers
2. **Repository**: Interface in Domain, implementation in Infrastructure
3. **Domain Entities**: Rich models with validation and business logic
4. **Dependency Injection**: Constructor injection with interface dependencies
5. **API Endpoints**: Thin controllers using MediatR
6. **Testing**: Mock dependencies, test business logic in isolation

When using GitHub Copilot, reference these patterns to generate consistent, maintainable code that follows enterprise architecture best practices.
