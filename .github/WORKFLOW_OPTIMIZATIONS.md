# GitHub Actions Workflow Optimizations

This document describes the optimizations implemented to improve the efficiency of GitHub Actions workflows in this repository.

## Overview

The workflows have been optimized to:
- Reduce unnecessary workflow runs
- Speed up build and test times
- Prevent resource conflicts
- Minimize GitHub Actions usage costs
- Improve developer experience

## Optimization Strategies

### 1. Concurrency Controls

All workflows now include concurrency groups to prevent multiple simultaneous runs:

```yaml
concurrency:
  group: ${{ github.workflow }}-${{ github.event.pull_request.number || github.ref }}
  cancel-in-progress: true  # For CI/test workflows
  # OR
  cancel-in-progress: false  # For release workflows
```

**Benefits:**
- Prevents wasted compute on outdated commits
- Reduces queue times
- Saves GitHub Actions minutes

### 2. Path Filters

CI and test workflows skip when only documentation changes:

```yaml
on:
  push:
    paths-ignore:
      - '**.md'
      - 'docs/**'
      - '.github/ISSUE_TEMPLATE/**'
      - 'LICENSE'
```

**Benefits:**
- Reduces unnecessary builds
- Faster feedback for documentation PRs
- Saves compute resources

### 3. Timeout Limits

All jobs now have explicit timeout limits:

- CI builds: 15 minutes
- Container tests: 30 minutes
- Release builds: 45 minutes
- Version determination: 5 minutes
- Stale/greetings/labeler: 5-10 minutes

**Benefits:**
- Prevents hanging workflows
- Faster detection of issues
- Predictable workflow duration

### 4. Dependency Caching

Enhanced caching strategies:

**Backend (.NET):**
```yaml
- name: Setup .NET
  uses: actions/setup-dotnet@v5
  with:
    dotnet-version: '9.0.x'
    cache: true
    cache-dependency-path: backend/GrafanaBanana.Api/GrafanaBanana.Api.csproj
```

**Frontend (Node.js):**
```yaml
- name: Setup Node.js
  uses: actions/setup-node@v6
  with:
    node-version: '20'
    cache: 'npm'
    cache-dependency-path: frontend/package-lock.json
```

**Docker:**
```yaml
- name: Build Docker Image
  uses: docker/build-push-action@v5
  with:
    cache-from: type=gha
    cache-to: type=gha,mode=max
```

**Benefits:**
- Faster dependency restoration
- Reduced network usage
- More consistent build times

### 5. Optimized npm Install

Using `npm ci` with optimization flags:

```yaml
- name: Install dependencies
  run: cd frontend && npm ci --prefer-offline --no-audit
```

**Benefits:**
- Faster installs (prefer offline cache)
- Skips audit checks in CI (separate security scanning)
- Cleaner output

### 6. Production Build Mode

Frontend builds use production configuration:

```yaml
- name: Build
  run: cd frontend && npm run build -- --configuration production
```

**Benefits:**
- More realistic build testing
- Catches production-specific issues early
- Better optimization validation

### 7. Reduced Test Coverage Overhead

Disabled code coverage in CI tests (can be enabled separately):

```yaml
- name: Test
  run: cd frontend && npm run test -- --watch=false --browsers=ChromeHeadless --code-coverage=false
```

**Benefits:**
- Faster test execution
- Cleaner output
- Coverage can be run separately when needed

### 8. Scheduled Job Optimization

Stale issue/PR workflow reduced from daily to weekly:

```yaml
on:
  schedule:
    - cron: "0 2 * * 1" # Weekly on Monday at 2 AM UTC
```

**Benefits:**
- Reduced unnecessary workflow runs
- Still maintains issue hygiene
- Saves GitHub Actions minutes

## Workflow-Specific Optimizations

### CI Workflow (`ci.yml`)
- ✅ Concurrency control with auto-cancellation
- ✅ Path filters for documentation
- ✅ Timeout limits (15 minutes per job)
- ✅ Enhanced caching for .NET and npm
- ✅ Optimized npm install flags
- ✅ Production build configuration
- ✅ Disabled coverage in CI tests

