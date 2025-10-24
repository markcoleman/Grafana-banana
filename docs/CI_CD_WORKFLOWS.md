# CI/CD Workflows Documentation

This document describes the automated CI/CD workflows for building, testing, and releasing the Grafana-banana application.

## Overview

The project uses GitHub Actions workflows to automate:
1. **Continuous Integration (CI)**: Building and testing on every push and PR
2. **Container Testing**: Testing Docker builds on PRs before merge
3. **Automatic Releases**: Creating releases and publishing containers on push to main
4. **Manual Releases**: Supporting manual release creation when needed

## Workflow Files

### 1. `ci.yml` - Continuous Integration

**Triggers:**
- Push to `main` or `develop` branches
- Pull requests to `main` or `develop` branches
- Manual workflow dispatch

**Purpose:**
- Builds and tests the .NET backend
- Builds, lints, and tests the Angular frontend
- Runs on every push to ensure code quality

**Jobs:**
- `build-backend`: Restores, builds, and tests the .NET API
- `build-frontend`: Installs dependencies, lints, builds, and tests the Angular app

### 2. `test-containers.yml` - Container Build Testing

**Triggers:**
- Pull requests to `main` or `develop` branches
- Manual workflow dispatch

**Purpose:**
- Tests Docker image builds before merging to main
- Validates that containers can be built successfully
- Performs smoke tests on built containers
- Does NOT push containers to registry (dry-run mode)

**Jobs:**
- `test-build`: 
  - Builds both backend and frontend Docker images
  - Runs containers and validates they start correctly
  - Tests health endpoints
  - Generates test summary

**How to use:**
- Automatically runs on every PR
- Manually trigger via Actions tab for testing
- Review the test summary in the workflow run

### 3. `publish-release.yml` - Automatic Release on Push to Main

**Triggers:**
- Push to `main` branch
- Manual workflow dispatch

**Purpose:**
- Automatically creates a release when code is merged to main
- Builds and pushes Docker containers to GitHub Container Registry
- Generates semantic version based on commit history
- Creates GitHub release with changelog

**Jobs:**
1. `determine-version`: 
   - Analyzes git history since last tag
   - Determines next semantic version using conventional commits
   - Outputs new version and tag

2. `build-and-push`:
   - Builds Docker images for both backend and frontend
   - Pushes images to GHCR with version tag and `latest` tag
   - Supports multi-platform builds (linux/amd64, linux/arm64)

3. `create-release`:
   - Generates changelog from commits
   - Creates GitHub release with version tag
   - Includes Docker pull commands in release notes

**Semantic Versioning Logic:**
- **MAJOR bump**: Breaking changes detected (commits with `!` or `BREAKING CHANGE:`)
- **MINOR bump**: New features detected (commits starting with `feat:`)
- **PATCH bump**: Bug fixes or other changes (default)

**Conventional Commit Examples:**
```bash
# Patch bump (0.1.0 -> 0.1.1)
git commit -m "fix: correct login validation"

# Minor bump (0.1.0 -> 0.2.0)
git commit -m "feat: add user profile page"

# Major bump (0.1.0 -> 1.0.0)
git commit -m "feat!: redesign API authentication"
# or
git commit -m "feat: new auth system

BREAKING CHANGE: API now requires OAuth tokens"
```

### 4. `release.yml` - Manual Release

**Triggers:**
- Manual workflow dispatch (with version input)
- Publishing a GitHub release manually
- Pushing a tag matching `v*.*.*`

**Purpose:**
- Supports manual release creation for special cases
- Allows publishing specific versions
- Can be used to republish containers for a specific version

**How to use:**
1. Go to Actions tab in GitHub
2. Select "Manual Release and Publish Containers"
3. Click "Run workflow"
4. Enter the version tag (e.g., `v1.0.0`)
5. Click "Run workflow"

## Workflow Usage Guide

### For Developers: Normal Development Flow

1. **Create a feature branch:**
   ```bash
   git checkout -b feature/my-feature
   ```

2. **Make changes and commit using conventional commits:**
   ```bash
   git commit -m "feat: add new dashboard widget"
   ```

3. **Push and create a Pull Request:**
   ```bash
   git push origin feature/my-feature
   ```

