# Telemetry and Observability Guide

This document provides comprehensive information about the telemetry and observability features implemented in Grafana-banana.

## 📊 Overview

Grafana-banana implements a complete observability stack following the **three pillars of observability**:

1. **Metrics** - Quantitative measurements of system behavior
2. **Traces** - Request flow through distributed systems
3. **Logs** - Timestamped records of discrete events

## 🔧 Technology Stack

### Backend (.NET 9)
- **OpenTelemetry SDK** - Vendor-neutral observability framework
- **Serilog** - Structured logging library
- **Prometheus Exporter** - Metrics exposition
- **OTLP Exporter** - Sends traces to Tempo

### Frontend (Angular 20)
- **OpenTelemetry Browser SDK** - Client-side instrumentation
- **Automatic Instrumentation** - Page loads, user interactions, XHR/Fetch

### Observability Platform
- **Grafana** - Unified observability dashboard
- **Prometheus** - Time-series metrics database
- **Tempo** - Distributed tracing backend
- **Loki** - Log aggregation system
- **Promtail** - Log collector and forwarder

## 📈 Metrics

### Backend Metrics

#### Custom Application Metrics

| Metric Name | Type | Description | Labels |
|------------|------|-------------|---------|
| `api_requests_total` | Counter | Total number of API requests | `endpoint` |
| `api_requests_active` | UpDownCounter | Currently active requests | - |
| `api_request_duration_ms` | Histogram | Request duration in milliseconds | `endpoint`, `status` |
| `weather_forecast_requests` | Counter | Weather forecast endpoint calls | - |

#### Automatic ASP.NET Core Metrics

- `http_server_request_duration` - HTTP request duration
- `http_server_active_requests` - Active HTTP server requests
- `kestrel_connection_duration` - Kestrel connection duration
- `kestrel_active_connections` - Active Kestrel connections
- `kestrel_queued_connections` - Queued connections
- `kestrel_total_connections` - Total connections

#### .NET Runtime Metrics

- `process_runtime_dotnet_gc_collections_count` - GC collections
- `process_runtime_dotnet_gc_objects_size_bytes` - Object size in heap
- `process_runtime_dotnet_gc_allocations_size_bytes` - Allocated bytes
- `process_runtime_dotnet_thread_pool_*` - Thread pool metrics
- `process_runtime_dotnet_exceptions_count` - Exception count
- `process_cpu_seconds_total` - Total CPU time
- `process_working_set_bytes` - Working set memory
- `dotnet_total_memory_bytes` - Total .NET memory

### Frontend Metrics

Automatically collected by OpenTelemetry Browser SDK:
- Document load timing
- Resource fetch timing
- User interaction timing
- Long tasks
- Layout shifts

## 🔍 Distributed Tracing

### Trace Attributes

Every trace includes:
- **Service Name** - `grafana-banana-api` or `grafana-banana-frontend`
- **Service Version** - Application version
- **Deployment Environment** - `development`, `staging`, `production`
- **Host Name** - Machine/container name
- **Trace ID** - Unique identifier for the entire request flow
- **Span ID** - Unique identifier for each operation

### Backend Span Attributes

HTTP Server spans include:
- `http.method` - HTTP method (GET, POST, etc.)
- `http.url` - Full URL
- `http.target` - Request target
- `http.status_code` - HTTP status code
- `http.request.user_agent` - User agent
- `http.request.content_length` - Request size
- `http.response.content_length` - Response size
- `net.peer.ip` - Client IP address
- Custom attributes per endpoint

### Frontend Span Attributes

Browser spans include:
- `component` - Component type (document-load, user-interaction, fetch)
- `http.url` - Request URL
- `http.method` - HTTP method
- `target.element` - DOM element for interactions
- `event.type` - Event type (click, submit, etc.)

### Trace Correlation

- Frontend traces are correlated with backend traces via W3C Trace Context headers
- Logs include trace IDs for correlation
- Metrics can be filtered by trace ID in Grafana

## 📝 Structured Logging

### Log Levels

- **Verbose** - Extremely detailed diagnostic information
- **Debug** - Detailed information for debugging
- **Information** - General informational messages
- **Warning** - Warning messages for unexpected but handled events
- **Error** - Error messages for failures
- **Fatal** - Critical failures that require immediate attention

### Log Enrichment

All logs are enriched with:
- **Timestamp** - ISO 8601 format with timezone
- **Level** - Log level
- **SourceContext** - Logger category name
- **MachineName** - Server/container name
- **ThreadId** - Thread identifier
- **RequestHost** - HTTP request host
- **RequestScheme** - HTTP or HTTPS
- **UserAgent** - Client user agent
- **RemoteIP** - Client IP address
- **TraceId** - Distributed trace identifier (when available)
- **SpanId** - Current span identifier (when available)

### Log Sinks

1. **Console** - Colored console output for development
2. **File** - Rolling file logs in `backend/GrafanaBanana.Api/logs/`
3. **Loki** - Centralized log aggregation (via Promtail)

### Log Format

JSON format for structured logging:
```json
{
  "Timestamp": "2025-10-22T01:00:00.000+00:00",
  "Level": "Information",
  "MessageTemplate": "HTTP {Method} {Path} responded {StatusCode} in {Elapsed} ms",
  "Properties": {
    "Method": "GET",
    "Path": "/weatherforecast",
    "StatusCode": 200,
    "Elapsed": 45.23,
    "SourceContext": "Microsoft.AspNetCore.Hosting.Diagnostics",
    "MachineName": "backend-container",
    "ThreadId": 15,
    "RequestHost": "localhost:5000",
    "RemoteIP": "172.18.0.1"
  }
}
```

