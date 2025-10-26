using GrafanaBanana.Api.Domain.Repositories;
using GrafanaBanana.Api.Infrastructure.Repositories;
using System.Reflection;

namespace GrafanaBanana.Api.Application;

/// <summary>
/// Extension methods for registering application layer services
/// Implements Dependency Injection pattern for loose coupling
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Registers all application layer services including MediatR, repositories, and handlers
    /// Following the Dependency Inversion Principle - depending on abstractions, not concretions
    /// </summary>
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        // Register MediatR for CQRS pattern
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));

        // Register repositories (Repository Pattern)
        services.AddScoped<IWeatherForecastRepository, WeatherForecastRepository>();
        services.AddScoped<IBananaAnalyticsRepository, BananaAnalyticsRepository>();

        return services;
    }
}
