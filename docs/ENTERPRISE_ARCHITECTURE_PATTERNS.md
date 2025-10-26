# Enterprise Architecture Patterns Implementation

This document describes the enterprise architecture patterns implemented in the Grafana-banana backend API.

## Overview

The backend has been refactored to follow enterprise-grade architecture patterns including:

- **Clean Architecture** - Separation of concerns into distinct layers
- **CQRS Pattern** - Command Query Responsibility Segregation
- **Repository Pattern** - Data access abstraction
- **Mediator Pattern** - Decoupled request/response handling
- **Dependency Inversion Principle** - Programming to interfaces, not implementations

## Architecture Layers

### 1. Domain Layer (`Domain/`)

The innermost layer containing business logic and enterprise rules.

**Purpose**: 
- Contains domain entities and business logic
- Defines repository interfaces (contracts)
- Has no dependencies on other layers
- Represents the core business model

**Components**:

- **Entities** (`Domain/Entities/`)
  - `WeatherForecast.cs` - Domain entity for weather data
  - Contains business logic and validation
  - Factory methods for creating valid entities

- **Repository Interfaces** (`Domain/Repositories/`)
  - `IWeatherForecastRepository.cs` - Contract for weather data access
  - `IBananaAnalyticsRepository.cs` - Contract for banana analytics data access
  - Define data access operations without implementation details

**Example**:
```csharp
public class WeatherForecast
{
    // Encapsulated properties
    public DateOnly Date { get; private set; }
    public int TemperatureC { get; private set; }
    
    // Domain logic
    public int TemperatureF => 32 + (int)(TemperatureC * 9.0 / 5.0);
    
    // Factory method with validation
    public static WeatherForecast Create(DateOnly date, int temperatureC, string? summary)
    {
        if (temperatureC < -273) // Business rule validation
            throw new ArgumentException("Temperature cannot be below absolute zero");
        
        return new WeatherForecast(date, temperatureC, summary);
    }
}
```

### 2. Application Layer (`Application/`)

Contains application-specific business rules and orchestrates the flow of data.

**Purpose**:
- Implements use cases
- Coordinates domain entities
- Handles business workflows
- Independent of delivery mechanism (API, CLI, etc.)

**Components**:

- **Queries** (`Application/Queries/`)
  - Read operations following CQRS pattern
  - `GetWeatherForecastQuery.cs` - Query for weather data
  - `GetBananaAnalyticsQuery.cs` - Query for banana analytics
  - `GetBananaProductionQuery.cs` - Query for production data
  - `GetBananaSalesQuery.cs` - Query for sales data

- **Handlers** (`Application/Handlers/`)
  - Implement query/command handling logic
  - `GetWeatherForecastQueryHandler.cs` - Handles weather queries
  - `GetBananaAnalyticsQueryHandler.cs` - Handles analytics queries
  - Uses repository abstractions to access data
  - Contains business logic coordination

- **Dependency Injection** (`Application/DependencyInjection.cs`)
  - Registers all application services
  - Configures MediatR for CQRS
  - Wires up repositories and handlers

**Example Query**:
```csharp
// Query definition (request)
public record GetWeatherForecastQuery(int Days = 5) : IRequest<IEnumerable<WeatherForecast>>;

// Query handler (use case implementation)
public class GetWeatherForecastQueryHandler : IRequestHandler<GetWeatherForecastQuery, IEnumerable<WeatherForecast>>
{
    private readonly IWeatherForecastRepository _repository;
    
    public async Task<IEnumerable<WeatherForecast>> Handle(GetWeatherForecastQuery request, CancellationToken cancellationToken)
    {
        return await _repository.GetForecastsAsync(request.Days, cancellationToken);
    }
}
```

### 3. Infrastructure Layer (`Infrastructure/`)

Contains implementations of interfaces defined in the domain layer.

**Purpose**:
- Implements repository interfaces
- Handles external concerns (database, APIs, file system)
- Adapts external systems to domain contracts

**Components**:

- **Repositories** (`Infrastructure/Repositories/`)
  - `WeatherForecastRepository.cs` - Implementation for weather data
  - `BananaAnalyticsRepository.cs` - Adapter for Databricks service
  - Concrete implementations of domain repository interfaces

**Example Repository**:
```csharp
public class WeatherForecastRepository : IWeatherForecastRepository
{
    public Task<IEnumerable<WeatherForecast>> GetForecastsAsync(int days, CancellationToken cancellationToken)
    {
        // Implementation details hidden from domain
        var forecasts = Enumerable.Range(1, days)
            .Select(index => WeatherForecast.Create(
                DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                Random.Shared.Next(-20, 55),
                Summaries[Random.Shared.Next(Summaries.Length)]
            ));
        
        return Task.FromResult(forecasts);
    }
}
```

### 4. API Layer (`Program.cs`)

The outermost layer - the entry point and delivery mechanism.

**Purpose**:
- HTTP endpoint definitions
- Request/response handling
- Dependency injection configuration
- Middleware pipeline setup

