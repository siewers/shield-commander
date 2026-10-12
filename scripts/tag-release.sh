#!/bin/bash
set -eu

# Create a SemVer release tag (vMAJOR.MINOR.PATCH[-alpha|beta|rc.N]).
# The version is bumped from the latest stable tag, so prereleases of the same
# bump share a version core: major beta -> v1.0.0-beta.2 (after beta.1), then major -> v1.0.0.

usage() {
  echo "Usage: $0 <major|minor|patch> [alpha|beta|rc]"
  exit 1
}

BUMP="${1:-}"
STABILITY="${2:-}"

case "$STABILITY" in
  ""|alpha|beta|rc) ;;
  *) usage ;;
esac

# Latest stable SemVer tag; the earlier CalVer tags (v2026.x.y) are excluded by their year-sized major
LATEST=$(git tag --list 'v*' \
  | grep -E '^v[0-9]+\.[0-9]+\.[0-9]+$' \
  | sed 's/^v//' \
  | awk -F. '$1 < 1000' \
  | sort -t. -k1,1n -k2,2n -k3,3n \
  | tail -n 1)

IFS=. read -r MAJOR MINOR PATCH <<< "${LATEST:-0.0.0}"

case "$BUMP" in
  major) MAJOR=$((MAJOR + 1)); MINOR=0; PATCH=0 ;;
  minor) MINOR=$((MINOR + 1)); PATCH=0 ;;
  patch) PATCH=$((PATCH + 1)) ;;
  *) usage ;;
esac

CORE="v${MAJOR}.${MINOR}.${PATCH}"

if git rev-parse -q --verify "refs/tags/$CORE" >/dev/null; then
  echo "Error: $CORE is already released"
  exit 1
fi

TAG="$CORE"

if [[ -n "$STABILITY" ]]; then
  N=1
  while git rev-parse -q --verify "refs/tags/${CORE}-${STABILITY}.${N}" >/dev/null; do
    N=$((N + 1))
  done
  TAG="${CORE}-${STABILITY}.${N}"
fi

git tag "$TAG"
echo "Tagged: $TAG"
echo "Push with: git push origin $TAG"
