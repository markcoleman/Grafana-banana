# Contributing Screenshots

Thank you for contributing to the visual documentation of Grafana-banana!

## Quick Guide

Screenshots help users understand what the application looks like and how features work. Here's how to contribute:

## Taking Screenshots

### General Guidelines

1. **Use high-quality screenshots**: Capture at native resolution
2. **Show context**: Include enough of the interface to understand the feature
3. **Clean up**: Remove personal information, test data should be professional
4. **Annotate if needed**: Use arrows or highlights to draw attention to important elements
5. **Optimize file size**: Use PNG for UI screenshots, compress if over 500KB

### Recommended Tools

- **macOS**: Cmd+Shift+4 (area), Cmd+Shift+3 (full screen)
- **Windows**: Windows+Shift+S (Snipping Tool)
- **Linux**: Flameshot, GNOME Screenshot
- **Browser**: Browser DevTools screenshot feature (F12 → three dots → Capture screenshot)

## Screenshot Categories

### Frontend (Angular App)

Location: `docs/screenshots/frontend/`

**What to capture:**
- Main application page (home view)
- Weather forecast display
- Any interactive features
- OpenTelemetry traces in browser console
- Network tab showing API calls
- Responsive views (desktop, tablet, mobile)

**Example process:**
1. Start the frontend: `cd frontend && npm start`
2. Navigate to http://localhost:4200
3. Take screenshots of key features
4. Save with descriptive names like `01-home-page.png`

### Backend (Web API)

Location: `docs/screenshots/backend/`

**What to capture:**
- Swagger/OpenAPI documentation page (http://localhost:5000/swagger)
- API endpoint responses (using Swagger UI or Postman)
- Health check endpoints
- Metrics endpoint showing Prometheus format
- Example trace IDs in responses

**Example process:**
1. Start the backend: `cd backend/GrafanaBanana.Api && dotnet run`
2. Navigate to http://localhost:5000/swagger
3. Test endpoints and capture responses
4. Save with descriptive names like `01-swagger-ui.png`

### Observability Stack

Location: `docs/screenshots/observability/`

**What to capture:**
- Grafana main dashboard
- Prometheus metrics queries
- Tempo distributed traces
- Loki log aggregation
- Pre-configured dashboards with real data

**Example process:**
1. Start the full stack: `docker-compose up -d`
2. Generate some traffic: curl endpoints or use the frontend
3. Open Grafana at http://localhost:3000
4. Navigate to dashboards and capture key views
5. Save with descriptive names like `01-grafana-dashboard.png`

## File Naming Convention

Use a numbered prefix and descriptive name:

```
01-main-feature.png
02-detailed-view.png
03-error-handling.png
```

This helps maintain order when screenshots are displayed.

## File Formats

- **PNG**: Preferred for UI screenshots (better quality, supports transparency)
- **JPG**: Only for photos or if file size is a concern
- **GIF**: For animations or step-by-step processes (keep under 5MB)

## Optimization

Before committing, optimize images:

```bash
# Using ImageOptim (macOS)
# Drag and drop files

# Using pngcrush (Linux/macOS)
pngcrush -reduce input.png output.png

# Using online tools
# https://tinypng.com/
# https://imageoptim.com/online
```

## Adding Screenshots to Documentation

After adding screenshots, update the relevant README files:

1. Add image references in markdown:
   ```markdown
   ![Description](../screenshots/frontend/01-home-page.png)
   ```

2. Or create a gallery in the README:
   ```markdown
   ## Screenshots
   
   <details>
   <summary>View Screenshots</summary>
   
   ### Home Page
   ![Home Page](../screenshots/frontend/01-home-page.png)
   
   ### Weather Forecast
   ![Weather Forecast](../screenshots/frontend/02-weather-forecast.png)
   
   </details>
   ```

## Review Process

When submitting screenshots:

1. **Test**: Ensure screenshots are clear and relevant
2. **Verify**: Check file sizes (under 500KB preferred)
3. **Document**: Update README files with image references
4. **PR**: Include screenshots in your pull request
5. **Describe**: Explain what each screenshot shows in the PR description

## Privacy and Security

- **No sensitive data**: Remove API keys, passwords, or personal information
- **Use test data**: Show realistic but fake data
- **Blur if needed**: Blur sensitive areas using image editing tools
- **Review**: Double-check before committing

## Questions?

If you have questions about contributing screenshots, please:

1. Check the [screenshots README](screenshots/README.md)
2. Review existing screenshots for examples
3. Open an issue for clarification
4. Ask in pull request comments

Thank you for helping improve Grafana-banana's documentation! 📸
