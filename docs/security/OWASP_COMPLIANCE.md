# OWASP Top 10 Compliance Guide

This document outlines how Grafana-banana addresses each of the OWASP Top 10 2021 security risks and provides guidelines for developers to maintain security compliance.

## Table of Contents
1. [A01:2021 – Broken Access Control](#a012021--broken-access-control)
2. [A02:2021 – Cryptographic Failures](#a022021--cryptographic-failures)
3. [A03:2021 – Injection](#a032021--injection)
4. [A04:2021 – Insecure Design](#a042021--insecure-design)
5. [A05:2021 – Security Misconfiguration](#a052021--security-misconfiguration)
6. [A06:2021 – Vulnerable and Outdated Components](#a062021--vulnerable-and-outdated-components)
7. [A07:2021 – Identification and Authentication Failures](#a072021--identification-and-authentication-failures)
8. [A08:2021 – Software and Data Integrity Failures](#a082021--software-and-data-integrity-failures)
9. [A09:2021 – Security Logging and Monitoring Failures](#a092021--security-logging-and-monitoring-failures)
10. [A10:2021 – Server-Side Request Forgery (SSRF)](#a102021--server-side-request-forgery-ssrf)

---

## A01:2021 – Broken Access Control

### Current Implementation
- Rate limiting implemented to prevent abuse
- CORS policies configured to restrict cross-origin requests
- Environment-based configuration for development vs production

### Developer Guidelines
- **Always validate authorization** before granting access to resources
- **Implement the principle of least privilege** - users should only have access to what they need
- **Never rely on client-side checks** for access control
- **Use framework-provided authorization mechanisms** (e.g., ASP.NET Core Authorization policies)

### Example
```csharp
// BAD - No access control
app.MapGet("/admin/users", () => GetAllUsers());

// GOOD - With authorization
app.MapGet("/admin/users", () => GetAllUsers())
    .RequireAuthorization("AdminPolicy");
```

---

## A02:2021 – Cryptographic Failures

### Current Implementation
- HTTPS enforcement configured for production environments
- HSTS headers enabled in production
- Sensitive data logging is avoided

### Developer Guidelines
- **Always use HTTPS in production** - never transmit sensitive data over HTTP
- **Never log sensitive information** (passwords, tokens, API keys, PII)
- **Use secure hashing algorithms** (bcrypt, Argon2) for passwords, never plain text or MD5/SHA1
- **Encrypt sensitive data at rest** when storing in databases
- **Use strong encryption libraries** - don't implement your own cryptography

### Example
```csharp
// BAD - Logging sensitive data
_logger.LogInformation("User {UserId} logged in with password {Password}", userId, password);

// GOOD - Don't log sensitive data
_logger.LogInformation("User {UserId} logged in successfully", userId);
```

---

## A03:2021 – Injection

### Current Implementation
- Input validation middleware to detect malicious patterns
- Request body sanitization
- Query parameter validation

### Developer Guidelines
- **Always validate and sanitize input** from all sources (query params, body, headers)
- **Use parameterized queries** or ORMs to prevent SQL injection
- **Validate input against a whitelist**, not a blacklist
- **Encode output** when displaying user input
- **Use built-in validation attributes** in .NET models

### Example
```csharp
// BAD - String concatenation in queries (if using raw SQL)
var query = $"SELECT * FROM Users WHERE Id = {userId}";

// GOOD - Parameterized queries
var query = "SELECT * FROM Users WHERE Id = @userId";
// Or use Entity Framework/Dapper with parameters

// GOOD - Input validation
public record CreateUserRequest(
    [Required, StringLength(100)] string Name,
    [Required, EmailAddress] string Email,
    [Range(18, 120)] int Age
);
```

---

## A04:2021 – Insecure Design

### Current Implementation
- Security middleware integrated from the start
- Rate limiting to prevent DoS
- Health checks for service monitoring
- Comprehensive logging and telemetry

### Developer Guidelines
- **Design with security in mind** from the beginning, not as an afterthought
- **Use threat modeling** to identify potential security issues early
- **Implement defense in depth** - multiple layers of security
- **Follow secure coding standards** and design patterns
- **Regular security reviews** of design and architecture

### Security Design Checklist
- [ ] Authentication and authorization strategy defined
- [ ] Data validation and sanitization at boundaries
- [ ] Rate limiting and DoS protection
- [ ] Security logging and monitoring
- [ ] Error handling that doesn't leak sensitive information
- [ ] Secure defaults (deny by default, allow explicitly)

---

## A05:2021 – Security Misconfiguration

### Current Implementation
- Security headers configured (X-Frame-Options, CSP, HSTS, etc.)
- Server identification headers removed
- Environment-specific configurations
- Docker images run as non-root users
- Production vs development environment separation

### Developer Guidelines
- **Keep all software up to date** (frameworks, libraries, OS)
- **Use secure defaults** for all configurations
- **Remove or disable unused features** and frameworks
- **Don't expose stack traces or detailed errors** in production
- **Regularly review and harden configurations**
- **Use environment variables** for sensitive configuration, never hardcode

### Configuration Checklist
- [ ] All default passwords changed
- [ ] Unnecessary features/services disabled
- [ ] Error messages don't reveal sensitive information
- [ ] Security headers properly configured
- [ ] HTTPS enforced in production
- [ ] Secrets managed via environment variables or secret managers

---

## A06:2021 – Vulnerable and Outdated Components

### Current Implementation
- Dependabot configured for automated dependency updates
- Weekly dependency scans
- Grouped updates for related packages (OpenTelemetry, Serilog, Angular)
- CodeQL security scanning

### Developer Guidelines
- **Keep dependencies up to date** - respond to Dependabot PRs promptly
- **Remove unused dependencies** - fewer dependencies = smaller attack surface
- **Monitor security advisories** for the libraries you use
- **Only use components from trusted sources**
- **Regularly audit your dependencies** for known vulnerabilities

### Maintenance Process
1. Review Dependabot alerts weekly
2. Update critical security patches immediately
3. Test updates in development before production
4. Document any dependency-related decisions

---

## A07:2021 – Identification and Authentication Failures

### Current Implementation
- Rate limiting to prevent brute force attacks
- Security logging of authentication attempts
- Framework ready for authentication implementation

### Developer Guidelines
- **Implement multi-factor authentication (MFA)** for sensitive operations
- **Use strong password policies** (length, complexity, no common passwords)
- **Implement account lockout** after failed attempts
- **Never store passwords in plain text** - use proper hashing (bcrypt, Argon2)
- **Secure session management** - proper timeout, secure cookies
- **Implement proper logout** functionality

### Authentication Checklist
- [ ] Strong password requirements enforced
- [ ] Password hashing with salt (bcrypt/Argon2)
- [ ] Account lockout after failed attempts
- [ ] MFA available for users
- [ ] Secure session management
- [ ] Proper token expiration and refresh

---

## A08:2021 – Software and Data Integrity Failures

### Current Implementation
- Docker images use official base images from Microsoft and trusted sources
- Build process with integrity checks
- Dependency verification through package managers

### Developer Guidelines
- **Use official and trusted sources** for dependencies and base images
- **Verify package signatures** when possible
- **Implement CI/CD security checks** in your pipeline
- **Use integrity checks** (checksums, signatures) for critical data
- **Separate build, test, and production environments**

### CI/CD Security
- [ ] Code review required before merge
- [ ] Automated security scanning (CodeQL, Dependabot)
- [ ] Build artifacts signed
- [ ] Deployment requires authorization
- [ ] Audit trail of all changes

---

## A09:2021 – Security Logging and Monitoring Failures

### Current Implementation
- Comprehensive logging with Serilog
- Structured logging with context
- OpenTelemetry for distributed tracing
- Prometheus metrics
- Grafana dashboards for visualization
- Health check endpoints
- Security event logging (rate limit violations, suspicious requests)

### Developer Guidelines
- **Log all security-relevant events** (authentication, authorization failures, input validation failures)
- **Include sufficient context** in logs for investigation
- **Never log sensitive data** (passwords, tokens, credit cards, PII)
- **Set up alerts** for suspicious patterns
- **Regularly review logs** for anomalies
- **Ensure logs are stored securely** and are tamper-proof

### What to Log
✅ **DO LOG:**
- Authentication attempts (success and failure)
- Authorization failures
- Input validation failures
- Rate limit violations
- System errors and exceptions
- Security-relevant configuration changes

❌ **DON'T LOG:**
- Passwords (plain or hashed)
- Session tokens or API keys
- Credit card numbers
- Personal Identifiable Information (PII)
- Any sensitive data

### Example
```csharp
// BAD - Logging sensitive data
_logger.LogWarning("Login failed for user {Email} with password {Password}", email, password);

// GOOD - Security event without sensitive data
_logger.LogWarning("Login failed for user {Email} from IP {IpAddress}", email, ipAddress);
```

---

## A10:2021 – Server-Side Request Forgery (SSRF)

### Current Implementation
- Input validation on all endpoints
- URL validation to prevent malicious redirects

### Developer Guidelines
- **Validate and sanitize all URLs** provided by users
- **Use allow-lists** for permitted destinations
- **Disable unnecessary URL schemes** (file://, gopher://, etc.)
- **Don't expose internal network information** in responses
- **Implement network segmentation** - internal services shouldn't be reachable from user requests

### Example
```csharp
// BAD - No validation
var response = await httpClient.GetAsync(userProvidedUrl);

// GOOD - Validate URL
if (!IsValidUrl(userProvidedUrl) || !IsAllowedDomain(userProvidedUrl))
{
    return Results.BadRequest("Invalid URL");
}
var response = await httpClient.GetAsync(userProvidedUrl);
```

---

## Additional Security Resources

### OWASP Resources
- [OWASP Top 10](https://owasp.org/www-project-top-ten/)
- [OWASP Cheat Sheet Series](https://cheatsheetseries.owasp.org/)
- [OWASP API Security Top 10](https://owasp.org/www-project-api-security/)

### .NET Security
- [ASP.NET Core Security](https://learn.microsoft.com/en-us/aspnet/core/security/)
- [Secure coding guidelines](https://learn.microsoft.com/en-us/dotnet/standard/security/secure-coding-guidelines)

### Regular Security Tasks
- [ ] Weekly: Review Dependabot alerts
- [ ] Monthly: Security audit of new features
- [ ] Quarterly: Full security review
- [ ] Annually: Penetration testing (if applicable)

---

## Getting Help

If you discover a security vulnerability or have security concerns:
1. **DO NOT** create a public GitHub issue
2. Report privately through GitHub Security Advisories
3. Contact the maintainer directly
4. See [SECURITY.md](../../SECURITY.md) for detailed reporting instructions
