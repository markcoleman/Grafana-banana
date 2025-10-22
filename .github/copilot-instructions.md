# GitHub Copilot Instructions for Grafana-banana

## Project Overview

Grafana-banana is a full-stack web application featuring:

- **Backend**: .NET 9 Web API with minimal APIs pattern
- **Frontend**: Angular 20+ with TypeScript
- **Development Environment**: Dev container with all dependencies pre-configured
- **Architecture**: Clean separation between frontend and backend services

## Project Structure

```
Grafana-banana/
├── backend/GrafanaBanana.Api/      # .NET 9 Web API
│   ├── Program.cs                  # Main entry point, dependency injection, middleware
│   ├── appsettings.json           # Application configuration
│   └── GrafanaBanana.Api.csproj   # Project file with dependencies
├── frontend/                       # Angular application
│   ├── src/app/                   # Angular components and services
│   ├── package.json               # NPM dependencies and scripts
│   └── angular.json               # Angular CLI configuration
├── .devcontainer/                  # Dev container setup
├── docker-compose.yml              # Container orchestration
└── Makefile                        # Build and development commands
```

## Development Guidelines

### Backend (.NET Web API)

- **Framework**: .NET 9 with minimal APIs
- **Port**: Runs on http://localhost:5000
- **Entry Point**: `backend/GrafanaBanana.Api/Program.cs`
- **Pattern**: Use minimal API endpoints with `app.MapGet()`, `app.MapPost()`, etc.
- **CORS**: Pre-configured to allow Angular frontend (localhost:4200)
- **OpenAPI**: Swagger documentation available in development mode

#### Code Style:

```csharp
// Use minimal API pattern
app.MapGet("/api/endpoint", () =>
{
    // Implementation
    return Results.Ok(data);
})
.WithName("EndpointName")
.WithOpenApi();

// Use record types for DTOs
record ResponseDto(int Id, string Name, DateTime Created);
```

### Frontend (Angular)

- **Framework**: Angular 20+ with standalone components
- **Port**: Development server on http://localhost:4200
- **Entry Point**: `frontend/src/main.ts`
- **Components**: Use standalone components (no NgModule)
- **Styling**: CSS with component-level styles
- **HTTP**: Use Angular HttpClient for API communication

#### Code Style:

```typescript
// Standalone component pattern
@Component({
  selector: "app-component",
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: "./component.html",
  styleUrl: "./component.css",
})
export class ComponentName {}

// Services with dependency injection
@Injectable({
  providedIn: "root",
})
export class DataService {
  constructor(private http: HttpClient) {}
}
```

## Common Development Tasks

### Adding New API Endpoints

1. Add endpoint in `backend/GrafanaBanana.Api/Program.cs`
2. Use appropriate HTTP verb methods (`MapGet`, `MapPost`, `MapPut`, `MapDelete`)
3. Include `.WithName()` and `.WithOpenApi()` for documentation
4. Add CORS considerations if needed

### Adding New Angular Components

1. Create in `frontend/src/app/` directory
2. Use standalone component pattern
3. Import required Angular modules in the component
4. Update routing in `app.routes.ts` if needed

### API Communication

- Backend API base URL: `http://localhost:5000`
- Frontend development server: `http://localhost:4200`
- CORS is pre-configured for cross-origin requests
- Use Angular HttpClient for API calls

## Development Environment

### Dev Container Setup

- Pre-configured with .NET 9 SDK, Node.js, Angular CLI
- Port forwarding: 5000 (API), 4200 (Angular)
- Extensions: C# Dev Kit, Angular Language Service, ESLint

### Running the Application

```bash
# Start backend (from project root)
make start-backend
# or
cd backend/GrafanaBanana.Api && dotnet run

# Start frontend (from project root)
make start-frontend
# or
cd frontend && npm start
```

### Testing

```bash
# Backend tests
cd backend && dotnet test

# Frontend tests
cd frontend && npm test

# E2E tests
cd frontend && npm run e2e
```

## Coding Standards

### General Guidelines

- Follow language-specific conventions (.NET, TypeScript/Angular)
- Use meaningful variable and function names
- Add comments for complex business logic
- Write unit tests for new functionality

### .NET Specific

- Use PascalCase for public members
- Use camelCase for private members
- Prefer `var` for obvious types
- Use nullable reference types where appropriate
- Follow minimal API patterns for endpoints

### Angular/TypeScript Specific

- Use camelCase for variables and functions
- Use PascalCase for classes and interfaces
- Prefer `const` over `let` when possible
- Use TypeScript strict mode features
- Follow Angular style guide conventions

## Common Patterns

### Error Handling

```csharp
// Backend error handling
app.MapGet("/api/data/{id}", (int id) =>
{
    try
    {
        // Business logic
        return Results.Ok(data);
    }
    catch (NotFoundException)
    {
        return Results.NotFound();
    }
    catch (Exception)
    {
        return Results.Problem("Internal server error");
    }
});
```

```typescript
// Frontend error handling
this.dataService.getData(id).subscribe({
  next: (data) => (this.data = data),
  error: (error) => {
    console.error("Error fetching data:", error);
    // Handle error appropriately
  },
});
```

### Dependency Injection

```csharp
// Backend service registration
builder.Services.AddScoped<IDataService, DataService>();

// Usage in minimal API
app.MapGet("/api/data", (IDataService dataService) =>
{
    return dataService.GetData();
});
```

```typescript
// Frontend service injection
constructor(private dataService: DataService) {}
```

## File Organization

### Backend

- Controllers/Endpoints: Defined in `Program.cs`
- Models/DTOs: Create separate files in appropriate folders
- Services: Create in `Services/` folder with interfaces
- Configuration: Use `appsettings.json` and `appsettings.Development.json`

### Frontend

- Components: One per folder with component, template, style files
- Services: In `services/` folder or alongside related components
- Models: Create interfaces in `models/` folder
- Routing: Configure in `app.routes.ts`

## Security Considerations

- CORS is configured for development (localhost origins)
- Use HTTPS in production environments
- Validate input data on both client and server
- Implement proper authentication/authorization when needed

## Performance Guidelines

- Use async/await for I/O operations
- Implement proper caching strategies
- Optimize Angular change detection
- Use lazy loading for Angular routes when appropriate
- Monitor bundle sizes and optimize when needed

## Debugging Tips

- Use VS Code integrated debugger for both frontend and backend
- Backend debugging: Set breakpoints in .NET code
- Frontend debugging: Use browser dev tools and Angular DevTools
- API testing: Use built-in OpenAPI/Swagger interface or tools like Postman
- Check browser network tab for API call issues
- Verify CORS configuration if getting 403/CORS errors

Remember to always test changes locally in the dev container before committing!
