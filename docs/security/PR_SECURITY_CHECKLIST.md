# Pull Request Security Checklist

Use this checklist when creating or reviewing pull requests to ensure security standards are maintained.

## For PR Authors

Before submitting your PR, verify:

### Code Security
- [ ] No secrets, API keys, passwords, or credentials in code
- [ ] All user input is validated and sanitized
- [ ] Authentication and authorization checks are in place where needed
- [ ] Error handling doesn't expose sensitive information
- [ ] No hardcoded configuration or environment-specific values

### Data Protection
- [ ] Sensitive data is not logged
- [ ] PII (Personally Identifiable Information) is properly protected
- [ ] Data encryption is used where appropriate
- [ ] Database queries use parameterized queries (no string concatenation)

### Dependencies
- [ ] No new vulnerabilities introduced (check Dependabot alerts)
- [ ] Dependencies are from trusted sources
- [ ] Unused dependencies removed
- [ ] Dependencies are up to date

### Infrastructure
- [ ] Docker containers run as non-root user
- [ ] Health checks included where appropriate
- [ ] Resource limits configured
- [ ] Security headers properly configured

### Testing
- [ ] Security-related tests added for new features
- [ ] All existing tests pass
- [ ] Rate limiting tested (if applicable)
- [ ] Input validation tested with malicious inputs
- [ ] Authentication/authorization tested

### Documentation
- [ ] Security implications documented
- [ ] Configuration changes documented
- [ ] Breaking changes clearly noted
- [ ] Security guidelines followed

## For Reviewers

### Code Review
- [ ] Review changes for common vulnerabilities (OWASP Top 10)
- [ ] Verify input validation and sanitization
- [ ] Check for SQL injection vulnerabilities
- [ ] Check for XSS vulnerabilities
- [ ] Verify authentication and authorization logic
- [ ] Review error handling and logging
- [ ] Ensure no sensitive data exposure

### Security Patterns
- [ ] Rate limiting applied to new endpoints
- [ ] CORS configured correctly
- [ ] Security headers present
- [ ] Proper session management
- [ ] Secure defaults used

### Dependencies
- [ ] Review new dependencies for security issues
- [ ] Check for known vulnerabilities
- [ ] Verify dependency update impacts

### Best Practices
- [ ] Code follows secure coding guidelines
- [ ] Principle of least privilege applied
- [ ] Defense in depth implemented
- [ ] Fail securely on errors

## Common Vulnerabilities to Check

### Injection Flaws
- [ ] SQL Injection - parameterized queries used?
- [ ] Command Injection - user input in system calls?
- [ ] LDAP Injection - LDAP queries sanitized?
- [ ] XPath Injection - XML queries protected?

### Authentication Issues
- [ ] Weak passwords allowed?
- [ ] Session fixation possible?
- [ ] Brute force protection in place?
- [ ] Password reset secure?

### Authorization Issues
- [ ] Insecure direct object references?
- [ ] Missing function level access control?
- [ ] Privilege escalation possible?

### Sensitive Data Exposure
- [ ] Sensitive data in logs?
- [ ] Sensitive data in URLs?
- [ ] Sensitive data in error messages?
- [ ] Encryption used for sensitive data?

### Security Misconfiguration
- [ ] Default credentials changed?
- [ ] Unnecessary features enabled?
- [ ] Stack traces exposed?
- [ ] Security headers missing?

### Cross-Site Scripting (XSS)
- [ ] User input properly escaped?
- [ ] Content Security Policy configured?
- [ ] Avoid innerHTML with user data?

### Insecure Deserialization
- [ ] Deserializing untrusted data?
- [ ] Type validation on deserialization?

### Using Components with Known Vulnerabilities
- [ ] Check Dependabot alerts
- [ ] Verify npm audit / dotnet list package --vulnerable

### Insufficient Logging & Monitoring
- [ ] Security events logged?
- [ ] Proper log levels used?
- [ ] No sensitive data in logs?

## Automated Checks

The following checks run automatically:
- [ ] CodeQL security scanning
- [ ] Dependency vulnerability scanning (Dependabot)
- [ ] Build and test suite
- [ ] Linting and code style

## Questions to Ask

1. **What security risks does this change introduce?**
2. **Have security implications been considered?**
3. **Are there any edge cases that could be exploited?**
4. **Does this follow our security guidelines?**
5. **Is this change necessary, or is there a more secure approach?**

## Red Flags 🚩

Watch out for these red flags:

- Commented-out security checks
- TODO/HACK comments related to security
- Disabled security features "temporarily"
- Custom cryptography implementations
- Bypassing validation or authorization
- Exposing internal implementation details
- Large changes without adequate testing

## Resources

- [OWASP Top 10 Compliance](./OWASP_COMPLIANCE.md)
- [API Security Best Practices](./API_SECURITY.md)
- [Secure Coding Guidelines](./SECURE_CODING_GUIDELINES.md)
- [Security Policy](../../SECURITY.md)

## Approval Guidelines

**Do not approve** if:
- Critical security issues present
- Secrets or credentials committed
- Known vulnerabilities introduced
- Required checks don't pass

**Request changes** if:
- Security best practices not followed
- Tests are insufficient
- Documentation is missing

**Approve** only when:
- All checklist items verified
- Security concerns addressed
- Tests are adequate
- Documentation is complete

---

## Notes

- Security is everyone's responsibility
- When in doubt, ask for a second opinion
- It's better to be cautious than to approve something risky
- Security reviews may take longer - that's okay!

**Remember**: One security vulnerability can compromise the entire application. Take the time to review thoroughly.