### Container Test Workflow (`test-containers.yml`)
- ✅ Concurrency control with auto-cancellation
- ✅ Path filters for documentation
- ✅ Timeout limit (30 minutes)
- ✅ Docker layer caching
- ✅ Efficient container testing

### Release Workflows (`release.yml`, `publish-release.yml`)
- ✅ Concurrency control (no auto-cancel to prevent partial releases)
- ✅ Timeout limits (45 minutes for builds, 5-10 minutes for other jobs)
- ✅ Docker layer caching
- ✅ Job dependencies to ensure proper sequencing

### Maintenance Workflows (`stale.yml`, `greetings.yml`, `labeler.yml`)
- ✅ Timeout limits (5-10 minutes)
- ✅ Reduced stale check frequency (weekly vs daily)

## Dev Container Optimizations

The development container has been optimized for quick startup and shutdown:

### Startup Optimizations
```json
{
  "postCreateCommand": "cd frontend && npm ci --prefer-offline --no-audit",
  "mounts": [
    "source=${localWorkspaceFolder}/.devcontainer/.cache/nuget,target=/home/vscode/.nuget/packages,type=bind,consistency=cached",
    "source=${localWorkspaceFolder}/.devcontainer/.cache/npm,target=/home/vscode/.npm,type=bind,consistency=cached"
  ]
}
```

**Benefits:**
- Faster npm installs with offline cache
- Persistent package caches across container rebuilds
- Reduced network usage
- Consistent dependency versions

### Cache Directories
- `.devcontainer/.cache/nuget/` - NuGet package cache
- `.devcontainer/.cache/npm/` - npm package cache

These directories are gitignored and persist between container rebuilds.

## Expected Performance Improvements

| Metric | Before | After | Improvement |
|--------|--------|-------|-------------|
| CI Build (cached) | ~5-7 min | ~3-5 min | ~30-40% faster |
| Container Test | ~15-20 min | ~10-15 min | ~30% faster |
| Documentation PR | Full build | Skipped | 100% reduction |
| Redundant builds | Multiple | Cancelled | 100% reduction |
| Dev container startup | ~3-5 min | ~1-2 min | ~50-60% faster |
| Stale checks/month | ~30 runs | ~4 runs | ~87% reduction |

## Monitoring Workflow Performance

To monitor the effectiveness of these optimizations:

1. **GitHub Actions Usage:**
   - Go to Settings → Billing → Actions usage
   - Monitor minutes used over time

2. **Workflow Timing:**
   - Check individual workflow run times
   - Compare before/after optimization

3. **Cache Hit Rates:**
   - Review cache hit/miss rates in workflow logs
   - Look for "Cache restored" messages

## Future Optimization Opportunities

1. **Composite Actions:**
   - Extract common setup patterns into reusable composite actions
   - Reduce duplication across workflows

2. **Matrix Builds:**
   - Consider parallel testing across multiple Node/dotnet versions if needed

3. **Selective Testing:**
   - Use changed file detection to run only affected tests
   - Implement test splitting for larger test suites

4. **Pre-built Docker Images:**
   - Create and maintain base images with common dependencies
   - Reduce build times further

5. **GitHub Actions Cache:**
   - Implement more granular caching strategies
   - Cache test results for unchanged code

## Best Practices for Contributors

When adding new workflows:

1. ✅ Always add concurrency controls
2. ✅ Set appropriate timeout limits
3. ✅ Use path filters to skip unnecessary runs
4. ✅ Implement caching for dependencies
5. ✅ Optimize install commands (`npm ci --prefer-offline`)
6. ✅ Use production configurations where appropriate
7. ✅ Add meaningful job and step names
8. ✅ Document any workflow-specific optimizations

## Related Documentation

- [GitHub Actions Documentation](https://docs.github.com/en/actions)
- [Caching Dependencies](https://docs.github.com/en/actions/using-workflows/caching-dependencies-to-speed-up-workflows)
- [Workflow Syntax](https://docs.github.com/en/actions/using-workflows/workflow-syntax-for-github-actions)
- [Docker Build Cache](https://docs.docker.com/build/cache/)
