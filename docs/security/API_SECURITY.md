# API Security Best Practices

This guide provides comprehensive security best practices for developing and maintaining secure APIs in Grafana-banana, based on OWASP API Security Top 10 and industry standards.

## Table of Contents
1. [Authentication & Authorization](#authentication--authorization)
2. [Input Validation](#input-validation)
3. [Rate Limiting & DoS Protection](#rate-limiting--dos-protection)
4. [Data Security](#data-security)
5. [Error Handling](#error-handling)
6. [API Versioning](#api-versioning)
7. [Security Headers](#security-headers)
8. [Logging & Monitoring](#logging--monitoring)
9. [Testing](#testing)
10. [Deployment Security](#deployment-security)

---

## Authentication & Authorization

### Current Implementation
- Rate limiting configured to prevent brute force attacks
- Framework ready for JWT/OAuth2 implementation
- CORS policies restrict cross-origin access

### Best Practices

#### 1. Use Strong Authentication
```csharp
// Implement JWT authentication
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = configuration["Jwt:Issuer"],
            ValidAudience = configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(configuration["Jwt:Key"]))
        };
    });
```

#### 2. Implement Authorization Policies
```csharp
// Define policies
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy => 
        policy.RequireRole("Admin"));
    
    options.AddPolicy("ReadAccess", policy => 
        policy.RequireClaim("permission", "read"));
});

// Apply to endpoints
app.MapGet("/api/admin/users", () => GetUsers())
    .RequireAuthorization("AdminOnly");
```

#### 3. API Key Authentication (for service-to-service)
```csharp
// Simple API key middleware
app.Use(async (context, next) =>
{
    if (context.Request.Path.StartsWithSegments("/api"))
    {
        if (!context.Request.Headers.TryGetValue("X-API-Key", out var apiKey) ||
            !IsValidApiKey(apiKey))
        {
            context.Response.StatusCode = 401;
            await context.Response.WriteAsJsonAsync(new { error = "Invalid API key" });
            return;
        }
    }
    await next();
});
```

### Security Checklist
- [ ] Never transmit credentials in URL parameters
- [ ] Use HTTPS for all authentication endpoints
- [ ] Implement token expiration and refresh
- [ ] Store tokens securely (HttpOnly cookies or secure storage)
- [ ] Implement multi-factor authentication for sensitive operations
- [ ] Use standard authentication protocols (OAuth2, OpenID Connect)

---

## Input Validation

### Current Implementation
- Input validation middleware detects malicious patterns
- Request body and query parameter sanitization
- Request size limits (10MB)

### Best Practices

#### 1. Model Validation
```csharp
// Use data annotations
public record CreateProductRequest(
    [Required, StringLength(100, MinimumLength = 3)]
    string Name,
    
    [Required, Range(0.01, 999999.99)]
    decimal Price,
    
    [MaxLength(500)]
    string? Description,
    
    [Required, RegularExpression(@"^[A-Z]{2,3}-\d{4}$")]
    string Sku
);

// Endpoint with automatic validation
app.MapPost("/api/products", (CreateProductRequest request) =>
{
    // Request is automatically validated
    return Results.Created($"/api/products/{id}", product);
})
.WithOpenApi();
```

#### 2. Custom Validation
```csharp
public class SafeStringAttribute : ValidationAttribute
{
    protected override ValidationResult IsValid(object value, ValidationContext context)
    {
        if (value is string str && ContainsDangerousContent(str))
        {
            return new ValidationResult("Input contains potentially unsafe content");
        }
        return ValidationResult.Success;
    }
    
    private bool ContainsDangerousContent(string input)
    {
        var dangerousPatterns = new[] { "<script", "javascript:", "onerror=" };
        return dangerousPatterns.Any(p => 
            input.Contains(p, StringComparison.OrdinalIgnoreCase));
    }
}

// Usage
public record CommentRequest(
    [Required, SafeString, MaxLength(1000)]
    string Text
);
```

#### 3. Query Parameter Validation
```csharp
app.MapGet("/api/products", (
    [FromQuery, Range(1, 100)] int pageSize = 10,
    [FromQuery, Range(1, int.MaxValue)] int page = 1,
    [FromQuery, StringLength(50)] string? search = null) =>
{
    // Parameters are validated
    return GetProducts(pageSize, page, search);
});
```

### Validation Checklist
- [ ] Validate all input (body, query params, headers, path params)
- [ ] Whitelist acceptable input, don't just blacklist bad input
- [ ] Validate data types, formats, ranges, and lengths
- [ ] Sanitize input to prevent injection attacks
- [ ] Return clear validation error messages (but don't expose internals)
- [ ] Validate on the server side, never trust client validation alone

---

## Rate Limiting & DoS Protection

### Current Implementation
- Global rate limiter (100 requests/minute per IP)
- API-specific rate limiter (50 requests/minute)
- Strict rate limiter for sensitive endpoints (10 requests/minute)

### Best Practices

#### 1. Endpoint-Specific Rate Limiting
```csharp
// Apply different limits to different endpoints
app.MapPost("/api/login", LoginHandler)
    .RequireRateLimiting("strict");  // 10 req/min

app.MapGet("/api/products", GetProducts)
    .RequireRateLimiting("api");     // 50 req/min

app.MapGet("/health", HealthCheck)
    .DisableRateLimiting();          // No limit
```

#### 2. User-Specific Rate Limiting
```csharp
services.AddRateLimiter(options =>
{
    options.AddPolicy("per-user", context =>
    {
        var userId = context.User.Identity?.Name ?? 
                     context.Connection.RemoteIpAddress?.ToString() ?? 
                     "anonymous";
        
        return RateLimitPartition.GetTokenBucketLimiter(userId, _ =>
            new TokenBucketRateLimiterOptions
            {
                TokenLimit = 100,
                ReplenishmentPeriod = TimeSpan.FromMinutes(1),
                TokensPerPeriod = 20,
                AutoReplenishment = true
            });
    });
});
```

#### 3. Request Size Limits
```csharp
// Already implemented in SecurityExtensions.cs
builder.Services.Configure<KestrelServerOptions>(options =>
{
    options.Limits.MaxRequestBodySize = 10 * 1024 * 1024; // 10 MB
    options.Limits.MaxRequestHeaderCount = 100;
    options.Limits.MaxRequestHeadersTotalSize = 32 * 1024; // 32 KB
});
```

### DoS Protection Checklist
- [ ] Implement rate limiting on all public endpoints
- [ ] Set appropriate request size limits
- [ ] Implement connection limits
- [ ] Use timeouts for long-running operations
- [ ] Monitor for unusual traffic patterns
- [ ] Consider using a CDN or API gateway for additional protection

---

## Data Security

### Best Practices

#### 1. Data Classification
```csharp
// Mark sensitive data
public class User
{
    public int Id { get; set; }
    public string Username { get; set; }
    
    [PersonalData] // Custom attribute for PII
    public string Email { get; set; }
    
    [Sensitive]    // Never log or expose
    public string PasswordHash { get; set; }
}
```

#### 2. Response Filtering
```csharp
// Don't expose internal IDs or sensitive data
public record UserResponse(
    string Username,
    string Email,
    DateTime CreatedAt
)
{
    // No PasswordHash, internal IDs, or sensitive fields
}

app.MapGet("/api/users/{id}", (int id) =>
{
    var user = GetUser(id);
    return new UserResponse(user.Username, user.Email, user.CreatedAt);
});
```

#### 3. Encryption for Sensitive Data
```csharp
// Use data protection for sensitive data
public class SecureDataService
{
    private readonly IDataProtector _protector;
    
    public SecureDataService(IDataProtectionProvider provider)
    {
        _protector = provider.CreateProtector("SecureData");
    }
    
    public string Encrypt(string data) => _protector.Protect(data);
    public string Decrypt(string encrypted) => _protector.Unprotect(encrypted);
}
```

### Data Security Checklist
- [ ] Classify data by sensitivity level
- [ ] Encrypt sensitive data at rest and in transit
- [ ] Never expose more data than necessary in responses
- [ ] Use HTTPS for all API communication
- [ ] Implement proper data retention policies
- [ ] Sanitize data before logging

---

## Error Handling

### Current Implementation
- Structured error responses
- No stack traces in production
- Security event logging for suspicious requests

### Best Practices

#### 1. Consistent Error Responses
```csharp
public record ErrorResponse(
    string Error,
    string Message,
    int StatusCode,
    string? TraceId = null
);

app.UseExceptionHandler(errorApp =>
{
    errorApp.Run(async context =>
    {
        var exception = context.Features.Get<IExceptionHandlerFeature>()?.Error;
        
        var statusCode = exception switch
        {
            NotFoundException => 404,
            UnauthorizedException => 401,
            ValidationException => 400,
            _ => 500
        };
        
        // Log the full exception
        logger.LogError(exception, "Unhandled exception");
        
        // Return safe error message
        context.Response.StatusCode = statusCode;
        await context.Response.WriteAsJsonAsync(new ErrorResponse(
            Error: exception.GetType().Name,
            Message: context.RequestServices.GetService<IWebHostEnvironment>()
                .IsDevelopment() 
                ? exception.Message 
                : "An error occurred processing your request",
            StatusCode: statusCode,
            TraceId: Activity.Current?.Id
        ));
    });
});
```

#### 2. Validation Error Responses
```csharp
app.MapPost("/api/products", async (CreateProductRequest request) =>
{
    var errors = ValidateRequest(request);
    if (errors.Any())
    {
        return Results.BadRequest(new
        {
            error = "Validation failed",
            errors = errors.Select(e => new { field = e.Field, message = e.Message })
        });
    }
    
    // Process request
});
```

### Error Handling Checklist
- [ ] Never expose stack traces in production
- [ ] Use consistent error response format
- [ ] Return appropriate HTTP status codes
- [ ] Log all errors with sufficient context
- [ ] Don't reveal sensitive information in error messages
- [ ] Include correlation IDs for tracing

---

## API Versioning

### Best Practices

#### 1. URL Path Versioning
```csharp
// Version in URL path
app.MapGet("/api/v1/products", GetProductsV1);
app.MapGet("/api/v2/products", GetProductsV2);
```

#### 2. Header Versioning
```csharp
app.Use(async (context, next) =>
{
    var apiVersion = context.Request.Headers["X-API-Version"].FirstOrDefault() ?? "1";
    context.Items["ApiVersion"] = apiVersion;
    await next();
});
```

#### 3. Using ASP.NET API Versioning
```csharp
builder.Services.AddApiVersioning(options =>
{
    options.DefaultApiVersion = new ApiVersion(1, 0);
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.ReportApiVersions = true;
});
```

### Versioning Checklist
- [ ] Version your API from the start
- [ ] Maintain backward compatibility when possible
- [ ] Clearly document version changes
- [ ] Deprecate old versions gracefully
- [ ] Support at least N-1 versions

---

## Security Headers

### Current Implementation
- X-Frame-Options: DENY
- X-Content-Type-Options: nosniff
- X-XSS-Protection: 1; mode=block
- Referrer-Policy: strict-origin-when-cross-origin
- Content-Security-Policy (production)
- HSTS (production)

### All Security Headers Configured
```
X-Frame-Options: DENY
X-Content-Type-Options: nosniff
X-XSS-Protection: 1; mode=block
Referrer-Policy: strict-origin-when-cross-origin
Permissions-Policy: geolocation=(), microphone=(), camera=()
Content-Security-Policy: [configured per environment]
Strict-Transport-Security: max-age=31536000; includeSubDomains; preload
```

### Headers Checklist
- [ ] All security headers configured
- [ ] HSTS enabled in production
- [ ] CSP configured appropriately
- [ ] Server identification headers removed
- [ ] CORS properly configured

---

## Logging & Monitoring

### Current Implementation
- Structured logging with Serilog
- OpenTelemetry tracing
- Prometheus metrics
- Security event logging

### What to Log
```csharp
// Authentication events
_logger.LogInformation("User {UserId} authenticated successfully from {IpAddress}", 
    userId, ipAddress);

// Authorization failures
_logger.LogWarning("Unauthorized access attempt to {Resource} by {UserId} from {IpAddress}",
    resource, userId, ipAddress);

// Input validation failures
_logger.LogWarning("Invalid input detected: {ValidationErrors} from {IpAddress}",
    validationErrors, ipAddress);

// Rate limit violations
_logger.LogWarning("Rate limit exceeded for {IpAddress} on endpoint {Endpoint}",
    ipAddress, endpoint);
```

### Logging Checklist
- [ ] Log all security-relevant events
- [ ] Never log sensitive data
- [ ] Include correlation IDs
- [ ] Set up alerts for suspicious patterns
- [ ] Regularly review logs
- [ ] Ensure log integrity and retention

---

## Testing

### Security Test Checklist

```csharp
// Example security tests
[Fact]
public async Task RateLimiting_ExceedsLimit_Returns429()
{
    // Arrange
    var client = _factory.CreateClient();
    
    // Act - Make requests until rate limit exceeded
    var tasks = Enumerable.Range(0, 101)
        .Select(_ => client.GetAsync("/api/products"));
    var responses = await Task.WhenAll(tasks);
    
    // Assert
    Assert.Contains(responses, r => r.StatusCode == HttpStatusCode.TooManyRequests);
}

[Theory]
[InlineData("<script>alert('xss')</script>")]
[InlineData("'; DROP TABLE Users; --")]
[InlineData("../../../etc/passwd")]
public async Task InputValidation_MaliciousInput_ReturnsBadRequest(string maliciousInput)
{
    // Arrange & Act
    var response = await client.PostAsJsonAsync("/api/comments", 
        new { text = maliciousInput });
    
    // Assert
    Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
}

[Fact]
public async Task Authentication_InvalidToken_Returns401()
{
    // Test authentication
}

[Fact]
public async Task Authorization_InsufficientPermissions_Returns403()
{
    // Test authorization
}
```

### Testing Checklist
- [ ] Test authentication and authorization
- [ ] Test input validation with malicious input
- [ ] Test rate limiting
- [ ] Test error handling
- [ ] Test CORS policies
- [ ] Perform security scanning (CodeQL, SAST)

---

## Deployment Security

### Production Checklist
- [ ] HTTPS enforced
- [ ] Environment variables for secrets
- [ ] Security headers enabled
- [ ] Rate limiting active
- [ ] Logging and monitoring configured
- [ ] Docker containers run as non-root
- [ ] Security scanning in CI/CD
- [ ] Regular dependency updates
- [ ] Backup and disaster recovery plan

### Environment Configuration
```bash
# Production environment variables
ASPNETCORE_ENVIRONMENT=Production
ASPNETCORE_URLS=https://+:443;http://+:80
JWT_SECRET=<secure-secret-from-key-vault>
DATABASE_CONNECTION=<encrypted-connection-string>
```

---

## Additional Resources

- [OWASP API Security Top 10](https://owasp.org/www-project-api-security/)
- [OWASP Cheat Sheet Series](https://cheatsheetseries.owasp.org/)
- [Microsoft API Guidelines](https://github.com/microsoft/api-guidelines)
- [REST API Security Best Practices](https://restfulapi.net/security-essentials/)

---

## Quick Reference

### Secure Endpoint Template
```csharp
app.MapPost("/api/resource", async (
    [FromBody] CreateResourceRequest request,
    [FromServices] IResourceService service,
    [FromServices] ILogger<Program> logger) =>
{
    // 1. Input validation (automatic with data annotations)
    
    // 2. Authorization check
    // Implement when authentication is added
    
    // 3. Business logic with error handling
    try
    {
        var result = await service.CreateResource(request);
        
        // 4. Success logging
        logger.LogInformation("Resource created: {ResourceId}", result.Id);
        
        // 5. Return appropriate response
        return Results.Created($"/api/resource/{result.Id}", result);
    }
    catch (ValidationException ex)
    {
        logger.LogWarning("Validation failed: {Errors}", ex.Errors);
        return Results.BadRequest(new { error = ex.Message, errors = ex.Errors });
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Error creating resource");
        return Results.Problem("An error occurred");
    }
})
.WithName("CreateResource")
.WithOpenApi()
.RequireRateLimiting("api")
.Produces<ResourceResponse>(201)
.Produces<ErrorResponse>(400)
.Produces<ErrorResponse>(500);
```

This template includes all security best practices:
- ✅ Input validation
- ✅ Rate limiting
- ✅ Error handling
- ✅ Logging
- ✅ Proper HTTP status codes
- ✅ OpenAPI documentation
