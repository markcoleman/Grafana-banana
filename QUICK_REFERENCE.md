# Grafana-banana Observability Quick Reference

## 🚀 Quick Start Commands

```bash
# Start everything
docker-compose up -d

# View logs
docker-compose logs -f

# Stop everything
docker-compose down

# Rebuild and start
docker-compose up -d --build
```

## 🌐 Service URLs

| Service | URL | Login |
|---------|-----|-------|
| **Application** |
| Frontend | http://localhost:4200 | - |
| Backend API | http://localhost:5000 | - |
| **Observability** |
| Grafana | http://localhost:3000 | admin/admin |
| Prometheus | http://localhost:9090 | - |
| Tempo | http://localhost:3200 | - |
| Loki | http://localhost:3100 | - |
| **Endpoints** |
| Metrics | http://localhost:5000/metrics | - |
| Health | http://localhost:5000/health | - |
| Weather | http://localhost:5000/weatherforecast | - |
| Trace Test | http://localhost:5000/api/trace/test | - |

## 📊 Common Prometheus Queries

```promql
# Request rate
rate(api_requests_total[5m])

# Request rate by endpoint
sum by(endpoint) (rate(api_requests_total[5m]))

# P95 latency
histogram_quantile(0.95, rate(api_request_duration_ms_bucket[5m]))

# P99 latency
histogram_quantile(0.99, rate(api_request_duration_ms_bucket[5m]))

# Active requests
api_requests_active

# Memory usage (MB)
process_working_set_bytes / 1024 / 1024

# .NET memory (MB)
dotnet_total_memory_bytes / 1024 / 1024

# Error rate
rate(http_server_request_duration_count{http_response_status_code=~"5.."}[5m])

# Success rate
rate(http_server_request_duration_count{http_response_status_code=~"2.."}[5m])

# CPU usage
rate(process_cpu_seconds_total[5m])

# GC collections
rate(process_runtime_dotnet_gc_collections_count[5m])
```

## 📝 Common Loki Queries (LogQL)

```logql
# All API logs
{service="grafana-banana-api"}

# Error logs
{service="grafana-banana-api"} |= "Error"

# Errors and warnings
{service="grafana-banana-api"} |~ "Error|Warning"

# Specific endpoint
{service="grafana-banana-api"} | json | endpoint="/weatherforecast"

# By log level
{service="grafana-banana-api"} | json | Level="Error"

# Error rate
rate({service="grafana-banana-api"} |= "Error" [5m])

# Pattern matching
{service="grafana-banana-api"} |~ "HTTP.*responded (4|5)\\d{2}"

# Exclude healthchecks
{service="grafana-banana-api"} != "health"

# Last 100 entries
{service="grafana-banana-api"} | tail 100
```

## 🔍 Tempo Trace Queries

```
# By service
service.name="grafana-banana-api"

# By operation
name="GET /weatherforecast"

# By duration (> 100ms)
duration > 100ms

# By status code
http.status_code = 500

# Combined
service.name="grafana-banana-api" && duration > 50ms
```

## 🏥 Health Check Commands

```bash
# Basic health
curl http://localhost:5000/health

# Readiness (for K8s)
curl http://localhost:5000/health/ready

# Liveness (for K8s)
curl http://localhost:5000/health/live

# Pretty print
curl http://localhost:5000/health | jq
```

## 📈 Generate Test Data

```bash
# Single request
curl http://localhost:5000/weatherforecast

# Load test (100 requests)
for i in {1..100}; do
  curl -s http://localhost:5000/weatherforecast > /dev/null
  sleep 0.1
done

# Concurrent load (using Apache Bench)
ab -n 1000 -c 10 http://localhost:5000/weatherforecast

# Test tracing
curl http://localhost:5000/api/trace/test

# Test errors
curl http://localhost:5000/api/error/test
```

## 🔧 Docker Compose Commands

```bash
# Start specific services
docker-compose up -d grafana prometheus

# View logs for a service
docker-compose logs -f grafana

# Restart a service
docker-compose restart grafana

# Stop specific service
docker-compose stop tempo

# Remove volumes (clean slate)
docker-compose down -v

# Check service status
docker-compose ps

# Scale a service
docker-compose up -d --scale backend=3
```

