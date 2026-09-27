#!/usr/bin/env bash
# Starts a release cycle: branches `release/X.Y[.Z]` off main and commits the version bump as
# the branch's first commit, so it begins life already on a clean, correctly-versioned slate.
# Deliberately does NOT tag - that happens later, via release.sh, once this branch is merged
# back into main.
#
# Usage: scripts/start-release.sh <drop|patch>
set -euo pipefail

bump="${1:?Usage: scripts/start-release.sh <drop|patch>}"

branch="$(git rev-parse --abbrev-ref HEAD)"

if [[ "$branch" == release/* ]]; then
    echo "Already on '$branch' - nothing to do."
    exit 0
fi

if [[ "$branch" != "main" ]]; then
    echo "error: on '$branch' - switch to main to start a release." >&2
    exit 1
fi

git fetch origin main
if ! git merge-base --is-ancestor origin/main HEAD; then
    echo "error: local main has diverged from origin/main - pull/rebase first." >&2
    exit 1
fi

# bumps package.json, resyncs package-lock.json and Directory.Build.props, but makes no commit -
# that's this script's job, once the version is settled on the new release branch.
node scripts/bump-version.mjs "$bump"
version="$(node -p "require('./package.json').version")"

git checkout -b "release/$version"
git add package.json package-lock.json Directory.Build.props
git commit -m "chore: bump to $version"

echo "Started release/$version"
