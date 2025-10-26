# Grafana-banana

A full-stack web application with .NET Web API backend and Angular frontend, featuring comprehensive observability with Grafana, Prometheus, Tempo, and Loki, plus fun Databricks integration for banana analytics! 🍌

Built using enterprise architecture patterns including Clean Architecture, CQRS, and Repository Pattern for maintainability and scalability.

> **🚀 [Quick Start Guide](QUICKSTART.md)** - Get up and running in minutes!
> 
> **🏛️ [Enterprise Architecture Patterns](docs/ENTERPRISE_ARCHITECTURE_PATTERNS.md)** - Clean Architecture, CQRS, Repository Pattern
> 
> **🤖 [Copilot Best Practices](.github/COPILOT_BEST_PRACTICES.md)** - Guide for using GitHub Copilot with enterprise patterns
> 
> **📝 [Copilot Code Examples](docs/COPILOT_CODE_EXAMPLES.md)** - Working code examples for all architecture patterns
> 
> **📊 [Architecture Visual Guide](docs/ARCHITECTURE_VISUAL_GUIDE.md)** - Diagrams and visual documentation
> 
> **🍌 [Databricks Integration](docs/DATABRICKS_INTEGRATION.md)** - Banana analytics powered by Databricks
> 
> **📊 [Observability Guide](observability/README.md)** - Complete monitoring and tracing documentation
> 
> **🏛️ [Governance Framework](docs/GOVERNANCE.md)** - Enterprise governance and compliance documentation
> 
> **🏗️ [Architecture Documentation](docs/architecture/TECHNICAL_ARCHITECTURE.md)** - Technical architecture overview
> 
> **📐 [Architecture Decision Records](docs/architecture/README.md)** - ADRs documenting key decisions
> 
> **🔄 [CI/CD Workflows](docs/CI_CD_WORKFLOWS.md)** - Automated releases and container publishing
> 
> **📸 [Screenshots](docs/screenshots/README.md)** - Visual documentation of the application
> 
> **📋 [Changelog](CHANGELOG.md)** - Release notes and version history

## 🏗️ Project Structure

```
Grafana-banana/
├── backend/
│   └── GrafanaBanana.Api/          # .NET 9 Web API with OpenTelemetry
├── frontend/                        # Angular application with web tracing
├── observability/                   # Complete Grafana stack configuration
│   ├── grafana/                    # Grafana dashboards and provisioning
│   ├── prometheus/                 # Metrics collection config
│   ├── tempo/                      # Distributed tracing config
│   ├── loki/                       # Log aggregation config
│   └── promtail/                   # Log shipper config
├── .devcontainer/                   # Dev container configuration
└── .github/workflows/               # GitHub Actions CI/CD
```

## ⭐ Key Features

### 🔍 Comprehensive Observability

- **Distributed Tracing** with OpenTelemetry and Tempo
- **Metrics Collection** with Prometheus
- **Log Aggregation** with Loki and Promtail
- **Unified Dashboards** in Grafana
- **Full-Stack Monitoring** (Backend + Frontend)

### 📊 Monitoring Capabilities

#### Backend (.NET)
- ✅ OpenTelemetry instrumentation for ASP.NET Core
- ✅ Custom business metrics (counters, histograms, gauges)
- ✅ Structured logging with Serilog
- ✅ Health check endpoints
- ✅ Prometheus metrics endpoint (`/metrics`)
- ✅ Runtime and performance metrics

#### Frontend (Angular)
- ✅ OpenTelemetry browser instrumentation
- ✅ Automatic page load tracking
- ✅ User interaction tracking
- ✅ HTTP request tracing
- ✅ Error tracking

### 🎯 Available Endpoints

- **API**: http://localhost:5000
- **Frontend**: http://localhost:4200
- **Grafana**: http://localhost:3000 (admin/admin)
- **Prometheus**: http://localhost:9090
- **Tempo**: http://localhost:3200
- **Loki**: http://localhost:3100

#### API Endpoints

- `GET /weatherforecast` - Weather forecast data with full tracing
- `GET /metrics` - Prometheus metrics endpoint
- `GET /health` - Detailed health check
- `GET /health/ready` - Readiness probe
- `GET /health/live` - Liveness probe
- `GET /api/metrics/custom` - Custom metrics info
- `GET /api/trace/test` - Test distributed tracing
- `GET /api/error/test` - Test error tracking
- `GET /api/databricks/banana-analytics` - 🍌 Banana analytics from Databricks
- `GET /api/databricks/production/{year}` - 🍌 Production data by year
- `GET /api/databricks/sales` - 🍌 Sales data by region

