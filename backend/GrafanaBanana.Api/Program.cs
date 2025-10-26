using System.Diagnostics;
using System.Diagnostics.Metrics;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Serilog;
using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using GrafanaBanana.Api.Security;
using GrafanaBanana.Api.Databricks;

// Configure Serilog early
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(new ConfigurationBuilder()
        .SetBasePath(Directory.GetCurrentDirectory())
        .AddJsonFile("appsettings.json")
        .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production"}.json", true)
        .Build())
    .Enrich.FromLogContext()
    .Enrich.WithMachineName()
    .Enrich.WithThreadId()
    .CreateLogger();

try
{
    Log.Information("Starting Grafana-banana API application");

    var builder = WebApplication.CreateBuilder(args);

    // Replace default logging with Serilog
    builder.Host.UseSerilog();

    // Custom metrics for application monitoring
    var meter = new Meter("GrafanaBanana.Api", "1.0.0");
    var requestCounter = meter.CreateCounter<long>("api_requests_total", description: "Total number of API requests");
    var activeRequestsGauge = meter.CreateUpDownCounter<long>("api_requests_active", description: "Number of active API requests");
    var requestDuration = meter.CreateHistogram<double>("api_request_duration_ms", unit: "ms", description: "API request duration in milliseconds");
    var weatherForecastCounter = meter.CreateCounter<long>("weather_forecast_requests", description: "Number of weather forecast requests");
    var databricksQueryCounter = meter.CreateCounter<long>("databricks_queries_total", description: "Total number of Databricks queries");
    var databricksQueryDuration = meter.CreateHistogram<double>("databricks_query_duration_ms", unit: "ms", description: "Databricks query duration in milliseconds");

    // Add services to the container.
    builder.Services.AddOpenApi();

    // Add Databricks service
    builder.Services.Configure<DatabricksSettings>(builder.Configuration.GetSection("Databricks"));
    builder.Services.AddSingleton<IDatabricksService, DatabricksService>();

    // Add security services (rate limiting, input validation, etc.)
    builder.Services.AddSecurityServices(builder.Configuration);

    // Add CORS support
    builder.Services.AddCors(options =>
    {
        options.AddPolicy("AllowAngularApp",
            policy =>
            {
                policy.WithOrigins("http://localhost:4200", "http://localhost:5000")
                      .AllowAnyHeader()
                      .AllowAnyMethod();
            });

        options.AddPolicy("AllowAll",
            policy =>
            {
                policy.AllowAnyOrigin()
                      .AllowAnyHeader()
                      .AllowAnyMethod();
            });
    });

    // Add Health Checks
    builder.Services.AddHealthChecks()
        .AddCheck("self", () => HealthCheckResult.Healthy("API is running"))
        .AddCheck("weather_service", () => 
        {
            // Simulate checking weather service availability
            return HealthCheckResult.Healthy("Weather service is available");
        }, tags: new[] { "weather", "service" });

    // Configure OpenTelemetry
    var serviceName = builder.Configuration["OpenTelemetry:ServiceName"] ?? "grafana-banana-api";
    var serviceVersion = builder.Configuration["OpenTelemetry:ServiceVersion"] ?? "1.0.0";

    builder.Services.AddOpenTelemetry()
        .ConfigureResource(resource => resource
            .AddService(serviceName: serviceName, serviceVersion: serviceVersion)
            .AddAttributes(new Dictionary<string, object>
            {
                ["deployment.environment"] = builder.Environment.EnvironmentName,
                ["host.name"] = Environment.MachineName
            }))
        .WithTracing(tracing => tracing
            .AddAspNetCoreInstrumentation(options =>
            {
                options.RecordException = true;
                options.EnrichWithHttpRequest = (activity, httpRequest) =>
                {
                    activity.SetTag("http.request.user_agent", httpRequest.Headers.UserAgent.ToString());
                    activity.SetTag("http.request.content_length", httpRequest.ContentLength);
                };
                options.EnrichWithHttpResponse = (activity, httpResponse) =>
                {
                    activity.SetTag("http.response.content_length", httpResponse.ContentLength);
                };
            })
            .AddHttpClientInstrumentation(options =>
            {
                options.RecordException = true;
            })
            .AddSource(meter.Name)
            .SetSampler(new AlwaysOnSampler())
            .AddOtlpExporter(options =>
            {
                options.Endpoint = new Uri(builder.Configuration["OpenTelemetry:Otlp:Endpoint"] ?? "http://tempo:4317");
            }))
        .WithMetrics(metrics => metrics
            .AddAspNetCoreInstrumentation()
            .AddHttpClientInstrumentation()
            .AddRuntimeInstrumentation()
            .AddMeter(meter.Name)
            .AddPrometheusExporter());

    var app = builder.Build();

    // Configure the HTTP request pipeline.
    if (app.Environment.IsDevelopment())
    {
        app.MapOpenApi();
    }

    // Add security middleware (headers, rate limiting, etc.)
    app.UseSecurityMiddleware(app.Environment);

    // Add input validation middleware
    app.UseInputValidation();

    // Enable CORS
    app.UseCors("AllowAll");

    // Add Serilog request logging
    app.UseSerilogRequestLogging(options =>
    {
        options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
        {
            diagnosticContext.Set("RequestHost", httpContext.Request.Host.Value ?? "unknown");
            diagnosticContext.Set("RequestScheme", httpContext.Request.Scheme);
            var userAgent = httpContext.Request.Headers.UserAgent.ToString();
            diagnosticContext.Set("UserAgent", string.IsNullOrEmpty(userAgent) ? "unknown" : userAgent);
            diagnosticContext.Set("RemoteIP", httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown");
        };
    });

    // Expose Prometheus metrics endpoint
    app.MapPrometheusScrapingEndpoint();

    // Add health check endpoints
    app.MapHealthChecks("/health", new HealthCheckOptions
    {
        ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
    });

    app.MapHealthChecks("/health/ready", new HealthCheckOptions
    {
        Predicate = check => check.Tags.Contains("ready"),
        ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
    });

    app.MapHealthChecks("/health/live", new HealthCheckOptions
    {
        Predicate = _ => false,
        ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
    });

    var summaries = new[]
    {
        "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
    };

    app.MapGet("/weatherforecast", (ILogger<Program> logger) =>
    {
        var stopwatch = Stopwatch.StartNew();
        activeRequestsGauge.Add(1);
        requestCounter.Add(1, new KeyValuePair<string, object?>("endpoint", "/weatherforecast"));
        weatherForecastCounter.Add(1);

        using var activity = Activity.Current;
        activity?.SetTag("custom.endpoint", "weatherforecast");
        activity?.AddEvent(new ActivityEvent("Generating weather forecast"));

        logger.LogInformation("Generating weather forecast data");

        try
        {
            var forecast = Enumerable.Range(1, 5).Select(index =>
                new WeatherForecast
                (
                    DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                    Random.Shared.Next(-20, 55),
                    summaries[Random.Shared.Next(summaries.Length)]
                ))
                .ToArray();

            activity?.AddEvent(new ActivityEvent("Weather forecast generated successfully"));
            logger.LogInformation("Successfully generated {Count} weather forecast entries", forecast.Length);

            return forecast;
        }
        finally
        {
            stopwatch.Stop();
            activeRequestsGauge.Add(-1);
            requestDuration.Record(stopwatch.ElapsedMilliseconds, 
                new KeyValuePair<string, object?>("endpoint", "/weatherforecast"),
                new KeyValuePair<string, object?>("status", "success"));
        }
    })
    .WithName("GetWeatherForecast")
    .WithOpenApi()
    .RequireRateLimiting("api");

    // Add a metrics endpoint for custom application metrics
    app.MapGet("/api/metrics/custom", (ILogger<Program> logger) =>
    {
        logger.LogInformation("Custom metrics endpoint called");
        
        return new
        {
            service = serviceName,
            version = serviceVersion,
            environment = builder.Environment.EnvironmentName,
            uptime = DateTime.UtcNow,
            metrics = new
            {
                requestsTotal = "Available at /metrics",
                activeRequests = "Available at /metrics",
                requestDuration = "Available at /metrics"
            }
        };
    })
    .WithName("GetCustomMetrics")
    .WithOpenApi();

    // Add an endpoint to test tracing
    app.MapGet("/api/trace/test", async (ILogger<Program> logger) =>
    {
        using var activity = Activity.Current;
        activity?.SetTag("test.endpoint", "trace-test");
        
        logger.LogInformation("Testing distributed tracing");
        
        activity?.AddEvent(new ActivityEvent("Starting trace test"));
        
        // Simulate some work
        await Task.Delay(Random.Shared.Next(10, 100));
        
        activity?.AddEvent(new ActivityEvent("Trace test completed"));
        
        return new
        {
            message = "Tracing test completed",
            traceId = activity?.TraceId.ToString(),
            spanId = activity?.SpanId.ToString()
        };
    })
    .WithName("TestTracing")
    .WithOpenApi();

    // Add endpoint to test errors and exception tracking
    app.MapGet("/api/error/test", (ILogger<Program> logger) =>
    {
        logger.LogWarning("Testing error tracking - this is intentional");
        throw new InvalidOperationException("This is a test exception for observability testing");
    })
    .WithName("TestError")
    .WithOpenApi();

    // Databricks Banana Analytics Endpoints
    app.MapGet("/api/databricks/banana-analytics", async (
        IDatabricksService databricksService,
        ILogger<Program> logger) =>
    {
        var stopwatch = Stopwatch.StartNew();
        activeRequestsGauge.Add(1);
        requestCounter.Add(1, new KeyValuePair<string, object?>("endpoint", "/api/databricks/banana-analytics"));
        databricksQueryCounter.Add(1, new KeyValuePair<string, object?>("query_type", "full_analytics"));

        using var activity = Activity.Current;
        activity?.SetTag("databricks.query_type", "full_analytics");
        activity?.AddEvent(new ActivityEvent("Fetching banana analytics from Databricks"));

        logger.LogInformation("Fetching banana analytics data from Databricks");

        try
        {
            var analytics = await databricksService.GetBananaAnalyticsAsync();
            
            activity?.AddEvent(new ActivityEvent("Successfully retrieved banana analytics"));
            logger.LogInformation(
                "Retrieved banana analytics: {ProductionTons} tons produced, {Revenue} revenue, {Countries} countries served",
                analytics.Summary.TotalProductionTons,
                analytics.Summary.TotalRevenue,
                analytics.Summary.CountriesServed);

            return Results.Ok(analytics);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error fetching banana analytics from Databricks");
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            return Results.Problem("Failed to fetch banana analytics");
        }
        finally
        {
            stopwatch.Stop();
            activeRequestsGauge.Add(-1);
            databricksQueryDuration.Record(stopwatch.ElapsedMilliseconds,
                new KeyValuePair<string, object?>("query_type", "full_analytics"));
            requestDuration.Record(stopwatch.ElapsedMilliseconds,
                new KeyValuePair<string, object?>("endpoint", "/api/databricks/banana-analytics"));
        }
    })
    .WithName("GetBananaAnalytics")
    .WithOpenApi()
    .RequireRateLimiting("api");

    app.MapGet("/api/databricks/production/{year:int}", async (
        int year,
        IDatabricksService databricksService,
        ILogger<Program> logger) =>
    {
        var stopwatch = Stopwatch.StartNew();
        databricksQueryCounter.Add(1, new KeyValuePair<string, object?>("query_type", "production"));

        using var activity = Activity.Current;
        activity?.SetTag("databricks.query_type", "production");
        activity?.SetTag("databricks.query_year", year);

        logger.LogInformation("Fetching banana production data for year {Year}", year);

        try
        {
            var production = await databricksService.GetProductionDataAsync(year);
            logger.LogInformation("Retrieved {Count} production records for year {Year}", production.Count, year);
            return Results.Ok(production);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error fetching production data for year {Year}", year);
            return Results.Problem($"Failed to fetch production data for year {year}");
        }
        finally
        {
            stopwatch.Stop();
            databricksQueryDuration.Record(stopwatch.ElapsedMilliseconds,
                new KeyValuePair<string, object?>("query_type", "production"));
        }
    })
    .WithName("GetBananaProduction")
    .WithOpenApi()
    .RequireRateLimiting("api");

    app.MapGet("/api/databricks/sales", async (
        string? region,
        IDatabricksService databricksService,
        ILogger<Program> logger) =>
    {
        var stopwatch = Stopwatch.StartNew();
        databricksQueryCounter.Add(1, new KeyValuePair<string, object?>("query_type", "sales"));

        using var activity = Activity.Current;
        activity?.SetTag("databricks.query_type", "sales");
        activity?.SetTag("databricks.query_region", region ?? "all");

        // Sanitize region parameter for logging to prevent log forging
        var sanitizedRegion = (region ?? "all").Replace("\n", "").Replace("\r", "");
        logger.LogInformation("Fetching banana sales data for region {Region}", sanitizedRegion);

        try
        {
            var sales = await databricksService.GetSalesDataAsync(region ?? "Global");
            logger.LogInformation("Retrieved {Count} sales records", sales.Count);
            return Results.Ok(sales);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error fetching sales data");
            return Results.Problem("Failed to fetch sales data");
        }
        finally
        {
            stopwatch.Stop();
            databricksQueryDuration.Record(stopwatch.ElapsedMilliseconds,
                new KeyValuePair<string, object?>("query_type", "sales"));
        }
    })
    .WithName("GetBananaSales")
    .WithOpenApi()
    .RequireRateLimiting("api");

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC * 9.0 / 5.0);
}

