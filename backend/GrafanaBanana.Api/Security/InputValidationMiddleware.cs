using System.Text.Json;

namespace GrafanaBanana.Api.Security;

/// <summary>
/// Middleware to validate and sanitize incoming requests.
/// Protects against injection attacks (OWASP A03:2021 - Injection).
/// </summary>
public class InputValidationMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<InputValidationMiddleware> _logger;
    private static readonly HashSet<string> DangerousPatterns = new(StringComparer.OrdinalIgnoreCase)
    {
        "<script", "javascript:", "onerror=", "onload=", "eval(", 
        "expression(", "vbscript:", "data:text/html", "../", "..\\",
        "';--", "\"; --", "' OR '1'='1", "\" OR \"1\"=\"1"
    };

    public InputValidationMiddleware(RequestDelegate next, ILogger<InputValidationMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Only validate requests with bodies
        if (context.Request.ContentLength > 0 && 
            context.Request.ContentType?.Contains("application/json", StringComparison.OrdinalIgnoreCase) == true)
        {
            context.Request.EnableBuffering();
            
            using var reader = new StreamReader(context.Request.Body, leaveOpen: true);
            var body = await reader.ReadToEndAsync();
            context.Request.Body.Position = 0;

            // Check for dangerous patterns
            if (ContainsDangerousContent(body))
            {
                _logger.LogWarning(
                    "Potentially malicious request detected from {RemoteIp}. Path: {Path}",
                    context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                    SanitizeForLogging(context.Request.Path.ToString()));

                context.Response.StatusCode = StatusCodes.Status400BadRequest;
                await context.Response.WriteAsJsonAsync(new
                {
                    error = "Invalid request content detected",
                    message = "The request contains potentially unsafe content"
                });
                return;
            }
        }

        // Validate query parameters
        foreach (var query in context.Request.Query)
        {
            if (ContainsDangerousContent(query.Value.ToString()))
            {
                _logger.LogWarning(
                    "Potentially malicious query parameter detected from {RemoteIp}. Parameter: {Parameter}",
                    context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                    SanitizeForLogging(query.Key));

                context.Response.StatusCode = StatusCodes.Status400BadRequest;
                await context.Response.WriteAsJsonAsync(new
                {
                    error = "Invalid query parameter detected",
                    message = "The query parameter contains potentially unsafe content"
                });
                return;
            }
        }

        await _next(context);
    }

    private static bool ContainsDangerousContent(string content)
    {
        if (string.IsNullOrWhiteSpace(content))
            return false;

        return DangerousPatterns.Any(pattern => 
            content.Contains(pattern, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Sanitizes a string for safe logging by removing or escaping potentially malicious content.
    /// Prevents log forging attacks.
    /// </summary>
    private static string SanitizeForLogging(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return string.Empty;

        // Remove newlines and carriage returns to prevent log forging
        var sanitized = value.Replace("\r", "").Replace("\n", " ");
        
        // Truncate long strings to prevent log pollution
        if (sanitized.Length > 200)
            sanitized = sanitized[..200] + "...";
        
        return sanitized;
    }
}

/// <summary>
/// Extension method to add input validation middleware to the pipeline.
/// </summary>
public static class InputValidationMiddlewareExtensions
{
    public static IApplicationBuilder UseInputValidation(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<InputValidationMiddleware>();
    }
}