## 🚀 Getting Started

### Prerequisites

- [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- [Node.js 20+](https://nodejs.org/)
- [npm 10+](https://www.npmjs.com/)

Or use the provided devcontainer for a pre-configured development environment.

### Development with DevContainer

1. Open this repository in Visual Studio Code
2. Install the [Dev Containers extension](https://marketplace.visualstudio.com/items?itemName=ms-vscode-remote.remote-containers)
3. Click "Reopen in Container" when prompted (or use Command Palette: `Dev Containers: Reopen in Container`)
4. The environment will be automatically configured with all necessary tools

### Local Development Setup

#### Backend (.NET API)

```bash
# Navigate to the backend directory
cd backend/GrafanaBanana.Api

# Restore dependencies
dotnet restore

# Run the API
dotnet run
```

The API will be available at `http://localhost:5000`

#### Frontend (Angular)

```bash
# Navigate to the frontend directory
cd frontend

# Install dependencies
npm install

# Start development server
npm start
```

The frontend will be available at `http://localhost:4200`

#### Complete Stack with Docker Compose

Start the entire application with the full observability stack:

```bash
# Start all services (app + monitoring stack)
docker-compose up -d

# View logs
docker-compose logs -f

# Stop all services
docker-compose down
```

This starts:
- Backend API (port 5000)
- Frontend (port 4200)
- Grafana (port 3000)
- Prometheus (port 9090)
- Tempo (port 3200)
- Loki (port 3100)

## 🏗️ Building

### Build Backend

```bash
cd backend/GrafanaBanana.Api
dotnet build --configuration Release
```

### Build Frontend

```bash
cd frontend
npm run build
```

The production build will be in `frontend/dist/`

## 🧪 Testing

### Test Backend

```bash
cd backend/GrafanaBanana.Api
dotnet test
```

### Test Frontend

```bash
cd frontend
npm test
```

## 🔄 Continuous Integration & Deployment

This project uses GitHub Actions for automated CI/CD with multiple workflows:

### Automated Workflows

1. **Continuous Integration** (`ci.yml`)
   - Runs on every push and PR to `main`/`develop`
   - Builds and tests both backend and frontend
   - Validates code quality with linting

2. **Container Testing** (`test-containers.yml`)
   - Tests Docker builds on every PR
   - Validates containers start correctly
   - Runs smoke tests before merge

3. **Automatic Releases** (`publish-release.yml`)
   - Triggered on push to `main`
   - Builds and publishes Docker containers to GHCR
   - Creates GitHub releases with semantic versioning
   - Generates version based on conventional commits

4. **Manual Releases** (`release.yml`)
   - Supports manual version releases
   - Can republish specific versions

### Container Images

Published Docker images are available at GitHub Container Registry:

```bash
# Pull latest backend
docker pull ghcr.io/markcoleman/grafana-banana/backend:latest

# Pull latest frontend
docker pull ghcr.io/markcoleman/grafana-banana/frontend:latest

# Use specific version
docker pull ghcr.io/markcoleman/grafana-banana/backend:1.0.0
```

See [CI/CD Workflows Documentation](docs/CI_CD_WORKFLOWS.md) for complete details.

## 📝 Available Scripts

### Using Makefile (Recommended)

```bash
make help           # Show all available commands
make install        # Install all dependencies
make build          # Build both backend and frontend
make test           # Run all tests
make lint           # Run linting
make clean          # Clean build artifacts
make run-backend    # Run the backend API
make run-frontend   # Run the frontend app
```

### Backend

- `dotnet run` - Run the API in development mode
- `dotnet build` - Build the API
- `dotnet test` - Run tests

### Frontend

- `npm start` - Start development server
- `npm run build` - Build for production
- `npm test` - Run tests
- `npm run lint` - Run linter

## 🛠️ Tech Stack

**Backend:**
- .NET 9
- ASP.NET Core Web API
- Minimal APIs
- OpenTelemetry for tracing and metrics
- Serilog for structured logging
- Prometheus exporter

**Frontend:**
- Angular 20
- TypeScript
- OpenTelemetry for browser tracing
- CSS

**Observability:**
- Grafana - Visualization and dashboards
- Prometheus - Metrics collection
- Tempo - Distributed tracing
- Loki - Log aggregation
- Promtail - Log shipping

**DevOps:**
- GitHub Actions
- Dev Containers
- Docker & Docker Compose
- GitHub Container Registry (GHCR)

## 📊 Observability Features

### Metrics

The application exposes comprehensive metrics at `/metrics`:

**Application Metrics:**
- `api_requests_total` - Total API requests
- `api_requests_active` - Active requests
- `api_request_duration_ms` - Request duration histogram
- `weather_forecast_requests` - Weather endpoint requests

**Runtime Metrics:**
- .NET memory usage
- Garbage collection stats
- Thread pool metrics
- Exception counts
- CPU usage

### Distributed Tracing

Full distributed tracing from frontend to backend:
- Automatic HTTP request tracing
- Custom span creation
- Trace correlation across services
- Exception tracking in traces

### Structured Logging

All logs include:
- Timestamp and log level
- Source context
- Machine name and thread ID
- Request correlation
- Structured data for filtering

### Pre-configured Dashboards

Grafana dashboards are automatically provisioned with:
- API request rate and latency
- Memory and CPU usage
- Active requests
- Error rates
- Real-time log streaming

## 🔍 Monitoring Your Application

1. **Start the stack**: `docker-compose up -d`
2. **Open Grafana**: http://localhost:3000 (admin/admin)
3. **View the dashboard**: Navigate to "Grafana-banana API Observability"
4. **Generate some traffic**: Call the API endpoints
5. **Explore traces**: Click on any metric to see related traces
6. **View logs**: Check the logs panel for detailed application logs

For detailed monitoring instructions, see [observability/README.md](observability/README.md)

## 🐳 Docker Images

Pre-built Docker images are available on GitHub Container Registry for each release:

```bash
# Pull backend image
docker pull ghcr.io/markcoleman/grafana-banana/backend:latest

# Pull frontend image
docker pull ghcr.io/markcoleman/grafana-banana/frontend:latest

# Or pull a specific version
docker pull ghcr.io/markcoleman/grafana-banana/backend:0.1.0
docker pull ghcr.io/markcoleman/grafana-banana/frontend:0.1.0
```

### Using Pre-built Images

A pre-configured docker-compose file is available for using released images:

```bash
# Use pre-built images from GitHub Container Registry
docker-compose -f docker-compose.ghcr.yml up -d
```

Or modify the main `docker-compose.yml` to use pre-built images:

```yaml
services:
  backend:
    image: ghcr.io/markcoleman/grafana-banana/backend:latest
    # Remove the 'build' section
    
  frontend:
    image: ghcr.io/markcoleman/grafana-banana/frontend:latest
    # Remove the 'build' section
```

## 📦 Releases

Releases are managed through GitHub Releases. Each release:

- Automatically builds and publishes Docker images to GHCR
- Tags images with the version number and `latest`
- Includes release notes documenting changes
- See [CHANGELOG.md](CHANGELOG.md) for detailed version history

To create a new release, create a new tag and push it:

```bash
git tag -a v0.1.0 -m "Release version 0.1.0"
git push origin v0.1.0
```

Or create a release through the GitHub UI, which will trigger the automated container publishing workflow.

## 🏛️ Enterprise Governance

This project follows enterprise-grade governance practices suitable for corporate environments:

### Architecture & Decision Making

- **[Governance Framework](docs/GOVERNANCE.md)**: Comprehensive governance policies and processes
- **[Architecture Decision Records (ADRs)](docs/architecture/README.md)**: Documented architectural decisions
- **[Technical Architecture](docs/architecture/TECHNICAL_ARCHITECTURE.md)**: System architecture documentation

### Key Governance Areas

- **Decision Making**: Structured ADR process for architectural decisions
- **Security Governance**: Defense-in-depth security controls and policies
- **Data Governance**: Data classification, retention, and protection policies
- **Change Management**: Controlled change process with appropriate approvals
- **Risk Management**: Risk assessment and mitigation strategies
- **Quality Assurance**: Quality gates and standards for code and documentation
- **Compliance**: Adherence to industry standards (OWASP, CIS, NIST)
- **Metrics & KPIs**: Defined success metrics for development and operations

See the [Governance Framework](docs/GOVERNANCE.md) for complete documentation.

## 📸 Screenshots

Visual documentation and screenshots of the application are available in the [docs/screenshots](docs/screenshots/) directory:

- [Frontend Screenshots](docs/screenshots/frontend/) - Angular application UI
- [Backend Screenshots](docs/screenshots/backend/) - Web API documentation
- [Observability Screenshots](docs/screenshots/observability/) - Grafana dashboards and monitoring

Want to contribute screenshots? See the [Screenshots Contributing Guide](docs/CONTRIBUTING_SCREENSHOTS.md)!

## 📖 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.
