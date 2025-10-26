# GitHub Copilot Best Practices for Grafana-banana

This document provides specific guidance for using GitHub Copilot effectively within the Grafana-banana codebase, which implements enterprise architecture patterns.

## Overview

Grafana-banana uses:
- **Clean Architecture** with distinct layers (Domain, Application, Infrastructure, API)
- **CQRS Pattern** for separating read and write operations
- **Repository Pattern** for data access abstraction
- **Mediator Pattern** (MediatR) for decoupled request handling
- **Dependency Injection** throughout the application

## Quick Reference for Common Tasks

### Adding a New Query (Read Operation)

When you need to add a new read operation, follow this pattern:

1. **Create the Query** (Application/Queries/)
```csharp
/// <summary>
/// Query to retrieve [description of what you're getting].
/// Implements CQRS pattern for read operations.
/// </summary>
/// <param name="[ParamName]">[Description of parameter]</param>
public record Get[EntityName]Query([ParamType] [ParamName]) : IRequest<[ReturnType]>;
```

2. **Create the Handler** (Application/Handlers/)
```csharp
/// <summary>
/// Handler for Get[EntityName]Query.
/// Implements business logic for [description].
/// </summary>
public class Get[EntityName]QueryHandler : IRequestHandler<Get[EntityName]Query, [ReturnType]>
{
    private readonly I[EntityName]Repository _repository;
    private readonly ILogger<Get[EntityName]QueryHandler> _logger;

    public Get[EntityName]QueryHandler(
        I[EntityName]Repository repository,
        ILogger<Get[EntityName]QueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<[ReturnType]> Handle(Get[EntityName]Query request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Handling Get[EntityName]Query with {Param}", request.[ParamName]);
        
        var result = await _repository.Get[EntityName]Async(request.[ParamName], cancellationToken);
        
        _logger.LogInformation("Successfully retrieved [description]");
        
        return result;
    }
}
```

3. **Add API Endpoint** (Program.cs)
```csharp
app.MapGet("/api/[resource]", async (IMediator mediator, ILogger<Program> logger) =>
{
    logger.LogInformation("Processing [resource] request");
    
    try
    {
        var result = await mediator.Send(new Get[EntityName]Query([params]));
        return Results.Ok(result);
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Error fetching [resource]");
        return Results.Problem("Failed to fetch [resource]");
    }
})
.WithName("Get[EntityName]")
.WithOpenApi()
.RequireRateLimiting("api");
```

### Adding a New Command (Write Operation)

For write operations (create, update, delete), follow this pattern:

1. **Create the Command** (Application/Commands/)
```csharp
/// <summary>
/// Command to [action description].
/// Implements CQRS pattern for write operations.
/// </summary>
public record [Action][EntityName]Command([params]) : IRequest<[ReturnType]>;
```

2. **Create the Handler** (Application/Handlers/)
```csharp
/// <summary>
/// Handler for [Action][EntityName]Command.
/// </summary>
public class [Action][EntityName]CommandHandler : IRequestHandler<[Action][EntityName]Command, [ReturnType]>
{
    private readonly I[EntityName]Repository _repository;
    private readonly ILogger<[Action][EntityName]CommandHandler> _logger;

    public async Task<[ReturnType]> Handle([Action][EntityName]Command request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Handling [Action][EntityName]Command");
        
        // Validate input
        // Perform business logic
        // Save via repository
        
        return result;
    }
}
```

### Adding a New Domain Entity

1. **Create Entity** (Domain/Entities/)
```csharp
/// <summary>
/// Domain entity representing [description].
/// Follows Domain-Driven Design with encapsulated state and factory methods.
/// </summary>
public class [EntityName]
{
    /// <summary>
    /// Gets [description].
    /// </summary>
    public [Type] [PropertyName] { get; private set; }
    
    private [EntityName]([params])
    {
        // Initialize properties
    }
    
    /// <summary>
    /// Factory method for creating [EntityName] with validation.
    /// </summary>
    public static [EntityName] Create([params])
    {
        // Validate parameters
        // Apply business rules
        return new [EntityName]([params]);
    }
}
```

