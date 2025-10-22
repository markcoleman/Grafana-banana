# Grafana-banana

A full-stack web application with .NET Web API backend and Angular frontend.

## 🏗️ Project Structure

```
Grafana-banana/
├── backend/
│   └── GrafanaBanana.Api/          # .NET 9 Web API
├── frontend/                        # Angular application
├── .devcontainer/                   # Dev container configuration
└── .github/workflows/               # GitHub Actions CI/CD
```

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

## 🔄 Continuous Integration

This project uses GitHub Actions for CI/CD. The workflow runs on:
- Push to `main` or `develop` branches
- Pull requests to `main` or `develop` branches
- Manual workflow dispatch

The CI pipeline:
- ✅ Builds the .NET API
- ✅ Runs backend tests
- ✅ Builds the Angular frontend
- ✅ Runs linting checks
- ✅ Runs frontend tests

## 📝 Available Scripts

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

**Frontend:**
- Angular 19
- TypeScript
- CSS

**DevOps:**
- GitHub Actions
- Dev Containers
- Docker

## 📖 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.
