# Enterprise Architecture - Visual Guide

## Layer Diagram

```
┌─────────────────────────────────────────────────────────────────┐
│                        API Layer                                 │
│                       (Program.cs)                               │
│  ┌───────────────────────────────────────────────────────────┐ │
│  │  HTTP Endpoints (Minimal APIs)                            │ │
│  │  - GET /weatherforecast                                   │ │
│  │  - GET /api/databricks/banana-analytics                   │ │
│  │  - GET /api/databricks/production/{year}                  │ │
│  │  - GET /api/databricks/sales                              │ │
│  └───────────────────────────────────────────────────────────┘ │
│                            ↓ uses                                │
│                       IMediator.Send()                           │
└─────────────────────────────────────────────────────────────────┘
                               ↓
┌─────────────────────────────────────────────────────────────────┐
│                    Application Layer                             │
│                  (Queries & Handlers)                            │
│  ┌───────────────────────────────────────────────────────────┐ │
│  │  Queries (Read Operations)                                │ │
│  │  - GetWeatherForecastQuery                                │ │
│  │  - GetBananaAnalyticsQuery                                │ │
│  │  - GetBananaProductionQuery                               │ │
│  │  - GetBananaSalesQuery                                    │ │
│  └───────────────────────────────────────────────────────────┘ │
│                            ↓                                     │
│  ┌───────────────────────────────────────────────────────────┐ │
│  │  Handlers (Business Logic)                                │ │
│  │  - GetWeatherForecastQueryHandler                         │ │
│  │  - GetBananaAnalyticsQueryHandler                         │ │
│  │  - GetBananaProductionQueryHandler                        │ │
│  │  - GetBananaSalesQueryHandler                             │ │
│  └───────────────────────────────────────────────────────────┘ │
│                            ↓ uses                                │
│                   Repository Interfaces                          │
└─────────────────────────────────────────────────────────────────┘
                               ↓
┌─────────────────────────────────────────────────────────────────┐
│                      Domain Layer                                │
│                  (Entities & Interfaces)                         │
│  ┌───────────────────────────────────────────────────────────┐ │
│  │  Entities (Business Objects)                              │ │
│  │  - WeatherForecast (with business logic)                  │ │
│  └───────────────────────────────────────────────────────────┘ │
│                            ↑                                     │
│  ┌───────────────────────────────────────────────────────────┐ │
│  │  Repository Interfaces (Contracts)                        │ │
│  │  - IWeatherForecastRepository                             │ │
│  │  - IBananaAnalyticsRepository                             │ │
│  └───────────────────────────────────────────────────────────┘ │
│                            ↑ implemented by                      │
└─────────────────────────────────────────────────────────────────┘
                               ↑
┌─────────────────────────────────────────────────────────────────┐
│                   Infrastructure Layer                           │
│                (Repository Implementations)                      │
│  ┌───────────────────────────────────────────────────────────┐ │
│  │  Repository Implementations                               │ │
│  │  - WeatherForecastRepository                              │ │
│  │  - BananaAnalyticsRepository (adapter)                    │ │
│  └───────────────────────────────────────────────────────────┘ │
│                            ↓ uses                                │
│  ┌───────────────────────────────────────────────────────────┐ │
│  │  External Services                                        │ │
│  │  - DatabricksService                                      │ │
│  │  - In-memory data generation                             │ │
│  └───────────────────────────────────────────────────────────┘ │
└─────────────────────────────────────────────────────────────────┘
```

## Request Flow Example

### GET /weatherforecast

```
1. Client Request
   ↓
2. API Endpoint (Program.cs)
   app.MapGet("/weatherforecast", async (IMediator mediator) => ...)
   ↓
3. MediatR Dispatcher
   await mediator.Send(new GetWeatherForecastQuery(Days: 5))
   ↓
4. Query Handler (Application Layer)
   GetWeatherForecastQueryHandler.Handle()
   ↓
5. Repository Interface Call (Domain Layer)
   IWeatherForecastRepository.GetForecastsAsync()
   ↓
6. Repository Implementation (Infrastructure Layer)
   WeatherForecastRepository.GetForecastsAsync()
   ↓
7. Domain Entity Creation
   WeatherForecast.Create(date, temp, summary)
   ↓
8. Return through layers
   Repository → Handler → MediatR → Endpoint → Client
```

## CQRS Pattern Illustration

```
┌─────────────────────────────────────┐
│           Client Request            │
└─────────────────┬───────────────────┘
                  │
         ┌────────┴────────┐
         │                 │
    ┌────▼────┐      ┌────▼────┐
    │ Command │      │  Query  │
    │ (Write) │      │  (Read) │
    └────┬────┘      └────┬────┘
         │                 │
    ┌────▼────┐      ┌────▼────┐
    │Command  │      │ Query   │
    │Handler  │      │Handler  │
    └────┬────┘      └────┬────┘
         │                 │
         └────────┬────────┘
                  │
         ┌────────▼────────┐
         │   Repository    │
         │   (Data Access) │
         └─────────────────┘
```