2. **Create Repository Interface** (Domain/Repositories/)
```csharp
/// <summary>
/// Repository interface for [EntityName] data access.
/// Implements Repository Pattern for abstraction of data access logic.
/// </summary>
public interface I[EntityName]Repository
{
    /// <summary>
    /// [Method description].
    /// </summary>
    Task<[ReturnType]> [MethodName]Async([params], CancellationToken cancellationToken = default);
}
```

3. **Implement Repository** (Infrastructure/Repositories/)
```csharp
/// <summary>
/// Repository implementation for [EntityName].
/// Part of Infrastructure layer - handles actual data access.
/// </summary>
public class [EntityName]Repository : I[EntityName]Repository
{
    private readonly ILogger<[EntityName]Repository> _logger;
    
    public [EntityName]Repository(ILogger<[EntityName]Repository> logger)
    {
        _logger = logger;
    }
    
    public async Task<[ReturnType]> [MethodName]Async([params], CancellationToken cancellationToken)
    {
        // Implementation
    }
}
```

4. **Register in DependencyInjection.cs**
```csharp
services.AddScoped<I[EntityName]Repository, [EntityName]Repository>();
```

## Copilot-Specific Tips

### Using Copilot for Pattern Generation

1. **Start with XML Comments**: Write a detailed XML comment, and Copilot will generate the implementation
```csharp
/// <summary>
/// Query to get banana production data by year.
/// Returns production statistics including total tons and regional breakdown.
/// </summary>
public record GetBananaProductionQuery(int Year) : IRequest<BananaProduction>;
// Copilot will understand the pattern and help complete
```

2. **Reference Existing Patterns**: When creating similar functionality, open existing files as context
   - Have `GetWeatherForecastQuery.cs` open when creating a new query
   - Copilot will match the pattern and naming conventions

3. **Use Descriptive Names**: Clear names help Copilot understand intent
   - ✅ `GetBananaProductionByYearQuery`
   - ❌ `Query1` or `BananaQuery`

### Common Patterns Copilot Should Follow

#### Logging Pattern
```csharp
_logger.LogInformation("Handling {QueryName} for {Parameter}", nameof(Query), parameter);
// ... operation ...
_logger.LogInformation("Successfully retrieved {Count} items", items.Count());
```

#### Error Handling Pattern
```csharp
try
{
    var result = await _repository.GetDataAsync(id, cancellationToken);
    return Results.Ok(result);
}
catch (NotFoundException ex)
{
    logger.LogWarning(ex, "Resource not found: {Id}", id);
    return Results.NotFound();
}
catch (Exception ex)
{
    logger.LogError(ex, "Error fetching resource: {Id}", id);
    return Results.Problem("Failed to fetch resource");
}
```

#### Async/Await Pattern
- Always use `async`/`await` for I/O operations
- Pass `CancellationToken` to all async methods
- Use `Task<T>` return types

#### Dependency Injection Pattern
```csharp
public class SomeHandler
{
    private readonly IRepository _repository;
    private readonly ILogger<SomeHandler> _logger;
    
    public SomeHandler(IRepository repository, ILogger<SomeHandler> logger)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }
}
```

## Architecture Principles for Copilot

### Layer Boundaries

**Domain Layer** (Domain/)
- ✅ Can contain: Entities, Value Objects, Domain Interfaces
- ❌ Cannot reference: Application, Infrastructure, API layers
- ❌ No dependencies on: EF Core, HTTP, external libraries

**Application Layer** (Application/)
- ✅ Can contain: Queries, Commands, Handlers, DTOs
- ✅ Can reference: Domain layer
- ❌ Cannot reference: Infrastructure, API (except for DI registration)

**Infrastructure Layer** (Infrastructure/)
- ✅ Can contain: Repository implementations, External service clients
- ✅ Can reference: Domain, Application layers
- ✅ Can use: Database libraries, HTTP clients, external SDKs

**API Layer** (Program.cs, Controllers)
- ✅ Can reference: Application layer
- ✅ Handles: HTTP concerns, routing, middleware
- ❌ Should not contain: Business logic

### SOLID Principles

When Copilot generates code, ensure it follows:

1. **Single Responsibility**: Each class has one reason to change
   - Handlers handle one query/command
   - Repositories manage one entity type

