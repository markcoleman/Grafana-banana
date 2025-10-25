#!/bin/bash
# Test script to validate semantic versioning logic in publish-release.yml

set -e

echo "Testing semantic versioning logic..."
echo ""

# Test 1: No existing tags (should start at v0.1.0)
echo "Test 1: Initial version with no tags"
LATEST_TAG="v0.0.0"
LATEST_VERSION=${LATEST_TAG#v}
IFS='.' read -r MAJOR MINOR PATCH <<< "$LATEST_VERSION"
COMMITS="feat: initial commit"

if echo "$COMMITS" | grep -qiE "^(feat|fix|docs|style|refactor|perf|test|chore)(\(.+\))?!:|BREAKING CHANGE:"; then
  MAJOR=$((MAJOR + 1))
  MINOR=0
  PATCH=0
elif echo "$COMMITS" | grep -qiE "^feat(\(.+\))?:"; then
  MINOR=$((MINOR + 1))
  PATCH=0
else
  PATCH=$((PATCH + 1))
fi

NEW_VERSION="${MAJOR}.${MINOR}.${PATCH}"
echo "  Expected: 0.1.0"
echo "  Got: $NEW_VERSION"
[ "$NEW_VERSION" = "0.1.0" ] && echo "  ✓ PASS" || { echo "  ✗ FAIL"; exit 1; }
echo ""

# Test 2: Patch bump (bug fix)
echo "Test 2: Patch bump with bug fix"
LATEST_TAG="v1.2.3"
LATEST_VERSION=${LATEST_TAG#v}
IFS='.' read -r MAJOR MINOR PATCH <<< "$LATEST_VERSION"
COMMITS="fix: correct login validation"

if echo "$COMMITS" | grep -qiE "^(feat|fix|docs|style|refactor|perf|test|chore)(\(.+\))?!:|BREAKING CHANGE:"; then
  MAJOR=$((MAJOR + 1))
  MINOR=0
  PATCH=0
elif echo "$COMMITS" | grep -qiE "^feat(\(.+\))?:"; then
  MINOR=$((MINOR + 1))
  PATCH=0
else
  PATCH=$((PATCH + 1))
fi

NEW_VERSION="${MAJOR}.${MINOR}.${PATCH}"
echo "  Expected: 1.2.4"
echo "  Got: $NEW_VERSION"
[ "$NEW_VERSION" = "1.2.4" ] && echo "  ✓ PASS" || { echo "  ✗ FAIL"; exit 1; }
echo ""

# Test 3: Minor bump (new feature)
echo "Test 3: Minor bump with new feature"
LATEST_TAG="v1.2.3"
LATEST_VERSION=${LATEST_TAG#v}
IFS='.' read -r MAJOR MINOR PATCH <<< "$LATEST_VERSION"
COMMITS="feat: add user dashboard"

if echo "$COMMITS" | grep -qiE "^(feat|fix|docs|style|refactor|perf|test|chore)(\(.+\))?!:|BREAKING CHANGE:"; then
  MAJOR=$((MAJOR + 1))
  MINOR=0
  PATCH=0
elif echo "$COMMITS" | grep -qiE "^feat(\(.+\))?:"; then
  MINOR=$((MINOR + 1))
  PATCH=0
else
  PATCH=$((PATCH + 1))
fi

NEW_VERSION="${MAJOR}.${MINOR}.${PATCH}"
echo "  Expected: 1.3.0"
echo "  Got: $NEW_VERSION"
[ "$NEW_VERSION" = "1.3.0" ] && echo "  ✓ PASS" || { echo "  ✗ FAIL"; exit 1; }
echo ""

# Test 4: Major bump (breaking change with !)
echo "Test 4: Major bump with breaking change (!)"
LATEST_TAG="v1.2.3"
LATEST_VERSION=${LATEST_TAG#v}
IFS='.' read -r MAJOR MINOR PATCH <<< "$LATEST_VERSION"
COMMITS="feat!: redesign API authentication"

if echo "$COMMITS" | grep -qiE "^(feat|fix|docs|style|refactor|perf|test|chore)(\(.+\))?!:|BREAKING CHANGE:"; then
  MAJOR=$((MAJOR + 1))
  MINOR=0
  PATCH=0
elif echo "$COMMITS" | grep -qiE "^feat(\(.+\))?:"; then
  MINOR=$((MINOR + 1))
  PATCH=0
else
  PATCH=$((PATCH + 1))
fi

NEW_VERSION="${MAJOR}.${MINOR}.${PATCH}"
echo "  Expected: 2.0.0"
echo "  Got: $NEW_VERSION"
[ "$NEW_VERSION" = "2.0.0" ] && echo "  ✓ PASS" || { echo "  ✗ FAIL"; exit 1; }
echo ""

# Test 5: Major bump (BREAKING CHANGE footer)
echo "Test 5: Major bump with BREAKING CHANGE footer"
LATEST_TAG="v1.2.3"
LATEST_VERSION=${LATEST_TAG#v}
IFS='.' read -r MAJOR MINOR PATCH <<< "$LATEST_VERSION"
COMMITS="feat: new auth

BREAKING CHANGE: API now requires OAuth"

if echo "$COMMITS" | grep -qiE "^(feat|fix|docs|style|refactor|perf|test|chore)(\(.+\))?!:|BREAKING CHANGE:"; then
  MAJOR=$((MAJOR + 1))
  MINOR=0
  PATCH=0
elif echo "$COMMITS" | grep -qiE "^feat(\(.+\))?:"; then
  MINOR=$((MINOR + 1))
  PATCH=0
else
  PATCH=$((PATCH + 1))
fi

NEW_VERSION="${MAJOR}.${MINOR}.${PATCH}"
echo "  Expected: 2.0.0"
echo "  Got: $NEW_VERSION"
[ "$NEW_VERSION" = "2.0.0" ] && echo "  ✓ PASS" || { echo "  ✗ FAIL"; exit 1; }
echo ""

echo "All tests passed! ✓"
