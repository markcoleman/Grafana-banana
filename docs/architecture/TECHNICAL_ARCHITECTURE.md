# Technical Architecture Overview

## Executive Summary

Grafana-banana is a modern, full-stack web application demonstrating enterprise-grade architecture with comprehensive observability. The system follows cloud-native principles, implements the three pillars of observability (metrics, logs, traces), and adheres to security and operational best practices.

**Key Characteristics:**
- **Architecture Style**: Microservices-ready, API-first design
- **Deployment Model**: Containerized, cloud-agnostic
- **Scalability**: Horizontally scalable backend and frontend
- **Observability**: Full-stack instrumentation with OpenTelemetry
- **Security**: Defense-in-depth with multiple security layers

## Table of Contents

1. [System Architecture](#system-architecture)
2. [Component Architecture](#component-architecture)
3. [Data Architecture](#data-architecture)
4. [Security Architecture](#security-architecture)
5. [Deployment Architecture](#deployment-architecture)
6. [Integration Architecture](#integration-architecture)
7. [Observability Architecture](#observability-architecture)
8. [Network Architecture](#network-architecture)
9. [Development Architecture](#development-architecture)
10. [Scalability and Performance](#scalability-and-performance)

## System Architecture

### High-Level Architecture

```
┌─────────────────────────────────────────────────────────────┐
│                         Internet                             │
└─────────────────────────┬───────────────────────────────────┘
                          │
                          │ HTTPS
                          │
┌─────────────────────────▼───────────────────────────────────┐
│                    Load Balancer / CDN                       │
│                    (Production Deployment)                   │
└────────┬──────────────────────────────┬─────────────────────┘
         │                              │
         │                              │
┌────────▼─────────┐          ┌─────────▼──────────┐
│    Frontend      │          │     Backend        │
│   (Angular)      │◄────────►│   (.NET API)      │
│   Port: 4200     │   HTTP   │   Port: 5000      │
│                  │          │                    │
│  - Components    │          │  - Minimal APIs    │
│  - Services      │          │  - Business Logic  │
│  - OTLP Browser  │          │  - Health Checks   │
│  - State Mgmt    │          │  - OTLP .NET      │
└────────┬─────────┘          └─────────┬──────────┘
         │                              │
         │                              │
         │    Telemetry (OTLP)         │
         │                              │
         └──────────────┬───────────────┘
                        │
        ┌───────────────┼────────────────┐
        │               │                │
        │               │                │
┌───────▼──────┐ ┌──────▼──────┐ ┌──────▼──────┐
│  Prometheus  │ │    Tempo    │ │    Loki     │
│  (Metrics)   │ │  (Traces)   │ │   (Logs)    │
│  Port: 9090  │ │  Port: 3200 │ │ Port: 3100  │
└───────┬──────┘ └──────┬──────┘ └──────┬──────┘
        │               │                │
        └───────────────┼────────────────┘
                        │
                 ┌──────▼──────┐
                 │   Grafana   │
                 │ (Dashboard) │
                 │  Port: 3000 │
                 └─────────────┘
```

### System Context

**Users:**
- Application Users: Access frontend web application
- Developers: Develop and maintain the system
- Operations: Monitor and operate the system
- Security Teams: Monitor security and compliance

**External Systems:**
- None (standalone application)
- Future: Authentication providers, external APIs

## Component Architecture

### Frontend Architecture (Angular)

```
┌─────────────────────────────────────────────────┐
│              Angular Application                │
├─────────────────────────────────────────────────┤
│  Presentation Layer                             │
│  ┌──────────────┐  ┌──────────────┐           │
│  │  Components  │  │   Templates  │           │
│  │  (Standalone)│  │    (HTML)    │           │
│  └──────────────┘  └──────────────┘           │
├─────────────────────────────────────────────────┤
│  Business Logic Layer                           │
│  ┌──────────────┐  ┌──────────────┐           │
│  │   Services   │  │     State    │           │
│  │     (DI)     │  │  Management  │           │
│  └──────────────┘  └──────────────┘           │
├─────────────────────────────────────────────────┤
│  Infrastructure Layer                           │
│  ┌──────────────┐  ┌──────────────┐           │
│  │ HTTP Client  │  │    Router    │           │
│  │  (REST API)  │  │  Navigation  │           │
│  └──────────────┘  └──────────────┘           │
├─────────────────────────────────────────────────┤
│  Cross-Cutting Concerns                         │
│  ┌──────────────┐  ┌──────────────┐           │
│  │ OpenTelemetry│  │ Error Handler│           │
│  │   (Browser)  │  │  Interceptor │           │
│  └──────────────┘  └──────────────┘           │
└─────────────────────────────────────────────────┘
```

**Key Characteristics:**
- **Component Model**: Standalone components (no NgModule)
- **Dependency Injection**: Built-in DI container
- **Reactive Programming**: RxJS for async operations
- **Routing**: Angular Router for navigation
- **HTTP Communication**: HttpClient with interceptors
- **State Management**: Service-based state (can add NgRx if needed)
- **Observability**: OpenTelemetry browser instrumentation

### Backend Architecture (.NET)

```
┌─────────────────────────────────────────────────┐
│           .NET Web API Application              │
├─────────────────────────────────────────────────┤
│  API Layer (Minimal APIs)                       │
│  ┌──────────────┐  ┌──────────────┐           │
│  │  Endpoints   │  │  Middleware  │           │
│  │  (MapGet,    │  │  (CORS,      │           │
│  │   MapPost)   │  │   Auth, etc) │           │
│  └──────────────┘  └──────────────┘           │
├─────────────────────────────────────────────────┤
│  Business Logic Layer                           │
│  ┌──────────────┐  ┌──────────────┐           │
│  │   Services   │  │    Models    │           │
│  │  (Injected)  │  │   (Records)  │           │
│  └──────────────┘  └──────────────┘           │
├─────────────────────────────────────────────────┤
│  Data Access Layer (Future)                     │
│  ┌──────────────┐  ┌──────────────┐           │
│  │ Repositories │  │  EF Core     │           │
│  │  (Pattern)   │  │  (ORM)       │           │
│  └──────────────┘  └──────────────┘           │
├─────────────────────────────────────────────────┤
│  Infrastructure Layer                           │
│  ┌──────────────┐  ┌──────────────┐           │
│  │ Configuration│  │    Logging   │           │
│  │  (Settings)  │  │   (Serilog)  │           │
│  └──────────────┘  └──────────────┘           │
├─────────────────────────────────────────────────┤
│  Cross-Cutting Concerns                         │
│  ┌──────────────┐  ┌──────────────┐           │
│  │OpenTelemetry │  │Health Checks │           │
│  │ (Metrics,    │  │  (Ready/     │           │
│  │  Traces)     │  │   Live)      │           │
│  └──────────────┘  └──────────────┘           │
└─────────────────────────────────────────────────┘
```

**Key Characteristics:**
- **API Pattern**: Minimal APIs for endpoints
- **Dependency Injection**: Built-in .NET DI
- **Configuration**: appsettings.json with environment overrides
- **Logging**: Serilog for structured logging
- **Health Checks**: Readiness and liveness probes
- **Observability**: OpenTelemetry for metrics and traces
- **API Documentation**: OpenAPI/Swagger

## Data Architecture

### Current State: Stateless

The application is currently **stateless** with no persistent data storage.

```
┌──────────────┐
│   Frontend   │
└──────┬───────┘
       │ HTTP Request
       │
┌──────▼───────┐
│   Backend    │
│              │
│  In-Memory   │
│     Data     │
└──────────────┘
```

### Future State: With Data Persistence

```
┌──────────────┐
│   Frontend   │
└──────┬───────┘
       │
┌──────▼───────┐      ┌──────────────┐
│   Backend    │◄────►│   Database   │
│              │      │  (SQL/NoSQL) │
│   Services   │      └──────────────┘
└──────┬───────┘
       │
       │ Cache
       ▼
┌──────────────┐
│    Redis     │
│   (Cache)    │
└──────────────┘
```

### Data Flow

**Read Operations:**
1. Client → Frontend
2. Frontend → Backend API
3. Backend → (Future: Database/Cache)
4. Backend → Frontend
5. Frontend → Client

**Write Operations:**
1. Client → Frontend
2. Frontend → Backend API (POST/PUT)
3. Backend → (Future: Validation)
4. Backend → (Future: Database)
5. Backend → (Future: Audit Log)
6. Backend → Frontend (Response)
7. Frontend → Client

### Observability Data

**Metrics Storage:**
- Prometheus (TSDB) - 90 days retention
- Aggregation by Grafana

**Logs Storage:**
- Loki - 30 days retention
- Index-free log aggregation

**Traces Storage:**
- Tempo - 7 days retention
- Distributed trace storage

## Security Architecture

### Defense in Depth

```
┌─────────────────────────────────────────┐
│         Layer 7: Physical/Cloud         │
└─────────────────┬───────────────────────┘
                  │
┌─────────────────▼───────────────────────┐
│      Layer 6: Network Security          │
│  - Firewalls  - Security Groups         │
└─────────────────┬───────────────────────┘
                  │
┌─────────────────▼───────────────────────┐
│      Layer 5: Transport Security        │
│  - HTTPS/TLS  - Certificate Mgmt        │
└─────────────────┬───────────────────────┘
                  │
┌─────────────────▼───────────────────────┐
│    Layer 4: Application Security        │
│  - Authentication  - Authorization      │
│  - Input Validation  - CORS             │
└─────────────────┬───────────────────────┘
                  │
┌─────────────────▼───────────────────────┐
│       Layer 3: Data Security            │
│  - Encryption at Rest                   │
│  - Data Classification                  │
└─────────────────┬───────────────────────┘
                  │
┌─────────────────▼───────────────────────┐
│      Layer 2: Container Security        │
│  - Image Scanning  - Runtime Security   │
└─────────────────┬───────────────────────┘
                  │
┌─────────────────▼───────────────────────┐
│      Layer 1: Code Security             │
│  - Static Analysis  - Dependency Scan   │
│  - Code Review  - Security Testing      │
└─────────────────────────────────────────┘
```

### Security Controls

**Current Implementation:**

1. **Transport Security**
   - HTTPS enforced in production
   - TLS 1.2+ minimum
   - Secure headers configured

2. **Application Security**
   - CORS configured appropriately
   - Input validation on API endpoints
   - No secrets in code (environment variables)

3. **Dependency Security**
   - Dependabot for automated updates
   - Security advisories monitored
   - Regular dependency updates

4. **Container Security**
   - Official base images
   - Minimal images
   - Regular image updates

5. **Development Security**
   - Code review required
   - No direct commits to main
   - Branch protection enabled

**Future Enhancements:**

1. **Authentication & Authorization**
   - JWT tokens
   - OAuth2/OIDC
   - Role-based access control (RBAC)

2. **API Security**
   - Rate limiting
   - API key management
   - Request throttling

3. **Data Security**
   - Encryption at rest
   - PII handling
   - Data loss prevention (DLP)

## Deployment Architecture

### Container Architecture

```
┌────────────────────────────────────────────┐
│           Docker Compose Stack              │
├────────────────────────────────────────────┤
│                                             │
│  ┌──────────┐  ┌──────────┐               │
│  │ Frontend │  │ Backend  │               │
│  │ (nginx)  │  │(.NET)    │               │
│  │  :4200   │  │  :5000   │               │
│  └────┬─────┘  └────┬─────┘               │
│       │             │                      │
│  ┌────┴─────────────┴─────┐               │
│  │   Observability Stack   │               │
│  ├─────────────────────────┤               │
│  │ ┌──────────┐           │               │
│  │ │ Grafana  │ :3000     │               │
│  │ └────┬─────┘           │               │
│  │      │                  │               │
│  │ ┌────┴──────┬────┬─────┘──┐           │
│  │ │           │    │        │           │
│  │ │Prometheus │Tempo│ Loki  │           │
│  │ │  :9090    │:3200│ :3100 │           │
│  │ └───────────┴────┴────────┘           │
│  └─────────────────────────────           │
│                                             │
└────────────────────────────────────────────┘
```

### Deployment Models

**Development:**
- Docker Compose on local machine
- Hot reload for both frontend and backend
- Full observability stack

**CI/CD:**
- GitHub Actions runners
- Build and test on every commit
- Container builds and publishing

**Production (Future):**
- Kubernetes cluster
- Horizontal pod autoscaling
- Rolling updates with zero downtime
- Health check-based readiness

### Scaling Strategy

**Horizontal Scaling:**

```
           ┌──────────────┐
           │Load Balancer │
           └──────┬───────┘
                  │
     ┌────────────┼────────────┐
     │            │            │
┌────▼────┐  ┌───▼────┐  ┌───▼────┐
│Backend-1│  │Backend-2│  │Backend-3│
└─────────┘  └────────┘  └─────────┘
     │            │            │
     └────────────┼────────────┘
                  │
          ┌───────▼────────┐
          │   Data Layer   │
          │  (If needed)   │
          └────────────────┘
```

**Vertical Scaling:**
- Increase container resources (CPU, memory)
- Optimize code performance
- Cache frequently accessed data

## Integration Architecture

### API Design

**REST API Principles:**
- Resource-based URLs
- HTTP verbs for operations
- Status codes for responses
- JSON for data exchange
- Versioning strategy (URL or header)

**Current Endpoints:**
```
GET  /weatherforecast        - Weather data
GET  /metrics                - Prometheus metrics
GET  /health                 - Health check
GET  /health/ready           - Readiness check
GET  /health/live            - Liveness check
GET  /api/metrics/custom     - Custom metrics info
GET  /api/trace/test         - Trace testing
GET  /api/error/test         - Error testing
```

**OpenAPI/Swagger:**
- Auto-generated from code
- Available in development mode
- Interactive API documentation

### Integration Patterns

**Current:**
- HTTP REST API
- CORS for cross-origin access
- JSON data format

**Future Considerations:**
- GraphQL for complex queries
- gRPC for service-to-service
- WebSockets for real-time
- Message queue for async processing

## Observability Architecture

### Three Pillars Implementation

```
┌──────────────────────────────────────┐
│          Application                 │
└───┬──────────────┬────────────────┬──┘
    │              │                │
    │ Metrics      │ Logs          │ Traces
    │              │                │
┌───▼────┐   ┌────▼─────┐    ┌────▼────┐
│Prometheus│  │   Loki   │    │  Tempo  │
│          │  │          │    │         │
│ - Counters│ │- Levels  │    │- Spans  │
│ - Gauges  │ │- Context │    │- Context│
│ - Histgrm│  │- Query   │    │- Sampling│
└───┬──────┘  └────┬─────┘    └────┬────┘
    │              │                │
    └──────────────┴────────────────┘
                   │
            ┌──────▼──────┐
            │   Grafana   │
            │             │
            │ - Dashboards│
            │ - Alerts    │
            │ - Explore   │
            └─────────────┘
```

### Instrumentation Strategy

**Backend (.NET):**
- OpenTelemetry SDK
- ASP.NET Core instrumentation
- HTTP client instrumentation
- Custom metrics (Meter API)
- Structured logging (Serilog)

**Frontend (Angular):**
- OpenTelemetry Browser SDK
- Document load instrumentation
- HTTP fetch instrumentation
- User interaction tracking
- Custom spans for key operations

### Monitoring Strategy

**Golden Signals:**
1. **Latency**: Request response time
2. **Traffic**: Request rate
3. **Errors**: Error rate
4. **Saturation**: Resource utilization

**Key Metrics:**
- Request rate (requests/second)
- Error rate (%)
- Response time (p50, p95, p99)
- Active requests
- CPU and memory usage

**Alerting (Future):**
- Error rate > 1%
- Response time p95 > 500ms
- CPU usage > 80%
- Memory usage > 85%
- Service unavailable

## Network Architecture

### Development Network

```
┌─────────────────────────────────────┐
│         Docker Network              │
│         (bridge mode)               │
├─────────────────────────────────────┤
│  frontend:4200                      │
│  backend:5000                       │
│  grafana:3000                       │
│  prometheus:9090                    │
│  tempo:3200                         │
│  loki:3100                          │
│  promtail:9080                      │
└─────────────────────────────────────┘
        │
        │ Port Forwarding
        │
┌───────▼─────────┐
│   Host:         │
│   localhost     │
└─────────────────┘
```

### Production Network (Future)

```
┌──────────────────────────────────────┐
│           Internet                    │
└────────────┬─────────────────────────┘
             │
┌────────────▼─────────────────────────┐
│        Load Balancer                  │
│        (Public Subnet)                │
└────────────┬─────────────────────────┘
             │
┌────────────▼─────────────────────────┐
│     Application Tier                  │
│     (Private Subnet)                  │
│  - Frontend Pods                      │
│  - Backend Pods                       │
└────────────┬─────────────────────────┘
             │
┌────────────▼─────────────────────────┐
│      Observability Tier               │
│      (Private Subnet)                 │
│  - Grafana                            │
│  - Prometheus                         │
│  - Tempo / Loki                       │
└────────────┬─────────────────────────┘
             │
┌────────────▼─────────────────────────┐
│         Data Tier                     │
│      (Private Subnet)                 │
│  - Database                           │
│  - Cache                              │
└───────────────────────────────────────┘
```

## Development Architecture

### Development Environment

**Local Development:**
- DevContainer with VS Code
- All dependencies pre-installed
- Consistent environment across team

**Development Tools:**
- Visual Studio Code
- .NET 9 SDK
- Node.js 20+ and npm
- Docker Desktop
- Git

### Development Workflow

```
┌──────────────┐
│   Developer  │
└──────┬───────┘
       │
       │ Code Changes
       │
┌──────▼───────┐
│  Local Env   │
│ (DevContainer)│
└──────┬───────┘
       │
       │ Commit
       │
┌──────▼───────┐
│   GitHub     │
│  Repository  │
└──────┬───────┘
       │
       │ Webhook
       │
┌──────▼───────┐
│ GitHub       │
│ Actions      │
└──────┬───────┘
       │
       │ Build/Test
       │
┌──────▼───────┐
│   Deploy     │
│ (if passing) │
└──────────────┘
```

## Scalability and Performance

### Performance Targets

| Metric | Target | Measurement |
|--------|--------|-------------|
| API Response Time (p95) | < 200ms | Prometheus |
| Page Load Time | < 2s | Browser metrics |
| Throughput | > 100 RPS | Load testing |
| Error Rate | < 0.1% | Application logs |
| Availability | 99.9% | Uptime monitoring |

### Scalability Strategies

**Application Layer:**
- Stateless design enables horizontal scaling
- Load balancing distributes traffic
- Auto-scaling based on CPU/memory

**Data Layer (Future):**
- Read replicas for read-heavy workloads
- Caching for frequently accessed data
- Database connection pooling

**Observability Layer:**
- Prometheus federation for multi-cluster
- Loki distributed mode for log volume
- Tempo distributed tracing at scale

### Performance Optimization

**Backend:**
- Async/await for I/O operations
- Connection pooling
- Response caching
- Efficient serialization

**Frontend:**
- Lazy loading routes
- Code splitting
- Tree shaking
- AOT compilation
- Service worker (PWA)

## References

- [Architecture Decision Records](./README.md)
- [Governance Framework](../GOVERNANCE.md)
- [Security Policy](../../SECURITY.md)
- [Observability Guide](../../observability/README.md)
- [Contributing Guide](../../CONTRIBUTING.md)

## Document Control

| Version | Date | Author | Changes |
|---------|------|--------|---------|
| 1.0.0 | 2025-10-25 | System | Initial architecture documentation |

---

**Last Updated**: 2025-10-25  
**Next Review Date**: 2026-01-25  
**Document Owner**: Technical Lead  
**Approvers**: Architecture Review Board
