# Contributing to Grafana-banana

Thank you for your interest in contributing to Grafana-banana! This document provides guidelines and instructions for setting up your development environment.

## Prerequisites

- [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- [Node.js 20+](https://nodejs.org/)
- [Git](https://git-scm.com/)
- [Visual Studio Code](https://code.visualstudio.com/) (recommended)

## Getting Started

### Option 1: Using DevContainer (Recommended)

The easiest way to get started is using the provided DevContainer:

1. Install [Docker Desktop](https://www.docker.com/products/docker-desktop)
2. Install the [Dev Containers extension](https://marketplace.visualstudio.com/items?itemName=ms-vscode-remote.remote-containers) for VS Code
3. Clone the repository:
   ```bash
   git clone https://github.com/markcoleman/Grafana-banana.git
   cd Grafana-banana
   ```
4. Open the project in VS Code
5. Press `F1` and select "Dev Containers: Reopen in Container"
6. Wait for the container to build and start

The DevContainer will automatically:
- Install .NET 9 SDK
- Install Node.js 20
- Install all dependencies
- Configure VS Code with recommended extensions

### Option 2: Local Setup

If you prefer to work locally:

1. Clone the repository:
   ```bash
   git clone https://github.com/markcoleman/Grafana-banana.git
   cd Grafana-banana
   ```

2. Install backend dependencies:
   ```bash
   cd backend/GrafanaBanana.Api
   dotnet restore
   ```

3. Install frontend dependencies:
   ```bash
   cd ../../frontend
   npm install
   ```

## Running the Application

### Running Backend and Frontend Separately

**Terminal 1 - Backend:**
```bash
./start-backend.sh
# Or manually:
cd backend/GrafanaBanana.Api
dotnet run
```

The API will be available at http://localhost:5000

**Terminal 2 - Frontend:**
```bash
./start-frontend.sh
# Or manually:
cd frontend
npm start
```

The frontend will be available at http://localhost:4200

### Running Both Together

You can use two terminal windows/tabs to run both services simultaneously. The frontend will automatically connect to the backend API.

## Development Workflow

### Making Changes

1. Create a new branch:
   ```bash
   git checkout -b feature/your-feature-name
   ```

2. Make your changes to the code

3. Test your changes:
   ```bash
   # Backend tests
   cd backend/GrafanaBanana.Api
   dotnet test
   
   # Frontend tests
   cd frontend
   npm test
   ```

4. Build to ensure everything compiles:
   ```bash
   # Backend
   cd backend/GrafanaBanana.Api
   dotnet build
   
   # Frontend
   cd frontend
   npm run build
   ```

5. Commit your changes:
   ```bash
   git add .
   git commit -m "Description of your changes"
   ```

6. Push to GitHub:
   ```bash
   git push origin feature/your-feature-name
   ```

7. Create a Pull Request on GitHub

## Code Style

### Backend (.NET)

- Follow standard C# naming conventions
- Use meaningful variable and method names
- Add XML documentation comments for public APIs
- Keep methods small and focused

### Frontend (Angular)

- Follow [Angular Style Guide](https://angular.io/guide/styleguide)
- Use TypeScript strict mode
- Write component tests for new features
- Keep components focused on a single responsibility

## Testing

### Backend Tests

```bash
cd backend/GrafanaBanana.Api
dotnet test
```

### Frontend Tests

```bash
cd frontend

# Run tests once
npm test -- --watch=false --browsers=ChromeHeadless

# Run tests in watch mode
npm test

# Run linting
npm run lint
```

## Building for Production

### Backend

```bash
cd backend/GrafanaBanana.Api
dotnet build --configuration Release
dotnet publish --configuration Release --output ./publish
```

### Frontend

```bash
cd frontend
npm run build
```

The production build will be in `frontend/dist/frontend`

## Continuous Integration

This project uses GitHub Actions for CI/CD. Every push and pull request will:

1. Build the .NET API
2. Run backend tests
3. Build the Angular frontend
4. Run frontend linting
5. Run frontend tests

Make sure all checks pass before merging your pull request.

## Project Structure

```
Grafana-banana/
├── .devcontainer/          # DevContainer configuration
│   └── devcontainer.json
├── .github/
│   └── workflows/          # GitHub Actions CI/CD
│       └── ci.yml
├── backend/
│   └── GrafanaBanana.Api/  # .NET Web API
│       ├── Program.cs      # Main entry point
│       ├── Properties/
│       └── appsettings.json
├── frontend/               # Angular application
│   ├── src/
│   │   ├── app/           # App components
│   │   └── environments/  # Environment configs
│   ├── angular.json       # Angular configuration
│   └── package.json       # npm dependencies
├── start-backend.sh       # Helper script for backend
├── start-frontend.sh      # Helper script for frontend
└── README.md
```

## Need Help?

- Check the [README.md](README.md) for general information
- Review existing issues on GitHub
- Create a new issue if you find a bug or have a feature request

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.
