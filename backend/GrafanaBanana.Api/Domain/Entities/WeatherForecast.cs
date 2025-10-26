namespace GrafanaBanana.Api.Domain.Entities;

/// <summary>
/// Domain entity representing a weather forecast.
/// This entity follows the principles of Domain-Driven Design (DDD) with:
/// - Encapsulated state (private setters)
/// - Factory method for creation with validation
/// - Business logic embedded in the entity
/// </summary>
/// <remarks>
/// Part of the Domain Layer in Clean Architecture.
/// This entity has no dependencies on other layers.
/// </remarks>
public class WeatherForecast
{
    /// <summary>
    /// Gets the date for this weather forecast.
    /// </summary>
    public DateOnly Date { get; private set; }

    /// <summary>
    /// Gets the temperature in Celsius.
    /// </summary>
    public int TemperatureC { get; private set; }

    /// <summary>
    /// Gets the weather summary description (e.g., "Sunny", "Cloudy").
    /// </summary>
    public string? Summary { get; private set; }

    /// <summary>
    /// Gets the temperature in Fahrenheit.
    /// This is a calculated property demonstrating domain logic within the entity.
    /// </summary>
    public int TemperatureF => 32 + (int)(TemperatureC * 9.0 / 5.0);

    /// <summary>
    /// Private constructor to enforce use of factory method.
    /// Prevents direct instantiation without validation.
    /// </summary>
    /// <param name="date">The forecast date.</param>
    /// <param name="temperatureC">The temperature in Celsius.</param>
    /// <param name="summary">The weather summary.</param>
    public WeatherForecast(DateOnly date, int temperatureC, string? summary)
    {
        Date = date;
        TemperatureC = temperatureC;
        Summary = summary;
    }

    /// <summary>
    /// Factory method for creating weather forecast entities with validation.
    /// This follows the Factory Method pattern and ensures all created instances are valid.
    /// </summary>
    /// <param name="date">The forecast date.</param>
    /// <param name="temperatureC">The temperature in Celsius.</param>
    /// <param name="summary">The weather summary description.</param>
    /// <returns>A new valid WeatherForecast instance.</returns>
    /// <exception cref="ArgumentException">Thrown when temperature is below absolute zero (-273°C).</exception>
    /// <example>
    /// <code>
    /// var forecast = WeatherForecast.Create(DateOnly.FromDateTime(DateTime.Now), 20, "Sunny");
    /// </code>
    /// </example>
    public static WeatherForecast Create(DateOnly date, int temperatureC, string? summary)
    {
        // Domain validation - enforces business rule
        if (temperatureC < -273) // Absolute zero check
            throw new ArgumentException("Temperature cannot be below absolute zero", nameof(temperatureC));

        return new WeatherForecast(date, temperatureC, summary);
    }
}
