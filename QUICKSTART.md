# Quick Start Guide

Get the Grafana-banana application up and running in minutes!

## 🚀 Fastest Way to Start (Using DevContainer)

1. **Prerequisites:**
   - [Docker Desktop](https://www.docker.com/products/docker-desktop)
   - [Visual Studio Code](https://code.visualstudio.com/)
   - [Dev Containers extension](https://marketplace.visualstudio.com/items?itemName=ms-vscode-remote.remote-containers)

2. **Clone and Open:**
   ```bash
   git clone https://github.com/markcoleman/Grafana-banana.git
   cd Grafana-banana
   code .
   ```

3. **Reopen in Container:**
   - Press `F1` in VS Code
   - Select "Dev Containers: Reopen in Container"
   - Wait for the container to build (first time takes ~5 minutes)

4. **Start the Application:**

   Open two terminals in VS Code:

   **Terminal 1 - Backend:**
   ```bash
   ./start-backend.sh
   ```

   **Terminal 2 - Frontend:**
   ```bash
   ./start-frontend.sh
   ```

5. **Access the Application:**
   - Frontend: http://localhost:4200
   - Backend API: http://localhost:5000

That's it! You're ready to develop! 🎉

---

## 💻 Manual Setup (Without DevContainer)

### Prerequisites

- [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- [Node.js 20+](https://nodejs.org/)

### Steps

1. **Clone the Repository:**
   ```bash
   git clone https://github.com/markcoleman/Grafana-banana.git
   cd Grafana-banana
   ```

2. **Install Backend Dependencies:**
   ```bash
   cd backend/GrafanaBanana.Api
   dotnet restore
   cd ../..
   ```

3. **Install Frontend Dependencies:**
   ```bash
   cd frontend
   npm install
   cd ..
   ```

4. **Start Backend (Terminal 1):**
   ```bash
   ./start-backend.sh
   # Or manually:
   cd backend/GrafanaBanana.Api
   dotnet run
   ```

5. **Start Frontend (Terminal 2):**
   ```bash
   ./start-frontend.sh
   # Or manually:
   cd frontend
   npm start
   ```

6. **Access the Application:**
   - Frontend: http://localhost:4200
   - Backend API: http://localhost:5000/weatherforecast

---

## 🧪 Running Tests

### Backend Tests
```bash
cd backend/GrafanaBanana.Api
dotnet test
```

### Frontend Tests
```bash
cd frontend
npm test
```

### Frontend Linting
```bash
cd frontend
npm run lint
```

---

## 🏗️ Building for Production

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

The production build will be in `frontend/dist/frontend/`

---

## 📖 Next Steps

- Read the [README.md](README.md) for detailed documentation
- Check [CONTRIBUTING.md](CONTRIBUTING.md) for development guidelines
- Explore the code and make changes!

---

## ❓ Troubleshooting

### Port Already in Use

If you get an error that port 5000 or 4200 is already in use:

**Backend (Port 5000):**
```bash
# Find and kill the process
lsof -ti:5000 | xargs kill -9
# Or on Windows:
netstat -ano | findstr :5000
taskkill /PID <PID> /F
```

**Frontend (Port 4200):**
```bash
# Find and kill the process
lsof -ti:4200 | xargs kill -9
# Or on Windows:
netstat -ano | findstr :4200
taskkill /PID <PID> /F
```

### Frontend Can't Connect to Backend

Make sure the backend is running on http://localhost:5000 before starting the frontend.

### Build Errors

Try cleaning and rebuilding:

**Backend:**
```bash
cd backend/GrafanaBanana.Api
dotnet clean
dotnet restore
dotnet build
```

**Frontend:**
```bash
cd frontend
rm -rf node_modules package-lock.json
npm install
```

---

## 🎯 What You'll See

The application displays a weather forecast fetched from the .NET Web API backend. The data flows:

1. Angular frontend loads (http://localhost:4200)
2. Frontend calls the API (http://localhost:5000/weatherforecast)
3. Backend returns 5 days of weather forecast data
4. Frontend displays the data in colorful cards

This demonstrates a complete full-stack application with:
- ✅ .NET Web API backend
- ✅ Angular frontend
- ✅ CORS configuration
- ✅ HTTP communication
- ✅ Modern Angular patterns (signals, inject)
- ✅ TypeScript strict mode
- ✅ ESLint configuration
- ✅ Unit tests

Happy coding! 🚀
