using GrafanaBanana.Api.Domain.Repositories;
using GrafanaBanana.Api.Infrastructure.Repositories;
using System.Reflection;

namespace GrafanaBanana.Api.Application;

/// <summary>
/// Extension methods for registering application layer services in the dependency injection container.
/// Implements the Dependency Injection pattern for loose coupling and better testability.
/// </summary>
/// <remarks>
/// This class centralizes all service registrations for the Application layer, following best practices:
/// 
/// - **Dependency Inversion Principle**: Register interfaces (contracts) with their implementations
/// - **Service Locator Pattern**: Extension method for IServiceCollection for clean registration
/// - **Single Responsibility**: One place to manage application service registration
/// 
/// The registration follows the Clean Architecture layers:
/// - Application Layer: Queries, Handlers (registered via MediatR)
/// - Domain Layer: Interfaces only (no registration needed)
/// - Infrastructure Layer: Repository implementations
/// </remarks>
public static class DependencyInjection
{
    /// <summary>
    /// Registers all application layer services including MediatR, repositories, and handlers.
    /// Following the Dependency Inversion Principle - depending on abstractions, not concretions.
    /// </summary>
    /// <param name="services">The IServiceCollection to add services to.</param>
    /// <returns>The IServiceCollection for chaining.</returns>
    /// <remarks>
    /// This method performs the following registrations:
    /// 
    /// 1. **MediatR Registration**: Enables CQRS pattern by scanning for all IRequest handlers
    ///    - Automatically finds and registers all query handlers
    ///    - Enables pipeline behaviors for cross-cutting concerns
    /// 
    /// 2. **Repository Registration**: Maps repository interfaces to implementations
    ///    - Scoped lifetime: One instance per HTTP request
    ///    - Follows Repository Pattern for data access abstraction
    /// 
    /// Service Lifetimes:
    /// - Scoped: Created once per request, disposed at end of request
    /// - Suitable for repositories that may hold resources
    /// </remarks>
    /// <example>
    /// Usage in Program.cs:
    /// <code>
    /// var builder = WebApplication.CreateBuilder(args);
    /// 
    /// // Register all application services
    /// builder.Services.AddApplicationServices();
    /// 
    /// // Now you can inject IMediator, IWeatherForecastRepository, etc.
    /// </code>
    /// </example>
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        // Register MediatR for CQRS pattern
        // Scans current assembly for all classes implementing IRequestHandler<,>
        // This automatically registers all query and command handlers
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));

        // Register repositories (Repository Pattern)
        // Maps interface (Domain layer) to implementation (Infrastructure layer)
        // Scoped lifetime: New instance per HTTP request
        services.AddScoped<IWeatherForecastRepository, WeatherForecastRepository>();
        services.AddScoped<IBananaAnalyticsRepository, BananaAnalyticsRepository>();

        return services;
    }
}
