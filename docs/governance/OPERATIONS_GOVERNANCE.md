# Operations and Deployment Governance

## Overview

This document establishes the operational governance framework for deploying, monitoring, and maintaining the Grafana-banana application in various environments. It defines processes, standards, and best practices for reliable operations.

## Table of Contents

1. [Environment Management](#environment-management)
2. [Deployment Standards](#deployment-standards)
3. [Configuration Management](#configuration-management)
4. [Infrastructure as Code](#infrastructure-as-code)
5. [Monitoring and Alerting](#monitoring-and-alerting)
6. [Incident Management](#incident-management)
7. [Backup and Recovery](#backup-and-recovery)
8. [Capacity Planning](#capacity-planning)
9. [Performance Management](#performance-management)
10. [Operational Runbooks](#operational-runbooks)

## Environment Management

### Environment Types

| Environment | Purpose | Stability | Access | Data |
|-------------|---------|-----------|--------|------|
| **Development** | Active development | Unstable | All developers | Synthetic |
| **Testing/QA** | Testing and validation | Semi-stable | QA team, developers | Synthetic |
| **Staging** | Pre-production validation | Stable | Limited | Sanitized production-like |
| **Production** | Live system | Highly stable | Operations only | Real |

### Environment Characteristics

**Development:**
- Frequent changes and deployments
- May be incomplete or broken
- Individual developer machines or shared dev environment
- Use Docker Compose for local development
- Hot reload enabled for rapid development

**Testing/QA:**
- Automated test execution
- Manual QA testing
- Integration testing with dependencies
- Reset frequently
- Similar configuration to production

**Staging:**
- Production mirror (infrastructure and configuration)
- Final validation before production
- Performance testing
- User acceptance testing (UAT)
- Limited access, controlled changes

**Production:**
- Live, customer-facing system
- Maximum stability and availability
- Strict change control
- Complete monitoring and alerting
- Disaster recovery capabilities

### Environment Promotion

```
Development → Testing → Staging → Production
    │            │         │          │
    │            │         │          │
  Feature    Integration Pre-production Production
  Testing      Testing    Validation   Release
```

**Promotion Criteria:**
- All tests pass
- Security scan clean
- Performance benchmarks met
- Documentation updated
- Rollback plan documented
- Required approvals obtained

## Deployment Standards

### Deployment Principles

1. **Automation**: All deployments fully automated
2. **Repeatability**: Same process across all environments
3. **Traceability**: Every deployment tracked and auditable
4. **Rollback**: Always have rollback capability
5. **Zero Downtime**: Production deployments without downtime
6. **Observability**: Monitor deployment impact

### Deployment Methods

**Blue-Green Deployment:**
```
┌─────────┐
│   LB    │
└────┬────┘
     │
     ├──────┐
     │      │
  ┌──▼──┐ ┌▼────┐
  │Blue │ │Green│
  │(Old)│ │(New)│
  └─────┘ └─────┘
```

1. Deploy new version (Green)
2. Test Green environment
3. Switch traffic to Green
4. Monitor for issues
5. Keep Blue for rollback
6. Decommission Blue when stable

**Rolling Deployment:**
```
[Old][Old][Old]  →  [New][Old][Old]
                 →  [New][New][Old]
                 →  [New][New][New]
```

1. Update instances one at a time
2. Health check each instance
3. Continue if healthy
4. Rollback if issues detected

**Canary Deployment:**
```
    ┌─────────┐
    │   LB    │
    └────┬────┘
         │
    ┌────┴─────────┐
    │ 95%      5%  │
┌───▼───┐    ┌────▼───┐
│ Stable│    │ Canary │
│ (Old) │    │ (New)  │
└───────┘    └────────┘
```

1. Deploy to small subset (5-10%)
2. Monitor metrics closely
3. Gradually increase percentage
4. Rollback if issues
5. Complete when 100% healthy

### Deployment Checklist

**Pre-Deployment:**
- [ ] Code reviewed and approved
- [ ] All tests passing in CI/CD
- [ ] Security scan passed
- [ ] Performance tests passed
- [ ] Documentation updated
- [ ] Rollback plan documented
- [ ] Stakeholders notified
- [ ] Deployment window scheduled
- [ ] Backup taken (if applicable)

**During Deployment:**
- [ ] Deployment automation executed
- [ ] Health checks passing
- [ ] Smoke tests executed
- [ ] Metrics monitored
- [ ] Logs reviewed
- [ ] Performance validated

**Post-Deployment:**
- [ ] Functionality verified
- [ ] Performance metrics normal
- [ ] Error rates acceptable
- [ ] User acceptance confirmed
- [ ] Documentation updated
- [ ] Post-deployment review scheduled

### Rollback Procedures

**Triggers for Rollback:**
- Error rate > 5% (immediate)
- Performance degradation > 50%
- Critical functionality broken
- Security vulnerability introduced
- Data corruption detected

**Rollback Process:**
1. **Decision**: Determine rollback needed
2. **Communication**: Notify stakeholders
3. **Execute**: Trigger rollback automation
4. **Verify**: Confirm system restored
5. **Investigate**: Root cause analysis
6. **Document**: Update incident log

**Rollback Time Targets:**
- Development: 5 minutes
- Testing: 10 minutes
- Staging: 15 minutes
- Production: 5 minutes (critical), 15 minutes (normal)

## Configuration Management

### Configuration Principles

1. **Environment-Specific**: Configuration varies by environment
2. **Externalized**: No hardcoded configuration in code
3. **Secure**: Secrets managed securely
4. **Versioned**: Configuration changes tracked
5. **Validated**: Configuration validated before use

### Configuration Hierarchy

```
Default Config
    ↓
Environment Config (Development/Staging/Production)
    ↓
Override Config (Local/Runtime)
```

### Configuration Storage

**Non-Sensitive Configuration:**
- Stored in Git repository
- Environment-specific files
- appsettings.json, appsettings.{Environment}.json
- Environment variables

**Sensitive Configuration:**
- Stored in secrets management system
- Never committed to repository
- Injected at runtime
- Rotated regularly

**Example Structure:**
```
configuration/
├── base.yml                    # Base configuration
├── development.yml             # Development overrides
├── staging.yml                 # Staging overrides
├── production.yml              # Production overrides
└── secrets/                    # Secrets (not in Git)
    ├── development.secrets
    ├── staging.secrets
    └── production.secrets
```

### Secrets Management

**Current (Development):**
- Environment variables
- Local .env files (not committed)
- Docker secrets for compose

**Future (Production):**
- Azure Key Vault / AWS Secrets Manager / HashiCorp Vault
- Automatic rotation
- Access logging
- Encryption at rest

**Secrets Rotation:**
- API keys: Every 90 days
- Database passwords: Every 90 days
- Certificates: 30 days before expiration
- Service accounts: Every 180 days

## Infrastructure as Code

### IaC Principles

1. **Everything as Code**: All infrastructure defined in code
2. **Version Control**: All IaC in Git
3. **Idempotent**: Can apply multiple times safely
4. **Documented**: Clear documentation and comments
5. **Tested**: IaC changes tested before production

### Current Implementation

**Docker Compose:**
```yaml
# docker-compose.yml
services:
  frontend:
    build: ./frontend
    ports:
      - "4200:4200"
    environment:
      - NODE_ENV=production
    depends_on:
      - backend

  backend:
    build: ./backend/GrafanaBanana.Api
    ports:
      - "5000:5000"
    environment:
      - ASPNETCORE_ENVIRONMENT=Production
```

### Future: Kubernetes

**Example Deployment:**
```yaml
apiVersion: apps/v1
kind: Deployment
metadata:
  name: backend
spec:
  replicas: 3
  selector:
    matchLabels:
      app: backend
  template:
    metadata:
      labels:
        app: backend
    spec:
      containers:
      - name: api
        image: ghcr.io/markcoleman/grafana-banana/backend:latest
        ports:
        - containerPort: 5000
        resources:
          requests:
            cpu: 100m
            memory: 128Mi
          limits:
            cpu: 500m
            memory: 512Mi
        livenessProbe:
          httpGet:
            path: /health/live
            port: 5000
          initialDelaySeconds: 30
          periodSeconds: 10
        readinessProbe:
          httpGet:
            path: /health/ready
            port: 5000
          initialDelaySeconds: 10
          periodSeconds: 5
```

## Monitoring and Alerting

### Monitoring Strategy

**Four Golden Signals:**
1. **Latency**: Request response time
2. **Traffic**: Request volume
3. **Errors**: Error rate
4. **Saturation**: Resource utilization

### Monitoring Levels

**Infrastructure Monitoring:**
- CPU utilization
- Memory usage
- Disk I/O
- Network bandwidth
- Container health

**Application Monitoring:**
- Request rate
- Response time
- Error rate
- Custom business metrics
- Endpoint availability

**User Experience Monitoring:**
- Page load time
- Time to interactive
- User journey completion
- Error rates by user action

### Alerting Rules

**Critical Alerts** (Page immediately):
- Service down (availability < 90%)
- Error rate > 5%
- Database unavailable
- Critical security events

**High Priority Alerts** (Notify within 15 minutes):
- Error rate > 2%
- Response time p95 > 1s
- Disk space > 90%
- Memory usage > 85%

**Medium Priority Alerts** (Notify within 1 hour):
- Error rate > 1%
- Response time p95 > 500ms
- Unusual traffic patterns
- Certificate expiring < 30 days

**Low Priority Alerts** (Daily summary):
- Performance trends
- Capacity planning warnings
- Non-critical configuration issues

### Alert Response

**Severity Levels:**

| Severity | Response Time | Escalation | Communication |
|----------|--------------|------------|---------------|
| P0 (Critical) | Immediate | After 15 min | Status page |
| P1 (High) | 15 minutes | After 1 hour | Internal |
| P2 (Medium) | 1 hour | After 4 hours | Internal |
| P3 (Low) | Next business day | None | Ticket |

## Incident Management

### Incident Classification

**P0 - Critical:**
- Complete service outage
- Data loss or corruption
- Security breach
- Impact: All users

**P1 - High:**
- Major functionality unavailable
- Severe performance degradation
- Impact: Most users

**P2 - Medium:**
- Minor functionality unavailable
- Moderate performance issues
- Impact: Some users

**P3 - Low:**
- Cosmetic issues
- Minimal performance impact
- Impact: Few users

### Incident Response Process

```
Detection → Triage → Response → Resolution → Review
    │         │         │           │           │
 Monitor   Assess   Mitigate    Restore   Improve
```

**1. Detection:**
- Automated monitoring alerts
- User reports
- Health check failures

**2. Triage:**
- Assess severity and impact
- Assign incident commander
- Assemble response team
- Create incident record

**3. Response:**
- Communicate status
- Investigate root cause
- Implement mitigation
- Update stakeholders

**4. Resolution:**
- Implement fix
- Verify restoration
- Monitor for stability
- Communicate resolution

**5. Review:**
- Post-incident review (PIR)
- Document lessons learned
- Identify improvements
- Implement preventions

### Post-Incident Review

**Within 24-48 hours of resolution:**

1. **Timeline**: What happened and when
2. **Root Cause**: Why did it happen
3. **Impact**: Who/what was affected
4. **Response**: What worked/didn't work
5. **Action Items**: Preventive measures
6. **Follow-up**: Tracking improvements

## Backup and Recovery

### Backup Strategy

**Current (Stateless):**
- Configuration files in Git
- Container images in registry
- No data backup needed

**Future (With Data):**

**Backup Types:**
1. **Full Backup**: Complete system backup (Weekly)
2. **Incremental**: Changes since last backup (Daily)
3. **Transaction Log**: Continuous backup (Real-time)

**Backup Schedule:**
- Development: Not required
- Testing: Weekly
- Staging: Daily
- Production: Multiple daily + continuous

### Recovery Objectives

| Environment | RTO | RPO | Backup Frequency |
|------------|-----|-----|-----------------|
| Development | 4 hours | 1 day | Weekly |
| Testing | 2 hours | 1 day | Daily |
| Staging | 1 hour | 4 hours | Daily |
| Production | 15 minutes | 5 minutes | Continuous |

**RTO (Recovery Time Objective)**: Maximum acceptable downtime  
**RPO (Recovery Point Objective)**: Maximum acceptable data loss

### Disaster Recovery

**DR Scenarios:**
1. Application failure
2. Database corruption
3. Infrastructure failure
4. Region/datacenter outage
5. Security incident

**DR Testing:**
- Quarterly DR drills
- Annual full DR test
- Document results and improvements

## Capacity Planning

### Capacity Metrics

**Current Baseline:**
- Request rate: 10 RPS
- Response time: < 200ms (p95)
- Error rate: < 0.1%
- CPU: < 20%
- Memory: < 40%

**Growth Projections:**
- Monthly growth: +10% users
- Annual capacity review
- Quarterly infrastructure review

### Scaling Triggers

**Scale Out (Horizontal):**
- CPU > 70% for 10 minutes
- Memory > 80% for 10 minutes
- Request queue > 100

**Scale In:**
- CPU < 30% for 30 minutes
- Memory < 40% for 30 minutes
- Request queue < 10

## Performance Management

### Performance Targets

See [Governance Framework - Metrics and KPIs](../GOVERNANCE.md#metrics-and-kpis)

### Performance Testing

**Load Testing:**
- Quarterly or before major releases
- Test at 2x expected peak load
- Identify bottlenecks
- Validate scaling strategy

**Stress Testing:**
- Test system limits
- Identify breaking points
- Verify graceful degradation

**Endurance Testing:**
- Run at normal load for extended period
- Identify memory leaks
- Verify stability

## Operational Runbooks

### Standard Operating Procedures

**Daily Operations:**
- Review monitoring dashboards
- Check alert status
- Review error logs
- Verify backup completion
- Check system health

**Weekly Operations:**
- Review capacity metrics
- Analyze performance trends
- Security patch review
- Dependency updates
- Team sync meeting

**Monthly Operations:**
- Capacity planning review
- Security audit
- DR test planning
- Cost optimization review
- Documentation update

### Common Operations

**Restart Service:**
```bash
# Docker Compose
docker-compose restart backend

# Kubernetes
kubectl rollout restart deployment/backend
```

**View Logs:**
```bash
# Docker Compose
docker-compose logs -f backend

# Kubernetes
kubectl logs -f deployment/backend
```

**Scale Service:**
```bash
# Docker Compose
docker-compose up -d --scale backend=3

# Kubernetes
kubectl scale deployment/backend --replicas=3
```

## References

- [Governance Framework](./GOVERNANCE.md)
- [Technical Architecture](./architecture/TECHNICAL_ARCHITECTURE.md)
- [CI/CD Workflows](./CI_CD_WORKFLOWS.md)
- [Security Policy](../SECURITY.md)

## Document Control

| Version | Date | Author | Changes |
|---------|------|--------|---------|
| 1.0.0 | 2025-10-25 | System | Initial operations governance |

---

**Last Updated**: 2025-10-25  
**Next Review Date**: 2026-01-25  
**Document Owner**: Operations Lead  
**Approvers**: Technical Lead, Architecture Review Board
