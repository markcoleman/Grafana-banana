.PHONY: help install build test clean run-backend run-frontend dev lint

help: ## Show this help message
	@echo 'Usage: make [target]'
	@echo ''
	@echo 'Available targets:'
	@awk 'BEGIN {FS = ":.*?## "} /^[a-zA-Z_-]+:.*?## / {printf "  %-15s %s\n", $$1, $$2}' $(MAKEFILE_LIST)

install: ## Install all dependencies
	@echo "Installing backend dependencies..."
	cd backend/GrafanaBanana.Api && dotnet restore
	@echo "Installing frontend dependencies..."
	cd frontend && npm install
	@echo "✓ All dependencies installed"

build: ## Build both backend and frontend
	@echo "Building backend..."
	cd backend/GrafanaBanana.Api && dotnet build --configuration Release
	@echo "Building frontend..."
	cd frontend && npm run build
	@echo "✓ Build complete"

test: ## Run all tests
	@echo "Running backend tests..."
	cd backend/GrafanaBanana.Api && dotnet test
	@echo "Running frontend tests..."
	cd frontend && npm test -- --watch=false --browsers=ChromeHeadless
	@echo "✓ All tests passed"

lint: ## Run frontend linting
	@echo "Running ESLint..."
	cd frontend && npm run lint
	@echo "✓ Linting complete"

clean: ## Clean build artifacts
	@echo "Cleaning backend..."
	cd backend/GrafanaBanana.Api && dotnet clean
	@echo "Cleaning frontend..."
	cd frontend && rm -rf dist/ .angular/
	@echo "✓ Clean complete"

run-backend: ## Run the backend API
	@echo "Starting backend API on http://localhost:5000..."
	cd backend/GrafanaBanana.Api && dotnet run

run-frontend: ## Run the frontend application
	@echo "Starting frontend on http://localhost:4200..."
	cd frontend && npm start

dev: ## Run both backend and frontend (requires two terminals)
	@echo "To run in development mode, open two terminals:"
	@echo "  Terminal 1: make run-backend"
	@echo "  Terminal 2: make run-frontend"
	@echo ""
	@echo "Or use the helper scripts:"
	@echo "  Terminal 1: ./start-backend.sh"
	@echo "  Terminal 2: ./start-frontend.sh"
