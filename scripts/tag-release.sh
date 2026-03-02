#!/bin/bash
set -eu

STABILITY="${1:-}"

BASE="v$(date +%Y.%-m.%-d)"

case "$STABILITY" in
  alpha|beta|rc)
    BASE="${BASE}-${STABILITY}"
    ;;
  ""|stable)
    ;;
  *)
    echo "Usage: $0 [alpha|beta|rc|stable]"
    exit 1
    ;;
esac

TAG="$BASE"
N=1

while git rev-parse "$TAG" >/dev/null 2>&1; do
  TAG="${BASE}.${N}"
  N=$((N + 1))
done

git tag "$TAG"
echo "Tagged: $TAG"
echo "Push with: git push origin $TAG"
