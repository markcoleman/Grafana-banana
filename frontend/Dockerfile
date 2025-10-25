# Build stage
FROM node:25-alpine AS build

# Install security updates
RUN apk upgrade --no-cache

WORKDIR /app

# Copy package files
COPY package*.json ./

# Install dependencies (including dev dependencies needed for build)
RUN npm ci --no-audit

# Copy source code
COPY . .

# Build the Angular app
RUN npm run build

# Production stage
FROM nginx:alpine AS final

# Install security updates and curl for health checks
RUN apk upgrade --no-cache && \
    apk add --no-cache curl

WORKDIR /usr/share/nginx/html

# Remove default nginx static assets
RUN rm -rf ./*

# Copy built Angular app from build stage
# Angular's new @angular/build:application builder outputs to dist/<project>/browser
COPY --from=build /app/dist/frontend/browser .

# Copy nginx configuration
COPY nginx.conf /etc/nginx/conf.d/default.conf

# Nginx needs to run as root to bind to port 80 and access required directories
# Security is still maintained through proper configuration and minimal attack surface

EXPOSE 80

# Health check
HEALTHCHECK --interval=30s --timeout=3s --start-period=5s --retries=3 \
  CMD curl -f http://localhost/ || exit 1

CMD ["nginx", "-g", "daemon off;"]
