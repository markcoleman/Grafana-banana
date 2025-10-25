# Next Steps After Merging This PR

This document outlines what to do after merging this PR to ensure the CI/CD pipeline works correctly.

## Immediate Actions After Merge

### 1. Verify Automatic Release Workflow

Once you merge this PR to `main`, the `publish-release.yml` workflow will trigger automatically.

**What to watch:**
1. Go to Actions tab in GitHub
2. Find the "Publish Release on Push to Main" workflow
3. Monitor the three jobs:
   - Determine Next Version
   - Build and Push Docker Images
   - Create GitHub Release

**Expected outcome:**
- A new release (likely v0.1.0 or v0.0.1 depending on commits)
- Docker containers published to GHCR
- GitHub release created with changelog

### 2. Make Container Packages Public (if needed)

By default, GitHub packages are private. To make them public:

1. Go to: https://github.com/markcoleman/Grafana-banana/packages
2. Click on each package (backend and frontend)
3. Go to Package Settings (bottom right)
4. Under "Danger Zone" → "Change package visibility"
5. Change to "Public"
6. Confirm the change

This allows anyone to pull your containers without authentication.

### 3. Test Pulling Containers

After the workflow completes and packages are public:

```bash
# Pull the containers
docker pull ghcr.io/markcoleman/grafana-banana/backend:latest
docker pull ghcr.io/markcoleman/grafana-banana/frontend:latest

# Test running them
docker run -d -p 5000:8080 \
  -e ASPNETCORE_ENVIRONMENT=Development \
  ghcr.io/markcoleman/grafana-banana/backend:latest

docker run -d -p 4200:80 \
  ghcr.io/markcoleman/grafana-banana/frontend:latest

# Verify they're running
curl http://localhost:5000/health
curl http://localhost:4200

# Clean up
docker stop $(docker ps -q)
```

### 4. Test Using Docker Compose with GHCR

```bash
# Update docker-compose.ghcr.yml to use the new version
# Then run:
docker-compose -f docker-compose.ghcr.yml up -d

# Verify all services are running
docker-compose -f docker-compose.ghcr.yml ps

# Check logs
docker-compose -f docker-compose.ghcr.yml logs -f

# Clean up
docker-compose -f docker-compose.ghcr.yml down
```

## Testing the Workflows

### Test Container Build on PR

1. Create a new branch: `git checkout -b test/container-build`
2. Make a small change (e.g., update README)
3. Push and create a PR
4. Check that `test-containers.yml` runs
5. Review the test summary in the PR checks

### Test Manual Release

1. Go to Actions → "Manual Release and Publish Containers"
2. Click "Run workflow"
3. Enter a version like `v0.2.0`
4. Click "Run workflow"
5. Verify containers are built and pushed

### Test Semantic Versioning

Make commits with different conventional commit formats and see the version bumps:

```bash
# Patch bump
git commit -m "fix: correct button alignment"
# Merge to main → should bump patch version

# Minor bump
git commit -m "feat: add user settings page"
# Merge to main → should bump minor version

# Major bump
git commit -m "feat!: redesign authentication flow"
# Merge to main → should bump major version
```

## Troubleshooting

### Workflow Fails with Permission Error

**Problem:** "Resource not accessible by integration" error

**Solution:**
1. Go to Settings → Actions → General
2. Under "Workflow permissions"
3. Select "Read and write permissions"
4. Save

### Containers Not Visible

**Problem:** Can't find containers in GHCR

**Solution:**
1. Check the workflow completed successfully
2. Go to Packages section in GitHub
3. Verify packages are public
4. Check package names match `ghcr.io/markcoleman/grafana-banana/backend`

### Version Not Incrementing

**Problem:** Version stays the same or unexpected

**Solution:**
1. Check commit messages follow conventional commits format
2. Use `fix:`, `feat:`, `feat!:` prefixes
3. Check workflow logs for version calculation
4. Manually test version logic: `bash .github/workflows/test-semver.sh`

### Release Not Created

**Problem:** Containers pushed but no GitHub release

**Solution:**
1. Check workflow logs for "Create Release" step
2. Verify `contents: write` permission is set
3. Check if tag already exists: `git tag`
4. If tag exists, delete it: `git push --delete origin v1.0.0`

## Regular Maintenance

### Monitor Workflow Runs

Regularly check:
- Actions tab for failed workflows
- Container registry for disk usage
- Release notes for accuracy

### Keep Dependencies Updated

Update workflow actions periodically:
```yaml
actions/checkout@v5           # Check for v6
docker/setup-buildx-action@v3 # Check for v4
docker/build-push-action@v5   # Check for v6
```

### Review and Clean Old Containers

GHCR has storage limits. Periodically:
1. Go to Packages
2. Review old versions
3. Delete unused versions
4. Keep only recent and major versions

## Optimization Ideas

After everything works, consider:

1. **Add caching for faster builds:**
   - Already uses Docker layer caching
   - Consider adding dependency caching

2. **Add release notes template:**
   - Create `.github/RELEASE_TEMPLATE.md`
   - Customize release notes format

3. **Add environment-specific builds:**
   - Add `staging` builds for pre-production
   - Use different tags for different environments

4. **Add security scanning:**
   - Integrate Trivy for container scanning
   - Add SARIF upload for security results

5. **Add build notifications:**
   - Slack notifications for releases
   - Email notifications for failures

## Documentation Updates

After confirming everything works:

1. Update `CHANGELOG.md` with new CI/CD features
2. Add release badge to README:
   ```markdown
   ![Release](https://img.shields.io/github/v/release/markcoleman/Grafana-banana)
   ```
3. Update `QUICKSTART.md` with container usage
4. Add examples to documentation

## Success Criteria

✅ The setup is successful when:
- [ ] Automatic release workflow runs on push to main
- [ ] Containers are published to GHCR
- [ ] GitHub releases are created with correct versions
- [ ] Containers can be pulled publicly
- [ ] docker-compose.ghcr.yml works with published containers
- [ ] Container tests run on PRs
- [ ] Manual release workflow works
- [ ] Semantic versioning increments correctly

## Support

If you encounter issues not covered here:
1. Check workflow logs in Actions tab
2. Review `docs/CI_CD_WORKFLOWS.md` for detailed documentation
3. Check GitHub Actions documentation
4. Review conventional commits specification
5. Test locally with Docker build commands

## Summary

The CI/CD pipeline is now set up to:
- ✅ Automatically version and release on push to main
- ✅ Build and publish Docker containers
- ✅ Test containers on PRs
- ✅ Support manual releases when needed
- ✅ Follow semantic versioning best practices

Just merge this PR and watch the magic happen! 🚀