## 🏥 Health Checks

### Endpoints

| Endpoint | Purpose | Use Case |
|----------|---------|----------|
| `/health` | Overall application health | Monitoring dashboards |
| `/health/ready` | Readiness probe | Kubernetes readiness |
| `/health/live` | Liveness probe | Kubernetes liveness |

### Health Check Response

```json
{
  "status": "Healthy",
  "totalDuration": "00:00:00.0123456",
  "entries": {
    "self": {
      "status": "Healthy",
      "description": "API is running",
      "duration": "00:00:00.0001234"
    },
    "weather_service": {
      "status": "Healthy",
      "description": "Weather service is available",
      "duration": "00:00:00.0002345"
    }
  }
}
```

### Health Status Values

- **Healthy** - All checks passed
- **Degraded** - Some checks returned degraded status
- **Unhealthy** - One or more checks failed

## 🎯 Using the Observability Stack

### Starting the Stack

```bash
# Start all services
docker-compose up -d

# Check status
docker-compose ps

# View logs
docker-compose logs -f grafana prometheus tempo loki
```

### Accessing Services

- **Grafana**: http://localhost:3000
  - Username: `admin`
  - Password: `admin`
  
- **Prometheus**: http://localhost:9090
- **Tempo**: http://localhost:3200
- **Loki**: http://localhost:3100

### Viewing Metrics

1. Open Grafana: http://localhost:3000
2. Navigate to "Dashboards" → "Grafana-banana API Observability"
3. View real-time metrics, traces, and logs

Or query Prometheus directly:
```
http://localhost:9090/graph
```

Example queries:
```promql
# Request rate
rate(api_requests_total[5m])

# 95th percentile latency
histogram_quantile(0.95, rate(api_request_duration_ms_bucket[5m]))

# Active requests
api_requests_active

# Memory usage
process_working_set_bytes / 1024 / 1024
```

### Viewing Traces

1. In Grafana, go to "Explore"
2. Select "Tempo" as the data source
3. Search for traces by:
   - Service name
   - Operation name
   - Duration
   - Tags

Or view Tempo directly:
```
http://localhost:3200
```

### Viewing Logs

1. In Grafana, go to "Explore"
2. Select "Loki" as the data source
3. Use LogQL queries:

```logql
# All logs from the API
{service="grafana-banana-api"}

# Error logs only
{service="grafana-banana-api"} |= "Error"

# Logs for a specific endpoint
{service="grafana-banana-api"} | json | endpoint="/weatherforecast"

# Filter by log level
{service="grafana-banana-api"} | json | Level="Error"
```

## 🔗 Trace-to-Logs-to-Metrics Correlation

The observability stack is configured for seamless correlation:

1. **From Traces to Logs**: Click "Logs for this span" in any trace
2. **From Logs to Traces**: Click the trace ID in any log entry
3. **From Metrics to Traces**: Click any metric spike to view related traces
4. **From Traces to Metrics**: View metrics for a specific service/endpoint

## 🧪 Testing Telemetry

### Generate Test Data

```bash
# Call the API to generate traces
curl http://localhost:5000/weatherforecast

# Test distributed tracing
curl http://localhost:5000/api/trace/test

# Generate custom metrics
for i in {1..100}; do
  curl http://localhost:5000/weatherforecast
  sleep 0.1
done

# Test error tracking
curl http://localhost:5000/api/error/test
```

### Verify Metrics

```bash
# Check Prometheus targets
curl http://localhost:9090/api/v1/targets

# Query a metric
curl 'http://localhost:9090/api/v1/query?query=api_requests_total'

# Check backend metrics endpoint
curl http://localhost:5000/metrics
```

### Verify Traces

```bash
# Check Tempo health
curl http://localhost:3200/ready

# View traces in Grafana
# Navigate to Explore → Tempo → Search
```

### Verify Logs

```bash
# Check Loki health
curl http://localhost:3100/ready

# Query logs directly
curl -G -s "http://localhost:3100/loki/api/v1/query_range" \
  --data-urlencode 'query={service="grafana-banana-api"}' \
  --data-urlencode 'start=1h'

# View in Grafana
# Navigate to Explore → Loki → Log browser
```

## 📚 Best Practices

### Metrics
- Use counters for events that only increase (requests, errors)
- Use gauges for values that go up and down (memory, connections)
- Use histograms for distributions (latency, size)
- Add meaningful labels but avoid high cardinality
- Keep metric names consistent and descriptive

### Tracing
- Create spans for significant operations
- Add relevant attributes to spans
- Record exceptions in spans
- Use consistent span names
- Link related spans properly

### Logging
- Use appropriate log levels
- Include structured data, not just strings
- Log at boundaries (entry/exit of methods)
- Include correlation IDs
- Don't log sensitive information
- Use log sampling for high-volume logs

### Performance
- Use sampling for traces in production
- Batch span exports
- Configure appropriate retention periods
- Monitor the observability stack itself
- Use async logging

## 🔒 Security Considerations

- Scrub sensitive data from logs and traces
- Secure Grafana with proper authentication
- Use TLS for production deployments
- Limit access to observability endpoints
- Rotate credentials regularly
- Monitor for anomalous patterns

## 📖 Additional Resources

- [OpenTelemetry Documentation](https://opentelemetry.io/docs/)
- [Prometheus Best Practices](https://prometheus.io/docs/practices/)
- [Grafana Tempo Documentation](https://grafana.com/docs/tempo/)
- [Grafana Loki Documentation](https://grafana.com/docs/loki/)
- [Serilog Documentation](https://serilog.net/)
