using System.Threading.RateLimiting;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.AspNetCore.Server.Kestrel.Core;

namespace GrafanaBanana.Api.Security;

/// <summary>
/// Extension methods for configuring security-related services and middleware.
/// </summary>
public static class SecurityExtensions
{
    /// <summary>
    /// Adds comprehensive security services to the application.
    /// Implements OWASP Top 10 and API security best practices.
    /// </summary>
    public static IServiceCollection AddSecurityServices(this IServiceCollection services, IConfiguration configuration)
    {
        // Add rate limiting to prevent DoS attacks (OWASP A05:2021 - Security Misconfiguration)
        services.AddRateLimiter(options =>
        {
            // Global rate limiter
            options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
            {
                var userIdentifier = context.Connection.RemoteIpAddress?.ToString() ?? "anonymous";
                
                return RateLimitPartition.GetFixedWindowLimiter(
                    partitionKey: userIdentifier,
                    factory: partition => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = 100, // 100 requests per window
                        Window = TimeSpan.FromMinutes(1),
                        QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                        QueueLimit = 10
                    });
            });

            // Specific policy for API endpoints
            options.AddFixedWindowLimiter("api", options =>
            {
                options.PermitLimit = 50;
                options.Window = TimeSpan.FromMinutes(1);
                options.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
                options.QueueLimit = 5;
            });

            // Strict policy for sensitive endpoints
            options.AddFixedWindowLimiter("strict", options =>
            {
                options.PermitLimit = 10;
                options.Window = TimeSpan.FromMinutes(1);
                options.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
                options.QueueLimit = 2;
            });

            // Handle rate limit exceeded
            options.OnRejected = async (context, token) =>
            {
                context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
                
                TimeSpan? retryAfter = null;
                if (context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfterValue))
                {
                    retryAfter = retryAfterValue;
                    context.HttpContext.Response.Headers.RetryAfter = retryAfterValue.TotalSeconds.ToString();
                }

                await context.HttpContext.Response.WriteAsJsonAsync(new
                {
                    error = "Too many requests. Please try again later.",
                    retryAfterSeconds = retryAfter?.TotalSeconds
                }, cancellationToken: token);
            };
        });

        // Add request size limits
        services.Configure<IISServerOptions>(options =>
        {
            options.MaxRequestBodySize = 10 * 1024 * 1024; // 10 MB limit
        });

        services.Configure<KestrelServerOptions>(options =>
        {
            options.Limits.MaxRequestBodySize = 10 * 1024 * 1024; // 10 MB limit
        });

        return services;
    }

    /// <summary>
    /// Configures security middleware in the HTTP request pipeline.
    /// Implements security headers and protections.
    /// </summary>
    public static IApplicationBuilder UseSecurityMiddleware(this IApplicationBuilder app, IWebHostEnvironment env)
    {
        // Add security headers (OWASP A05:2021 - Security Misconfiguration)
        app.Use(async (context, next) =>
        {
            // Prevent clickjacking attacks
            context.Response.Headers.Append("X-Frame-Options", "DENY");
            
            // Prevent MIME type sniffing
            context.Response.Headers.Append("X-Content-Type-Options", "nosniff");
            
            // Enable browser XSS protection
            context.Response.Headers.Append("X-XSS-Protection", "1; mode=block");
            
            // Referrer policy
            context.Response.Headers.Append("Referrer-Policy", "strict-origin-when-cross-origin");
            
            // Permissions policy
            context.Response.Headers.Append("Permissions-Policy", 
                "geolocation=(), microphone=(), camera=()");

            // Content Security Policy (CSP) - Adjust based on your needs
            if (!env.IsDevelopment())
            {
                context.Response.Headers.Append("Content-Security-Policy",
                    "default-src 'self'; " +
                    "script-src 'self'; " +
                    "style-src 'self' 'unsafe-inline'; " +
                    "img-src 'self' data: https:; " +
                    "font-src 'self'; " +
                    "connect-src 'self'; " +
                    "frame-ancestors 'none'; " +
                    "base-uri 'self'; " +
                    "form-action 'self'");
            }

            // HSTS - HTTP Strict Transport Security (force HTTPS)
            if (!env.IsDevelopment())
            {
                context.Response.Headers.Append("Strict-Transport-Security", 
                    "max-age=31536000; includeSubDomains; preload");
            }

            // Remove server header to avoid information disclosure
            context.Response.Headers.Remove("Server");
            context.Response.Headers.Remove("X-Powered-By");

            await next();
        });

        // Enable rate limiting
        app.UseRateLimiter();

        return app;
    }
}
