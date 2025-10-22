# Grafana-banana Observability Stack

This directory contains the complete observability configuration for the Grafana-banana application, providing comprehensive monitoring, tracing, and logging capabilities.

## 🎯 Overview

The observability stack includes:

- **Grafana** - Visualization and dashboards (http://localhost:3000)
- **Prometheus** - Metrics collection and storage (http://localhost:9090)
- **Tempo** - Distributed tracing (http://localhost:3200)
- **Loki** - Log aggregation (http://localhost:3100)
- **Promtail** - Log shipper for Loki

## 🚀 Quick Start

### Starting the Observability Stack

From the project root directory:

```bash
# Start all services including observability stack
docker-compose up -d

# Or start only specific services
docker-compose up -d grafana prometheus tempo loki
```

### Accessing the Services

- **Grafana Dashboard**: http://localhost:3000
  - Username: `admin`
  - Password: `admin`
  - Pre-configured dashboards available in the "Grafana-banana" folder

- **Prometheus**: http://localhost:9090
  - Query metrics directly
  - View targets and service discovery

- **Tempo**: http://localhost:3200
  - Access trace data through Grafana

- **Loki**: http://localhost:3100
  - Access logs through Grafana

## 📊 Available Metrics

The .NET API exposes the following custom metrics at `/metrics`:

### Application Metrics

- `api_requests_total` - Total number of API requests (Counter)
  - Labels: `endpoint`
  
- `api_requests_active` - Number of currently active requests (UpDownCounter)

- `api_request_duration_ms` - Request duration in milliseconds (Histogram)
  - Labels: `endpoint`, `status`

- `weather_forecast_requests` - Number of weather forecast requests (Counter)

### .NET Runtime Metrics

- `process_working_set_bytes` - Process working set memory
- `dotnet_total_memory_bytes` - Total .NET memory usage
- `process_cpu_seconds_total` - Total CPU time
- `dotnet_gc_collections_total` - Garbage collection count
- `dotnet_exceptions_total` - Total exceptions thrown
- `dotnet_threadpool_*` - Thread pool metrics

### ASP.NET Core Metrics

- `http_server_request_duration` - HTTP request duration
- `http_server_active_requests` - Active HTTP requests
- `kestrel_*` - Kestrel server metrics

## 🔍 Distributed Tracing

### Backend (C# .NET)

The API automatically traces:
- HTTP requests (incoming and outgoing)
- Custom spans for specific operations
- Exception tracking

Custom traces can be viewed at:
- `/api/trace/test` - Test tracing endpoint

### Frontend (Angular)

The Angular application automatically traces:
- Page loads and navigation
- User interactions (clicks, form submissions)
- HTTP requests to the backend
- Custom application events

## 📝 Logging

### Log Levels

- **Information** - Normal application flow
- **Warning** - Unexpected events that don't prevent operation
- **Error** - Errors and exceptions

### Log Locations

- **Console Output** - Structured JSON logs
- **File Logs** - `backend/GrafanaBanana.Api/logs/grafana-banana-*.log`
- **Loki** - Centralized log aggregation

### Log Enrichment

All logs include:
- Timestamp
- Log level
- Source context
- Machine name
- Thread ID
- Request details (when applicable)
- Trace correlation IDs

## 🏥 Health Checks

Health check endpoints:

- `/health` - Overall application health with detailed status
- `/health/ready` - Readiness probe (for Kubernetes/orchestration)
- `/health/live` - Liveness probe

Example health check response:
```json
{
  "status": "Healthy",
  "totalDuration": "00:00:00.0123456",
  "entries": {
    "self": {
      "status": "Healthy",
      "description": "API is running"
    },
    "weather_service": {
      "status": "Healthy",
      "description": "Weather service is available"
    }
  }
}
```

## 📈 Pre-configured Dashboards

### API Observability Dashboard

Located at: `observability/grafana/dashboards/api-observability.json`

Panels include:
1. **API Request Rate** - Requests per second by endpoint
2. **P95 Request Duration** - 95th percentile response time
3. **Active Requests** - Real-time active request count
4. **Memory Usage** - Process and .NET memory consumption
5. **Application Logs** - Real-time log streaming

## 🔧 Configuration

### Prometheus Configuration

- File: `observability/prometheus/prometheus.yml`
- Scrape interval: 15 seconds
- Targets: API, Tempo, Loki, Grafana

### Tempo Configuration

- File: `observability/tempo/tempo.yaml`
- OTLP receivers: gRPC (4317), HTTP (4318)
- Storage: Local filesystem
- Retention: 1 hour

### Loki Configuration

- File: `observability/loki/loki-config.yaml`
- Schema: v13 (TSDB)
- Storage: Filesystem

### Promtail Configuration

- File: `observability/promtail/promtail-config.yaml`
- Scrapes: Application log files
- Pushes to: Loki

## 🎨 Grafana Data Sources

All data sources are automatically provisioned:

1. **Prometheus** (Default)
   - URL: http://prometheus:9090
   - Type: Metrics

2. **Tempo**
   - URL: http://tempo:3200
   - Type: Traces
   - Integrated with: Loki (trace to logs), Prometheus (trace to metrics)

3. **Loki**
   - URL: http://loki:3100
   - Type: Logs
   - Integrated with: Tempo (log to traces)

## 🔍 Querying Examples

### Prometheus Queries

```promql
# Request rate
rate(api_requests_total[5m])

# 95th percentile request duration
histogram_quantile(0.95, rate(api_request_duration_ms_bucket[5m]))

# Active requests
api_requests_active

# Memory usage
process_working_set_bytes

# Error rate
rate(http_server_request_duration_count{http_response_status_code=~"5.."}[5m])
```

### LogQL Queries (Loki)

```logql
# All logs from the API
{service="grafana-banana-api"}

# Error logs only
{service="grafana-banana-api"} |= "Error"

# Logs with specific endpoint
{service="grafana-banana-api"} | json | endpoint="/weatherforecast"

# Rate of errors
rate({service="grafana-banana-api"} |= "Error" [5m])
```

## 🐛 Troubleshooting

### Service Not Starting

```bash
# Check service logs
docker-compose logs -f <service-name>

# Restart a specific service
docker-compose restart <service-name>
```

### No Metrics Appearing

1. Check Prometheus targets: http://localhost:9090/targets
2. Verify the backend is running and exposing `/metrics`
3. Check network connectivity between containers

### No Traces Appearing

1. Verify Tempo is receiving data: http://localhost:3200/ready
2. Check OTLP endpoint configuration in the application
3. Verify network connectivity to Tempo (port 4317/4318)

### No Logs Appearing

1. Check Promtail is running: `docker-compose logs promtail`
2. Verify log files exist in `backend/GrafanaBanana.Api/logs/`
3. Check Loki status: http://localhost:3100/ready

## 📚 Additional Resources

- [OpenTelemetry Documentation](https://opentelemetry.io/docs/)
- [Prometheus Documentation](https://prometheus.io/docs/)
- [Grafana Tempo Documentation](https://grafana.com/docs/tempo/)
- [Grafana Loki Documentation](https://grafana.com/docs/loki/)
- [Grafana Dashboard Documentation](https://grafana.com/docs/grafana/)

## 🔄 Updating Configuration

After modifying configuration files:

```bash
# Restart affected services
docker-compose restart grafana prometheus tempo loki promtail

# Or recreate containers to pick up volume changes
docker-compose up -d --force-recreate
```

## 🧹 Cleanup

To remove all observability data:

```bash
# Stop and remove containers and volumes
docker-compose down -v

# Remove only data volumes
docker volume rm grafana-banana_prometheus_data
docker volume rm grafana-banana_tempo_data
docker volume rm grafana-banana_loki_data
docker volume rm grafana-banana_grafana_data
```
