# Release Process

This document describes the release process for Grafana-banana.

## Overview

Releases are automated through GitHub Actions and are triggered when a new tag is pushed or a GitHub Release is created.

## Release Workflow

### 1. Prepare the Release

1. **Update CHANGELOG.md**
   - Move changes from `[Unreleased]` to a new version section
   - Update version numbers and dates
   - Add a new `[Unreleased]` section for future changes

2. **Update Version Numbers** (if applicable)
   - Update version in `backend/GrafanaBanana.Api/GrafanaBanana.Api.csproj`
   - Update version in `frontend/package.json`

3. **Ensure Tests Pass**
   - Run `make test` locally
   - Ensure CI pipeline is green

### 2. Create a Release

#### Option A: Using Git Tags

```bash
# Create an annotated tag
git tag -a v0.1.0 -m "Release version 0.1.0"

# Push the tag to GitHub
git push origin v0.1.0
```

#### Option B: Using GitHub Releases (Recommended)

1. Go to the [Releases page](https://github.com/markcoleman/Grafana-banana/releases)
2. Click "Draft a new release"
3. Choose or create a tag (e.g., `v0.1.0`)
4. Set the release title (e.g., "Version 0.1.0")
5. Add release notes (copy from CHANGELOG.md)
6. Click "Publish release"

### 3. Automated Actions

When a release is published, the GitHub Actions workflow automatically:

1. **Builds Docker Images**
   - Backend API image
   - Frontend application image

2. **Tags Images**
   - Version tag (e.g., `0.1.0`)
   - `latest` tag

3. **Pushes to GitHub Container Registry**
   - `ghcr.io/markcoleman/grafana-banana/backend:0.1.0`
   - `ghcr.io/markcoleman/grafana-banana/backend:latest`
   - `ghcr.io/markcoleman/grafana-banana/frontend:0.1.0`
   - `ghcr.io/markcoleman/grafana-banana/frontend:latest`

4. **Creates Build Artifacts**
   - Multi-architecture images (amd64, arm64)
   - Build cache for faster subsequent builds

### 4. Verify the Release

After the workflow completes:

1. **Check GitHub Actions**
   - Verify the workflow completed successfully
   - Review the deployment summary

2. **Verify Docker Images**
   ```bash
   docker pull ghcr.io/markcoleman/grafana-banana/backend:0.1.0
   docker pull ghcr.io/markcoleman/grafana-banana/frontend:0.1.0
   ```

3. **Test the Images**
   ```bash
   # Test backend
   docker run -p 5000:5000 ghcr.io/markcoleman/grafana-banana/backend:0.1.0
   
   # Test frontend
   docker run -p 4200:80 ghcr.io/markcoleman/grafana-banana/frontend:0.1.0
   ```

## Version Numbering

This project follows [Semantic Versioning](https://semver.org/):

- **MAJOR** version (X.0.0): Incompatible API changes
- **MINOR** version (0.X.0): New functionality in a backward-compatible manner
- **PATCH** version (0.0.X): Backward-compatible bug fixes

## Pre-releases

For pre-release versions, use suffixes:

- `v0.1.0-alpha.1` - Alpha release
- `v0.1.0-beta.1` - Beta release
- `v0.1.0-rc.1` - Release candidate

## Hotfixes

For urgent bug fixes:

1. Create a hotfix branch from the release tag
2. Make the necessary fixes
3. Create a new patch release (increment the patch version)
4. Follow the standard release process

## Rollback

To rollback to a previous version:

1. Users can pull the previous version tag:
   ```bash
   docker pull ghcr.io/markcoleman/grafana-banana/backend:0.0.9
   docker pull ghcr.io/markcoleman/grafana-banana/frontend:0.0.9
   ```

2. Or update the `latest` tag (requires admin access):
   - This should be done carefully and documented in the changelog

## Troubleshooting

### Workflow Fails to Push Images

If the workflow fails with permission errors:

1. Ensure the repository has package write permissions enabled
2. Check that the `GITHUB_TOKEN` has the correct permissions
3. Verify the package visibility settings (public/private)

### Image Size Issues

If images are too large:

1. Review Dockerfile for optimization opportunities
2. Use multi-stage builds
3. Remove unnecessary dependencies
4. Use `.dockerignore` to exclude unnecessary files

### Build Cache Issues

If builds are slow or cache isn't working:

1. Check GitHub Actions cache settings
2. Verify the cache-from and cache-to configurations
3. Consider clearing old caches from the Actions settings

## Manual Release Workflow Trigger

You can manually trigger the release workflow:

1. Go to Actions → Release and Publish Containers
2. Click "Run workflow"
3. Enter the version tag (e.g., `v0.1.0`)
4. Click "Run workflow"

This is useful for re-publishing images or testing the workflow.