**Components**:
- Minimal API endpoints
- MediatR integration
- OpenTelemetry instrumentation
- Security middleware

**Example Endpoint**:
```csharp
app.MapGet("/weatherforecast", async (IMediator mediator, ILogger<Program> logger) =>
{
    // Dispatch query through MediatR (CQRS)
    var forecast = await mediator.Send(new GetWeatherForecastQuery(Days: 5));
    return forecast;
})
.WithName("GetWeatherForecast")
.WithOpenApi()
.RequireRateLimiting("api");
```

## Design Patterns Implemented

### 1. Clean Architecture

**Benefits**:
- Independent of frameworks
- Testable business logic
- Independent of UI
- Independent of database
- Independent of external agencies

**Layer Dependencies**:
```
API Layer (Program.cs)
    ↓ depends on
Application Layer (Handlers, Queries)
    ↓ depends on
Domain Layer (Entities, Interfaces)
    ↑ implemented by
Infrastructure Layer (Repositories)
```

**Key Principle**: Dependencies point inward. The domain layer has no dependencies.

### 2. CQRS (Command Query Responsibility Segregation)

**Benefits**:
- Separate read and write models
- Optimized query performance
- Scalable architecture
- Clear separation of concerns

**Implementation**:
- Queries: Read operations returning data (GET requests)
- Commands: Write operations modifying state (POST/PUT/DELETE requests)
- MediatR library for dispatching queries/commands to handlers

**Flow**:
```
HTTP Request → API Endpoint → MediatR → Query/Command → Handler → Repository → Response
```

### 3. Repository Pattern

**Benefits**:
- Abstracts data access logic
- Enables unit testing with mock repositories
- Centralizes data access code
- Separates domain logic from data access

**Implementation**:
- Interface defined in Domain layer
- Implementation in Infrastructure layer
- Dependency injection wires them together

### 4. Mediator Pattern (via MediatR)

**Benefits**:
- Reduced coupling between components
- Single Responsibility Principle
- Easy to add new features
- Clean request/response flow

**Implementation**:
- Queries/Commands implement `IRequest<TResponse>`
- Handlers implement `IRequestHandler<TRequest, TResponse>`
- MediatR dispatches requests to appropriate handlers

### 5. Dependency Inversion Principle

**Benefits**:
- High-level modules don't depend on low-level modules
- Both depend on abstractions
- Enables loose coupling
- Facilitates testing and maintenance

**Implementation**:
```csharp
// High-level module depends on abstraction
public class GetWeatherForecastQueryHandler
{
    private readonly IWeatherForecastRepository _repository; // Interface, not concrete class
}

// Low-level module implements abstraction
public class WeatherForecastRepository : IWeatherForecastRepository
{
    // Concrete implementation
}
```

## Dependency Injection Setup

All layers are wired together using ASP.NET Core's built-in DI container:

```csharp
// In Program.cs
builder.Services.AddApplicationServices(); // Registers all architecture components

// In Application/DependencyInjection.cs
public static IServiceCollection AddApplicationServices(this IServiceCollection services)
{
    // Register MediatR for CQRS
    services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));

    // Register repositories (Repository Pattern)
    services.AddScoped<IWeatherForecastRepository, WeatherForecastRepository>();
    services.AddScoped<IBananaAnalyticsRepository, BananaAnalyticsRepository>();

    return services;
}
```

## Testing Strategy

The layered architecture enables comprehensive testing:

### Unit Tests
- **Domain Layer**: Test business logic in entities
- **Application Layer**: Test handlers with mocked repositories
- **Infrastructure Layer**: Test repository implementations

### Integration Tests
- Test end-to-end flows through the API
- Use in-memory implementations for repositories
- Test MediatR pipeline

### Example Unit Test
```csharp
public class GetWeatherForecastQueryHandlerTests
{
    [Fact]
    public async Task Handle_ReturnsForecasts_WhenRepositoryHasData()
    {
        // Arrange
        var mockRepo = new Mock<IWeatherForecastRepository>();
        var testForecasts = new List<WeatherForecast>
        {
            WeatherForecast.Create(DateOnly.FromDateTime(DateTime.Now.AddDays(1)), 20, "Sunny"),
            WeatherForecast.Create(DateOnly.FromDateTime(DateTime.Now.AddDays(2)), 22, "Cloudy"),
            WeatherForecast.Create(DateOnly.FromDateTime(DateTime.Now.AddDays(3)), 18, "Rainy"),
            WeatherForecast.Create(DateOnly.FromDateTime(DateTime.Now.AddDays(4)), 25, "Hot"),
            WeatherForecast.Create(DateOnly.FromDateTime(DateTime.Now.AddDays(5)), 15, "Cool")
        };
        
        mockRepo.Setup(r => r.GetForecastsAsync(5, default))
                .ReturnsAsync(testForecasts);
        
        var mockLogger = new Mock<ILogger<GetWeatherForecastQueryHandler>>();
        var handler = new GetWeatherForecastQueryHandler(mockRepo.Object, mockLogger.Object);
        var query = new GetWeatherForecastQuery(Days: 5);
        
        // Act
        var result = await handler.Handle(query, CancellationToken.None);
        
        // Assert
        Assert.NotNull(result);
        Assert.Equal(5, result.Count());
        mockRepo.Verify(r => r.GetForecastsAsync(5, default), Times.Once);
    }
}
```

