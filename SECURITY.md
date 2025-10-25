# Security Policy

## Supported Versions

We release patches for security vulnerabilities for the following versions:

| Version | Supported          |
| ------- | ------------------ |
| Latest  | :white_check_mark: |
| < Latest| :x:                |

## Reporting a Vulnerability

If you discover a security vulnerability in Grafana-banana, please report it by:

1. **Do NOT create a public GitHub issue** - Security vulnerabilities should not be publicly disclosed until they have been addressed.

2. **Send a private report** to the project maintainer [@markcoleman](https://github.com/markcoleman) through one of these methods:
   - Use GitHub's private vulnerability reporting feature (recommended)
   - Send a direct message via GitHub
   - Contact through other secure channels

3. **Include the following information** in your report:
   - Type of vulnerability
   - Full paths of source file(s) related to the vulnerability
   - Location of the affected source code (tag/branch/commit or direct URL)
   - Step-by-step instructions to reproduce the issue
   - Proof-of-concept or exploit code (if possible)
   - Impact of the issue, including how an attacker might exploit it

## What to Expect

- **Acknowledgment**: You will receive an acknowledgment of your report within 48 hours
- **Investigation**: We will investigate the issue and determine its severity
- **Updates**: You will receive updates on the progress toward a fix
- **Fix**: Once validated, we will develop and test a fix
- **Disclosure**: We will coordinate the disclosure timeline with you
- **Credit**: Security researchers who responsibly disclose vulnerabilities will be credited (unless they prefer to remain anonymous)

## Security Update Process

1. Security vulnerabilities are addressed with the highest priority
2. Fixes are developed and tested in a private branch
3. A security advisory is prepared
4. The fix is released and the advisory is published
5. Affected users are notified through GitHub Security Advisories

## Security Best Practices

When deploying Grafana-banana:

### Comprehensive Security Documentation

For detailed security guidelines and best practices, see:
- **[OWASP Top 10 Compliance Guide](docs/security/OWASP_COMPLIANCE.md)** - Complete guide on addressing OWASP Top 10 vulnerabilities
- **[API Security Best Practices](docs/security/API_SECURITY.md)** - Comprehensive API security guidelines
- **[Secure Coding Guidelines](docs/security/SECURE_CODING_GUIDELINES.md)** - Developer guidelines for writing secure code
- **[PR Security Checklist](docs/security/PR_SECURITY_CHECKLIST.md)** - Security checklist for pull requests

### General
- Keep all dependencies up to date
- Review and act on Dependabot alerts
- Use strong authentication mechanisms
- Follow the principle of least privilege

### Backend (.NET API)
- Use HTTPS in production
- Enable authentication and authorization
- Validate all input data
- Keep .NET runtime and packages updated
- Review and configure CORS appropriately
- Use secure connection strings and environment variables

### Frontend (Angular)
- Keep npm packages updated
- Implement Content Security Policy (CSP)
- Sanitize user input
- Use HTTPS
- Enable security headers

### Infrastructure
- Keep Docker images updated
- Use official base images
- Don't run containers as root
- Scan images for vulnerabilities
- Keep observability stack components updated
- Secure Grafana, Prometheus, Tempo, and Loki endpoints
- Use strong credentials for all services

### Development
- Never commit secrets, API keys, or credentials
- Use environment variables for sensitive configuration
- Review code changes for security issues
- Run security scanning tools regularly
- Keep dev container images updated

## Automated Security

This project uses:
- **Dependabot**: Automated dependency updates for npm, NuGet, Docker, and GitHub Actions
- **GitHub Security Advisories**: Notifications for known vulnerabilities
- **CodeQL** (if configured): Static analysis for security issues

## Additional Resources

- [OWASP Top Ten](https://owasp.org/www-project-top-ten/)
- [.NET Security Best Practices](https://learn.microsoft.com/en-us/aspnet/core/security/)
- [Angular Security Guide](https://angular.io/guide/security)
- [Docker Security Best Practices](https://docs.docker.com/engine/security/)

## Contact

For any security-related questions or concerns, please contact the maintainer at [@markcoleman](https://github.com/markcoleman).