## Dependency Direction

```
API Layer
    ↓ depends on
Application Layer
    ↓ depends on
Domain Layer (Core - No Dependencies!)
    ↑ implemented by
Infrastructure Layer
```

**Key Principle**: Dependencies point INWARD
- API depends on Application
- Application depends on Domain
- Infrastructure implements Domain interfaces
- Domain has NO external dependencies

## Component Interaction

```
┌─────────────────────────────────────────────────────────────┐
│  API Layer                                                   │
│  ┌─────────────────────────────────────────────────────┐   │
│  │  Endpoint: GET /weatherforecast                     │   │
│  │  Responsibility: HTTP handling, routing             │   │
│  └─────────────────────────────────────────────────────┘   │
└───────────────────────┬─────────────────────────────────────┘
                        │ IMediator.Send()
┌───────────────────────▼─────────────────────────────────────┐
│  Application Layer                                           │
│  ┌─────────────────────────────────────────────────────┐   │
│  │  GetWeatherForecastQueryHandler                     │   │
│  │  Responsibility: Orchestration, business workflow   │   │
│  └─────────────────────────────────────────────────────┘   │
└───────────────────────┬─────────────────────────────────────┘
                        │ IWeatherForecastRepository
┌───────────────────────▼─────────────────────────────────────┐
│  Domain Layer                                                │
│  ┌─────────────────────────────────────────────────────┐   │
│  │  IWeatherForecastRepository (interface)             │   │
│  │  WeatherForecast (entity with business rules)       │   │
│  │  Responsibility: Core business logic                │   │
│  └─────────────────────────────────────────────────────┘   │
└───────────────────────▲─────────────────────────────────────┘
                        │ implements
┌───────────────────────┴─────────────────────────────────────┐
│  Infrastructure Layer                                        │
│  ┌─────────────────────────────────────────────────────┐   │
│  │  WeatherForecastRepository (implementation)         │   │
│  │  Responsibility: Data access, external systems      │   │
│  └─────────────────────────────────────────────────────┘   │
└─────────────────────────────────────────────────────────────┘
```

## Folder Structure

```
GrafanaBanana.Api/
│
├── Domain/                      # Core business layer
│   ├── Entities/
│   │   └── WeatherForecast.cs  # Business entities with logic
│   └── Repositories/
│       ├── IWeatherForecastRepository.cs     # Contracts
│       └── IBananaAnalyticsRepository.cs
│
├── Application/                 # Use cases layer
│   ├── Queries/
│   │   ├── GetWeatherForecastQuery.cs       # Read operations
│   │   ├── GetBananaAnalyticsQuery.cs
│   │   ├── GetBananaProductionQuery.cs
│   │   └── GetBananaSalesQuery.cs
│   ├── Handlers/
│   │   ├── GetWeatherForecastQueryHandler.cs # Business logic
│   │   ├── GetBananaAnalyticsQueryHandler.cs
│   │   ├── GetBananaProductionQueryHandler.cs
│   │   └── GetBananaSalesQueryHandler.cs
│   └── DependencyInjection.cs  # Service registration
│
├── Infrastructure/              # External concerns layer
│   └── Repositories/
│       ├── WeatherForecastRepository.cs      # Implementations
│       └── BananaAnalyticsRepository.cs
│
├── Security/                    # Cross-cutting concerns
│   ├── SecurityExtensions.cs
│   └── InputValidationMiddleware.cs
│
├── Databricks/                  # External service integration
│   ├── DatabricksService.cs
│   └── Models.cs
│
└── Program.cs                   # API layer / Entry point
```

## Testing Strategy by Layer

```
┌─────────────────────────────────────────────────────────────┐
│  Unit Tests                                                  │
│  ┌─────────────────────────────────────────────────────┐   │
│  │  Domain Layer Tests                                 │   │
│  │  - Test business logic in entities                  │   │
│  │  - Test factory methods                             │   │
│  │  - Test domain validations                          │   │
│  │  Dependencies: None (pure business logic)           │   │
│  └─────────────────────────────────────────────────────┘   │
│  ┌─────────────────────────────────────────────────────┐   │
│  │  Application Layer Tests                            │   │
│  │  - Test handlers with mocked repositories          │   │
│  │  - Test business workflows                          │   │
│  │  - Test error handling                              │   │
│  │  Dependencies: Mock repositories                    │   │
│  └─────────────────────────────────────────────────────┘   │
│  ┌─────────────────────────────────────────────────────┐   │
│  │  Infrastructure Layer Tests                         │   │
│  │  - Test repository implementations                  │   │
│  │  - Test data access logic                           │   │
│  │  Dependencies: In-memory or test database           │   │
│  └─────────────────────────────────────────────────────┘   │
└─────────────────────────────────────────────────────────────┘

┌─────────────────────────────────────────────────────────────┐
│  Integration Tests                                           │
│  ┌─────────────────────────────────────────────────────┐   │
│  │  End-to-End API Tests                               │   │
│  │  - Test complete request flow                       │   │
│  │  - Test MediatR pipeline                            │   │
│  │  - Test cross-cutting concerns                      │   │
│  │  Dependencies: TestServer, in-memory repositories   │   │
│  └─────────────────────────────────────────────────────┘   │
└─────────────────────────────────────────────────────────────┘
```