## Migration from Monolithic to Layered

### Before (Monolithic)
```csharp
app.MapGet("/weatherforecast", () =>
{
    // Business logic mixed with API concerns
    var forecast = Enumerable.Range(1, 5)
        .Select(index => new WeatherForecast(
            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            Random.Shared.Next(-20, 55),
            summaries[Random.Shared.Next(summaries.Length)]
        ));
    return forecast;
});
```

### After (Layered Architecture)
```csharp
// API Layer - Clean and focused
app.MapGet("/weatherforecast", async (IMediator mediator) =>
{
    return await mediator.Send(new GetWeatherForecastQuery(Days: 5));
});

// Application Layer - Business logic
public class GetWeatherForecastQueryHandler : IRequestHandler<GetWeatherForecastQuery, IEnumerable<WeatherForecast>>
{
    private readonly IWeatherForecastRepository _repository;
    
    public async Task<IEnumerable<WeatherForecast>> Handle(...)
    {
        return await _repository.GetForecastsAsync(request.Days);
    }
}

// Infrastructure Layer - Data access
public class WeatherForecastRepository : IWeatherForecastRepository
{
    public Task<IEnumerable<WeatherForecast>> GetForecastsAsync(int days)
    {
        // Data access implementation
    }
}
```

## Benefits Achieved

### 1. Maintainability
- Clear separation of concerns
- Easy to locate and modify code
- Changes in one layer don't affect others

### 2. Testability
- Business logic isolated from infrastructure
- Easy to mock dependencies
- Comprehensive test coverage possible

### 3. Flexibility
- Easy to swap implementations (e.g., change data source)
- Can add new features without modifying existing code
- Different UI layers can share the same application core

### 4. Scalability
- CQRS enables separate scaling of read and write operations
- Repository pattern enables caching strategies
- Clean architecture supports microservices migration

### 5. Team Productivity
- Clear boundaries between layers
- Parallel development possible
- Reduced merge conflicts
- Easier onboarding for new developers

## Best Practices

### 1. Keep Domain Layer Pure
- No external dependencies
- Only business logic
- Framework-agnostic

### 2. Use Interfaces for Abstractions
- Define contracts in domain layer
- Implement in infrastructure layer
- Program to interfaces, not implementations

### 3. Follow SOLID Principles
- **S**ingle Responsibility Principle
- **O**pen/Closed Principle
- **L**iskov Substitution Principle
- **I**nterface Segregation Principle
- **D**ependency Inversion Principle

### 4. Keep Handlers Focused
- One handler per query/command
- Handlers should be thin coordinators
- Business logic in domain entities

### 5. Use Meaningful Names
- Queries end with "Query"
- Handlers end with "QueryHandler" or "CommandHandler"
- Repositories end with "Repository"

## Future Enhancements

### 1. Add Commands for Write Operations
When write operations are needed:
```csharp
public record CreateWeatherForecastCommand(DateOnly Date, int Temperature) : IRequest<Guid>;
```

### 2. Add Validation Pipeline
Use FluentValidation with MediatR pipeline:
```csharp
services.AddMediatR(cfg => {
    cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());
    cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
});
```

### 3. Add Caching Behavior
Implement caching as a cross-cutting concern:
```csharp
public class CachingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
{
    // Cache query results
}
```

### 4. Add Unit of Work Pattern
For coordinating multiple repository operations:
```csharp
public interface IUnitOfWork
{
    IWeatherForecastRepository WeatherForecasts { get; }
    IBananaAnalyticsRepository BananaAnalytics { get; }
    Task<int> SaveChangesAsync();
}
```

## References

- [Clean Architecture: A Craftsman's Guide to Software Structure and Design by Robert C. Martin](https://blog.cleancoder.com/uncle-bob/2012/08/13/the-clean-architecture.html)
- [CQRS Pattern by Martin Fowler](https://martinfowler.com/bliki/CQRS.html)
- [MediatR Documentation](https://github.com/jbogard/MediatR)
- [Repository Pattern by Martin Fowler](https://martinfowler.com/eaaCatalog/repository.html)
- [Dependency Inversion Principle](https://en.wikipedia.org/wiki/Dependency_inversion_principle)

## Document Control

| Version | Date | Author | Changes |
|---------|------|--------|---------|
| 1.0.0 | 2025-10-26 | System | Initial documentation of enterprise architecture patterns |

---

**Last Updated**: 2025-10-26  
**Next Review Date**: 2026-01-26  
**Document Owner**: Technical Lead  
**Approvers**: Architecture Review Board
