namespace GrafanaBanana.Api.Domain.Entities;

/// <summary>
/// Domain entity representing a weather forecast
/// </summary>
public class WeatherForecast
{
    public DateOnly Date { get; private set; }
    public int TemperatureC { get; private set; }
    public string? Summary { get; private set; }

    // Domain logic
    public int TemperatureF => 32 + (int)(TemperatureC * 9.0 / 5.0);

    public WeatherForecast(DateOnly date, int temperatureC, string? summary)
    {
        Date = date;
        TemperatureC = temperatureC;
        Summary = summary;
    }

    // Factory method for creating forecasts
    public static WeatherForecast Create(DateOnly date, int temperatureC, string? summary)
    {
        // Domain validation
        if (temperatureC < -273) // Absolute zero check
            throw new ArgumentException("Temperature cannot be below absolute zero", nameof(temperatureC));

        return new WeatherForecast(date, temperatureC, summary);
    }
}
