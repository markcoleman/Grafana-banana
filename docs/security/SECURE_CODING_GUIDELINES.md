# Secure Coding Guidelines for Developers

This document provides practical secure coding guidelines for all developers contributing to Grafana-banana. Following these guidelines will help prevent security vulnerabilities and maintain a secure codebase.

## Table of Contents
1. [General Principles](#general-principles)
2. [Backend (.NET) Guidelines](#backend-net-guidelines)
3. [Frontend (Angular) Guidelines](#frontend-angular-guidelines)
4. [Database Security](#database-security)
5. [Docker & Infrastructure](#docker--infrastructure)
6. [Code Review Checklist](#code-review-checklist)

---

## General Principles

### Defense in Depth
Implement multiple layers of security controls. If one layer fails, others will still protect the system.

### Principle of Least Privilege
Grant only the minimum access rights necessary to perform a task.

### Fail Securely
When errors occur, the system should fail in a secure state, not an open/vulnerable state.

### Never Trust Input
Validate and sanitize all input from any source (users, APIs, files, databases).

### Don't Security Through Obscurity
Security should not rely on keeping implementation details secret. Use proven security mechanisms.

---

## Backend (.NET) Guidelines

### 1. Input Validation

#### ✅ DO
```csharp
// Use data annotations for validation
public record CreateUserRequest(
    [Required, StringLength(100, MinimumLength = 3)]
    string Username,
    
    [Required, EmailAddress]
    string Email,
    
    [Required, MinLength(8)]
    string Password
);

// Validate and sanitize input
public string SanitizeInput(string input)
{
    if (string.IsNullOrWhiteSpace(input))
        return string.Empty;
    
    // Remove dangerous characters
    return input.Replace("<", "&lt;").Replace(">", "&gt;");
}
```

#### ❌ DON'T
```csharp
// Don't trust input without validation
app.MapGet("/api/user/{id}", (string id) =>
{
    return GetUser(id); // Dangerous! id could be malicious
});

// Don't use string concatenation for queries
var query = $"SELECT * FROM Users WHERE Username = '{username}'"; // SQL Injection!
```

### 2. Authentication & Authorization

#### ✅ DO
```csharp
// Use framework authentication
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options => { /* configure */ });

// Always check authorization
app.MapGet("/api/admin/data", () => GetData())
    .RequireAuthorization("AdminPolicy");

// Use secure password hashing
using var hash = new Rfc2898DeriveBytes(password, salt, iterations: 100000);
```

#### ❌ DON'T
```csharp
// Don't implement custom crypto
string hashedPassword = ComputeMD5Hash(password); // Insecure!

// Don't store passwords in plain text
user.Password = password; // Never!

// Don't check authorization manually
if (user.Role == "admin") // Fragile and error-prone
```

### 3. Error Handling

#### ✅ DO
```csharp
// Use structured error handling
try
{
    return await ProcessRequest(request);
}
catch (ValidationException ex)
{
    logger.LogWarning("Validation failed: {Message}", ex.Message);
    return Results.BadRequest(new { error = "Invalid input" });
}
catch (Exception ex)
{
    logger.LogError(ex, "Unexpected error");
    return Results.Problem("An error occurred"); // Generic message
}
```

#### ❌ DON'T
```csharp
// Don't expose internal details
catch (Exception ex)
{
    return Results.BadRequest(ex.ToString()); // Exposes stack trace!
}

// Don't swallow exceptions
catch (Exception) { } // Silent failure!
```

### 4. Logging

#### ✅ DO
```csharp
// Log security events
_logger.LogWarning("Failed login attempt for user {Username} from {IpAddress}", 
    username, ipAddress);

// Use structured logging
_logger.LogInformation("Order {OrderId} created by {UserId}", orderId, userId);
```

#### ❌ DON'T
```csharp
// Don't log sensitive data
_logger.LogInformation("User logged in with password {Password}", password); // Never!
_logger.LogInformation("Processing credit card {CardNumber}", cardNumber); // Never!

// Don't log tokens
_logger.LogInformation("API request with token {Token}", apiToken); // Never!
```

### 5. Dependency Management

#### ✅ DO
- Keep dependencies up to date
- Review Dependabot PRs promptly
- Use package lock files
- Remove unused dependencies
- Audit dependencies regularly

#### ❌ DON'T
- Ignore security warnings
- Use packages from untrusted sources
- Include unnecessary dependencies
- Pin to old vulnerable versions

### 6. Configuration

#### ✅ DO
```csharp
// Use environment variables for secrets
var apiKey = builder.Configuration["ApiKey"] 
    ?? throw new InvalidOperationException("ApiKey not configured");

// Use User Secrets in development
builder.Configuration.AddUserSecrets<Program>();

// Validate configuration
if (string.IsNullOrEmpty(config.DatabaseConnection))
    throw new InvalidOperationException("Database connection not configured");
```

#### ❌ DON'T
```csharp
// Don't hardcode secrets
const string ApiKey = "sk_live_12345"; // Never!
const string ConnectionString = "Server=prod;User=sa;Password=admin"; // Never!

// Don't commit secrets to source control
```

---

## Frontend (Angular) Guidelines

### 1. Input Validation & Sanitization

#### ✅ DO
```typescript
// Use Angular's DomSanitizer
import { DomSanitizer } from '@angular/platform-browser';

constructor(private sanitizer: DomSanitizer) {}

sanitizeUrl(url: string) {
  return this.sanitizer.sanitize(SecurityContext.URL, url);
}

// Validate input
validateEmail(email: string): boolean {
  const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
  return emailRegex.test(email);
}
```

#### ❌ DON'T
```typescript
// Don't use innerHTML with user input
element.innerHTML = userInput; // XSS vulnerability!

// Don't bypass sanitization
this.sanitizer.bypassSecurityTrustHtml(userInput); // Dangerous!
```

### 2. HTTP Security

#### ✅ DO
```typescript
// Use interceptors for authentication
@Injectable()
export class AuthInterceptor implements HttpInterceptor {
  intercept(req: HttpRequest<any>, next: HttpHandler) {
    const token = this.getToken();
    if (token) {
      req = req.clone({
        setHeaders: { Authorization: `Bearer ${token}` }
      });
    }
    return next.handle(req);
  }
}

// Validate responses
this.http.get<User[]>('/api/users').pipe(
  map(users => users.filter(u => u.id > 0)), // Validate data
  catchError(error => {
    this.logger.error('Failed to fetch users', error);
    return throwError(() => new Error('Failed to fetch users'));
  })
);
```

#### ❌ DON'T
```typescript
// Don't trust API responses blindly
this.http.get('/api/data').subscribe(data => {
  eval(data.code); // Never execute code from API!
});

// Don't expose tokens in URL
this.http.get(`/api/data?token=${token}`); // Use headers instead!
```

### 3. Component Security

#### ✅ DO
```typescript
// Use OnPush change detection for better security
@Component({
  changeDetection: ChangeDetectionStrategy.OnPush,
  // ...
})

// Implement proper cleanup
ngOnDestroy() {
  this.subscriptions.unsubscribe();
}

// Validate forms
this.form = this.fb.group({
  email: ['', [Validators.required, Validators.email]],
  password: ['', [Validators.required, Validators.minLength(8)]]
});
```

#### ❌ DON'T
```typescript
// Don't store sensitive data in component state
export class UserComponent {
  password: string; // Never store passwords!
  creditCard: string; // Never store in memory!
}
```

### 4. Routing Security

#### ✅ DO
```typescript
// Use route guards
@Injectable()
export class AuthGuard implements CanActivate {
  canActivate(route: ActivatedRouteSnapshot): boolean {
    if (this.authService.isAuthenticated()) {
      return true;
    }
    this.router.navigate(['/login']);
    return false;
  }
}

// Configure routes with guards
const routes: Routes = [
  {
    path: 'admin',
    canActivate: [AuthGuard],
    loadChildren: () => import('./admin/admin.module')
  }
];
```

#### ❌ DON'T
```typescript
// Don't rely on client-side authorization alone
// Always verify on the server!
```

### 5. Local Storage Security

#### ✅ DO
```typescript
// Store non-sensitive data only
localStorage.setItem('theme', 'dark');
localStorage.setItem('language', 'en');

// Use sessionStorage for short-lived data
sessionStorage.setItem('tempData', JSON.stringify(data));
```

#### ❌ DON'T
```typescript
// Don't store sensitive data in localStorage
localStorage.setItem('password', password); // Never!
localStorage.setItem('apiKey', apiKey); // Never!
localStorage.setItem('ssn', ssn); // Never!

// Tokens should be in HttpOnly cookies or use secure storage
```

---

## Database Security

### 1. Query Safety

#### ✅ DO
```csharp
// Use parameterized queries
using var command = connection.CreateCommand();
command.CommandText = "SELECT * FROM Users WHERE Id = @id";
command.Parameters.AddWithValue("@id", userId);

// Use Entity Framework with LINQ
var user = await context.Users
    .Where(u => u.Id == userId)
    .FirstOrDefaultAsync();

// Use Dapper with parameters
var user = await connection.QueryFirstOrDefaultAsync<User>(
    "SELECT * FROM Users WHERE Id = @Id",
    new { Id = userId });
```

#### ❌ DON'T
```csharp
// Don't concatenate SQL strings
var query = $"SELECT * FROM Users WHERE Id = {userId}"; // SQL Injection!
var query = "SELECT * FROM Users WHERE Name = '" + userName + "'"; // SQL Injection!
```

### 2. Connection Strings

#### ✅ DO
```csharp
// Use environment variables
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

// Use connection string builders
var builder = new SqlConnectionStringBuilder
{
    DataSource = config["DbServer"],
    InitialCatalog = config["DbName"],
    IntegratedSecurity = true,
    Encrypt = true, // Always encrypt
    TrustServerCertificate = false
};
```

#### ❌ DON'T
```csharp
// Don't hardcode connection strings
const string ConnectionString = "Server=prod;Database=mydb;User=admin;Password=123"; // Never!

// Don't disable encryption
builder.Encrypt = false; // Dangerous!
```

---

## Docker & Infrastructure

### 1. Dockerfile Security

#### ✅ DO
```dockerfile
# Use official base images
FROM mcr.microsoft.com/dotnet/aspnet:9.0

# Create non-root user
RUN groupadd -r appuser && useradd -r -g appuser appuser

# Set ownership
RUN chown -R appuser:appuser /app

# Switch to non-root user
USER appuser

# Add health check
HEALTHCHECK --interval=30s --timeout=3s CMD curl -f http://localhost/ || exit 1
```

#### ❌ DON'T
```dockerfile
# Don't run as root
USER root # Dangerous in production!

# Don't include secrets
ENV API_KEY=secret123 # Never!

# Don't use latest tag
FROM nginx:latest # Pin specific versions
```

### 2. Docker Compose Security

#### ✅ DO
```yaml
services:
  backend:
    environment:
      - ASPNETCORE_ENVIRONMENT=${ENVIRONMENT:-Production}
      - ConnectionString=${CONNECTION_STRING}
    networks:
      - private-network
    read_only: true # When possible
    security_opt:
      - no-new-privileges:true
```

#### ❌ DON'T
```yaml
services:
  backend:
    environment:
      - PASSWORD=admin123 # Don't hardcode secrets!
    privileged: true # Don't use privileged mode!
```

---

## Code Review Checklist

### Security Review Points

When reviewing code, check for:

#### Input Validation
- [ ] All user input validated and sanitized
- [ ] Whitelist validation used (not blacklist)
- [ ] Proper data type validation
- [ ] Length and range checks

#### Authentication & Authorization
- [ ] Authentication required for protected endpoints
- [ ] Authorization checks in place
- [ ] Proper session management
- [ ] No hardcoded credentials

#### Data Protection
- [ ] Sensitive data encrypted
- [ ] No sensitive data in logs
- [ ] Proper error handling (no info leakage)
- [ ] HTTPS used for data transmission

#### Dependencies
- [ ] No known vulnerable dependencies
- [ ] Unused dependencies removed
- [ ] Dependencies from trusted sources

#### Configuration
- [ ] No hardcoded secrets
- [ ] Environment variables used properly
- [ ] Secure defaults configured

#### Logging
- [ ] Security events logged
- [ ] No sensitive data in logs
- [ ] Proper log levels used

#### Error Handling
- [ ] Errors caught and handled
- [ ] No stack traces exposed in production
- [ ] Generic error messages for users

---

## Pre-Commit Checklist

Before committing code, verify:

- [ ] No secrets or credentials in code
- [ ] All tests pass (including security tests)
- [ ] Code follows secure coding guidelines
- [ ] Dependencies are up to date
- [ ] No obvious security vulnerabilities
- [ ] Code has been reviewed
- [ ] Documentation updated if needed

---

## Security Testing Commands

```bash
# Run security scan with CodeQL (automatic in CI/CD)
# Check for vulnerabilities in dependencies
dotnet list package --vulnerable

# Run frontend audit
cd frontend && npm audit

# Build and test
dotnet build
dotnet test
npm run build
npm test
```

---

## Incident Response

If you discover a security vulnerability:

1. **DO NOT** create a public GitHub issue
2. Report it privately via GitHub Security Advisories
3. Contact the maintainer directly
4. Provide details:
   - Description of the vulnerability
   - Steps to reproduce
   - Potential impact
   - Suggested fix (if any)

See [SECURITY.md](../../SECURITY.md) for complete reporting process.

---

## Additional Resources

- [OWASP Secure Coding Practices](https://owasp.org/www-project-secure-coding-practices-quick-reference-guide/)
- [OWASP Top 10](https://owasp.org/www-project-top-ten/)
- [Microsoft Security Best Practices](https://learn.microsoft.com/en-us/dotnet/standard/security/)
- [Angular Security Guide](https://angular.io/guide/security)
- [CWE Top 25](https://cwe.mitre.org/top25/)

---

## Keep Learning

Security is an ongoing process. Stay informed:

- Follow security blogs and newsletters
- Participate in security training
- Review security updates regularly
- Learn from security incidents
- Share knowledge with the team

**Remember: Security is everyone's responsibility!**