## Benefits Visualization

```
┌────────────────────────────────────────────────────────┐
│  Before: Monolithic Approach                           │
│  ┌──────────────────────────────────────────────────┐ │
│  │  Program.cs                                       │ │
│  │  ├─ Endpoint definition                          │ │
│  │  ├─ Business logic                               │ │
│  │  ├─ Data access                                  │ │
│  │  └─ Response formatting                          │ │
│  │                                                   │ │
│  │  Problems:                                        │ │
│  │  ❌ Tight coupling                                │ │
│  │  ❌ Hard to test                                  │ │
│  │  ❌ Difficult to maintain                         │ │
│  │  ❌ No clear boundaries                           │ │
│  └──────────────────────────────────────────────────┘ │
└────────────────────────────────────────────────────────┘
                         ↓ Refactored to
┌────────────────────────────────────────────────────────┐
│  After: Layered Architecture                           │
│  ┌──────────────────────────────────────────────────┐ │
│  │  API Layer: Program.cs                            │ │
│  │  └─ Endpoint definition only                     │ │
│  └──────────────────────────────────────────────────┘ │
│  ┌──────────────────────────────────────────────────┐ │
│  │  Application Layer: Handlers                      │ │
│  │  └─ Business workflow orchestration              │ │
│  └──────────────────────────────────────────────────┘ │
│  ┌──────────────────────────────────────────────────┐ │
│  │  Domain Layer: Entities & Interfaces              │ │
│  │  └─ Core business rules                          │ │
│  └──────────────────────────────────────────────────┘ │
│  ┌──────────────────────────────────────────────────┐ │
│  │  Infrastructure Layer: Repositories               │ │
│  │  └─ Data access implementation                   │ │
│  └──────────────────────────────────────────────────┘ │
│                                                        │
│  Benefits:                                             │
│  ✅ Loose coupling                                    │
│  ✅ Easy to test (mock repositories)                 │
│  ✅ Clear separation of concerns                     │
│  ✅ Maintainable and extensible                      │
│  ✅ Follows SOLID principles                         │
└────────────────────────────────────────────────────────┘
```

## Cross-Cutting Concerns

```
┌─────────────────────────────────────────────────────────────┐
│                  Middleware Pipeline                         │
│  ┌───────────────────────────────────────────────────────┐ │
│  │  Security Middleware                                  │ │
│  │  - Rate limiting                                      │ │
│  │  - Security headers                                   │ │
│  │  - Input validation                                   │ │
│  └───────────────────────────────────────────────────────┘ │
│  ┌───────────────────────────────────────────────────────┐ │
│  │  Observability Middleware                             │ │
│  │  - Request logging (Serilog)                          │ │
│  │  - Metrics collection (OpenTelemetry)                 │ │
│  │  - Distributed tracing (Tempo)                        │ │
│  └───────────────────────────────────────────────────────┘ │
│  ┌───────────────────────────────────────────────────────┐ │
│  │  CORS Middleware                                      │ │
│  │  - Cross-origin resource sharing                      │ │
│  └───────────────────────────────────────────────────────┘ │
│  ┌───────────────────────────────────────────────────────┐ │
│  │  Routing Middleware                                   │ │
│  │  - Endpoint routing                                   │ │
│  └───────────────────────────────────────────────────────┘ │
└─────────────────────────────────────────────────────────────┘
                           ↓
┌─────────────────────────────────────────────────────────────┐
│                     MediatR Pipeline                         │
│  ┌───────────────────────────────────────────────────────┐ │
│  │  Validation Behavior (Future)                         │ │
│  │  - FluentValidation                                   │ │
│  └───────────────────────────────────────────────────────┘ │
│  ┌───────────────────────────────────────────────────────┐ │
│  │  Logging Behavior                                     │ │
│  │  - Structured logging                                 │ │
│  └───────────────────────────────────────────────────────┘ │
│  ┌───────────────────────────────────────────────────────┐ │
│  │  Request Handler                                      │ │
│  │  - Business logic execution                           │ │
│  └───────────────────────────────────────────────────────┘ │
└─────────────────────────────────────────────────────────────┘
```

---

**Document Version**: 1.0  
**Last Updated**: 2025-10-26  
**For more details**: See [Enterprise Architecture Patterns](ENTERPRISE_ARCHITECTURE_PATTERNS.md)