## 📦 Package Versions

### Backend (.NET 9)
- OpenTelemetry.Exporter.OpenTelemetryProtocol: 1.10.0
- OpenTelemetry.Exporter.Prometheus.AspNetCore: 1.10.0-beta.1
- OpenTelemetry.Instrumentation.AspNetCore: 1.10.0
- OpenTelemetry.Instrumentation.Http: 1.10.0
- OpenTelemetry.Instrumentation.Runtime: 1.10.0
- Serilog.AspNetCore: 9.0.0
- AspNetCore.HealthChecks.UI.Client: 9.0.0

### Frontend (Angular 20)
- @opentelemetry/api: Latest
- @opentelemetry/sdk-trace-web: Latest
- @opentelemetry/instrumentation-*: Latest
- @opentelemetry/exporter-trace-otlp-http: Latest

## 🎯 Key Metrics to Monitor

### Application Health
- `api_requests_active` - Should be stable
- `http_server_request_duration` - Should be low
- `process_working_set_bytes` - Should be stable
- Error rate - Should be near zero

### Performance
- P95 latency < 100ms (adjust for your SLA)
- P99 latency < 500ms
- Request rate - matches expected traffic
- Active requests - proportional to traffic

### Resource Usage
- Memory - stable, no leaks
- CPU - under 80% sustained
- GC collections - not excessive
- Thread pool - not exhausted

## 🚨 Common Issues

### Port Already in Use
```bash
# Find and kill process on port 5000
lsof -ti:5000 | xargs kill -9

# Or use different port
ASPNETCORE_URLS=http://localhost:5555 dotnet run
```

### Grafana Can't Connect to Datasources
```bash
# Check if services are running
docker-compose ps

# Check service logs
docker-compose logs prometheus tempo loki

# Restart Grafana
docker-compose restart grafana
```

### No Metrics Appearing
1. Check Prometheus targets: http://localhost:9090/targets
2. Verify backend is exposing metrics: http://localhost:5000/metrics
3. Check Prometheus config: `observability/prometheus/prometheus.yml`

### No Traces Appearing
1. Check Tempo is running: http://localhost:3200/ready
2. Verify OTLP endpoint in appsettings.json
3. Check backend logs for telemetry errors

### No Logs in Loki
1. Check Promtail is running: `docker-compose logs promtail`
2. Verify logs exist: `ls backend/GrafanaBanana.Api/logs/`
3. Check Promtail config: `observability/promtail/promtail-config.yaml`

## 📚 Documentation Files

- `README.md` - Project overview
- `observability/README.md` - Observability setup guide
- `TELEMETRY.md` - Comprehensive telemetry reference
- `OBSERVABILITY_SUMMARY.md` - Feature checklist
- `QUICK_REFERENCE.md` - This file

## 🎓 Learning Path

1. **Start Here**: README.md
2. **Setup**: observability/README.md
3. **Deep Dive**: TELEMETRY.md
4. **Reference**: QUICK_REFERENCE.md
5. **Verify**: OBSERVABILITY_SUMMARY.md

## 💡 Pro Tips

- Use Grafana's Explore feature to investigate issues
- Create alerts based on metrics
- Use log sampling in production
- Configure retention policies
- Set up dashboards for different teams
- Use variables in Grafana dashboards
- Create custom annotations for deployments
- Export and version control dashboards
- Set up notification channels
- Use Grafana folders to organize dashboards

## 🔗 Useful Links

- [OpenTelemetry Docs](https://opentelemetry.io/docs/)
- [Prometheus Query Examples](https://prometheus.io/docs/prometheus/latest/querying/examples/)
- [LogQL Docs](https://grafana.com/docs/loki/latest/logql/)
- [Grafana Dashboard Gallery](https://grafana.com/grafana/dashboards/)
- [ASP.NET Core OpenTelemetry](https://learn.microsoft.com/en-us/dotnet/core/diagnostics/observability-with-otel)