2. **Open/Closed**: Open for extension, closed for modification
   - Use interfaces for dependencies
   - Extend via new implementations, not modifying existing code

3. **Liskov Substitution**: Derived classes must be substitutable
   - Repository implementations must honor interface contracts

4. **Interface Segregation**: Many specific interfaces vs one general
   - Separate read and write repositories if needed

5. **Dependency Inversion**: Depend on abstractions
   - Program to interfaces (`IRepository`), not implementations

## Testing Patterns

### Unit Testing Handlers
```csharp
[Fact]
public async Task Handle_ValidRequest_ReturnsExpectedResult()
{
    // Arrange
    var mockRepository = new Mock<IWeatherForecastRepository>();
    mockRepository.Setup(r => r.GetForecastsAsync(5, default))
        .ReturnsAsync(new[] { WeatherForecast.Create(DateOnly.FromDateTime(DateTime.Now), 20, "Sunny") });
    
    var handler = new GetWeatherForecastQueryHandler(
        mockRepository.Object, 
        Mock.Of<ILogger<GetWeatherForecastQueryHandler>>());
    
    var query = new GetWeatherForecastQuery(Days: 5);
    
    // Act
    var result = await handler.Handle(query, default);
    
    // Assert
    Assert.NotNull(result);
    Assert.Single(result);
}
```

## Common Mistakes to Avoid

1. **Don't bypass the Mediator**: Always use `mediator.Send()`, not direct handler calls
   ```csharp
   // ❌ Wrong
   var handler = new GetWeatherForecastQueryHandler(repo, logger);
   var result = await handler.Handle(query);
   
   // ✅ Correct
   var result = await mediator.Send(query);
   ```

2. **Don't mix layers**: Keep business logic in handlers, not in API endpoints
   ```csharp
   // ❌ Wrong - Business logic in endpoint
   app.MapGet("/data", () => {
       var data = repository.GetAll();
       var filtered = data.Where(x => x.IsActive);
       return filtered;
   });
   
   // ✅ Correct - Business logic in handler
   app.MapGet("/data", (IMediator mediator) => 
       mediator.Send(new GetActiveDataQuery()));
   ```

3. **Don't forget observability**: Always add logging and metrics
   ```csharp
   // ❌ Missing logging
   var result = await _repository.GetDataAsync(id);
   return result;
   
   // ✅ Proper logging
   _logger.LogInformation("Fetching data for {Id}", id);
   var result = await _repository.GetDataAsync(id);
   _logger.LogInformation("Successfully retrieved data");
   return result;
   ```

## File Organization

```
GrafanaBanana.Api/
├── Domain/
│   ├── Entities/            # Domain entities with business logic
│   └── Repositories/        # Repository interfaces (contracts)
├── Application/
│   ├── Queries/            # Read operations (CQRS queries)
│   ├── Commands/           # Write operations (CQRS commands)  
│   ├── Handlers/           # Query and command handlers
│   └── DependencyInjection.cs
├── Infrastructure/
│   └── Repositories/       # Repository implementations
├── Security/               # Security middleware and services
├── Databricks/            # External service integrations
└── Program.cs             # API layer and startup
```

## Additional Resources

- [Enterprise Architecture Patterns Documentation](../docs/ENTERPRISE_ARCHITECTURE_PATTERNS.md)
- [ADR-0006: Enterprise Architecture Patterns](../docs/architecture/ADR-0006-enterprise-architecture-patterns.md)
- [Clean Architecture by Robert C. Martin](https://blog.cleancoder.com/uncle-bob/2012/08/13/the-clean-architecture.html)
- [CQRS Pattern](https://martinfowler.com/bliki/CQRS.html)
- [MediatR Documentation](https://github.com/jbogard/MediatR/wiki)

## Quick Commands

```bash
# Build backend
cd backend/GrafanaBanana.Api && dotnet build

# Run backend
cd backend/GrafanaBanana.Api && dotnet run

# Run tests
cd backend && dotnet test

# Start with Docker Compose
docker-compose up -d
```

---

**Remember**: When in doubt, look at existing implementations of similar features. The codebase is structured to be consistent and predictable, making it easier for Copilot to generate appropriate code.