4. **CI checks run automatically:**
   - `ci.yml` runs to build and test your code
   - `test-containers.yml` runs to verify Docker builds

5. **Review the checks:**
   - Ensure all checks pass before merging
   - Review the container build test summary
   - Address any failures

6. **Merge to main:**
   - Once approved and checks pass, merge the PR
   - `publish-release.yml` automatically runs
   - A new release is created with updated version
   - Containers are published to GHCR

### For Maintainers: Testing Before Merge

You can test the full release process before merging to main:

1. **Test container builds on your PR:**
   - `test-containers.yml` runs automatically
   - Or manually trigger it from the Actions tab

2. **Preview what version will be created:**
   - Review your commit messages
   - Ensure they follow conventional commit format
   - The workflow will determine version based on these

3. **Manual testing of the release workflow:**
   - Create a test branch and trigger `publish-release.yml` manually
   - Note: This will create a real release, so use carefully

### For Releases: Manual Override

If you need to create a specific version manually:

1. **Use the manual release workflow:**
   ```bash
   # Option 1: Via GitHub Actions UI
   Actions -> Manual Release and Publish Containers -> Run workflow
   
   # Option 2: Via git tag
   git tag v1.2.3
   git push origin v1.2.3
   ```

2. **The workflow will:**
   - Build and push containers with your specified version
   - Update the `latest` tag
   - Create a GitHub release

## Published Container Images

After a successful release, container images are available at:

**Backend API:**
```bash
docker pull ghcr.io/markcoleman/grafana-banana/backend:latest
docker pull ghcr.io/markcoleman/grafana-banana/backend:1.0.0
```

**Frontend:**
```bash
docker pull ghcr.io/markcoleman/grafana-banana/frontend:latest
docker pull ghcr.io/markcoleman/grafana-banana/frontend:1.0.0
```

## Using Released Containers

### With Docker Compose

The repository includes `docker-compose.ghcr.yml` for using published containers:

```bash
# Use latest version
docker-compose -f docker-compose.ghcr.yml up -d

# Use specific version (edit file first to change tags)
# Change :latest to :1.0.0 in docker-compose.ghcr.yml
docker-compose -f docker-compose.ghcr.yml up -d
```

### Standalone Containers

```bash
# Run backend
docker run -p 5000:8080 \
  -e ASPNETCORE_ENVIRONMENT=Development \
  ghcr.io/markcoleman/grafana-banana/backend:latest

# Run frontend
docker run -p 4200:80 \
  ghcr.io/markcoleman/grafana-banana/frontend:latest
```

## Troubleshooting

### Container build fails in PR
- Check the `test-containers.yml` workflow logs
- Verify Dockerfile syntax
- Ensure all build dependencies are available
- Test locally: `docker build -t test ./backend/GrafanaBanana.Api`

### Release workflow fails
- Check that commits follow conventional commit format
- Verify GitHub token has necessary permissions
- Ensure no conflicts with existing tags
- Check workflow logs for specific errors

### Wrong version number generated
- Review your commit messages since last tag
- Ensure conventional commit format is used
- Use manual release workflow for specific version

### Containers not accessible
- Verify packages are public in GitHub settings
- Check GitHub Container Registry permissions
- Ensure you're authenticated: `docker login ghcr.io`

## Security Considerations

- Container images are built from source on GitHub runners
- Multi-platform builds ensure compatibility
- Images are scanned by GitHub's security features
- All workflows use GitHub-provided tokens (no secrets needed)
- Container registry access controlled by GitHub permissions

## Best Practices

1. **Use conventional commits** for automatic versioning
2. **Test locally first** before pushing
3. **Review PR checks** before merging
4. **Monitor workflow runs** for any issues
5. **Keep Dockerfiles optimized** for build speed
6. **Use specific tags** in production, not `latest`
7. **Document breaking changes** in commit messages

## Related Files

- `.github/workflows/ci.yml` - Continuous integration
- `.github/workflows/test-containers.yml` - Container testing
- `.github/workflows/publish-release.yml` - Automatic releases
- `.github/workflows/release.yml` - Manual releases
- `docker-compose.yml` - Local development with source builds
- `docker-compose.ghcr.yml` - Deployment with published containers
- `backend/GrafanaBanana.Api/Dockerfile` - Backend container definition
- `frontend/Dockerfile` - Frontend container definition
