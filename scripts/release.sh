#!/usr/bin/env bash
# Cuts a release and pushes it. GitLab CI creates the Release for the new tag, then fires the
# Portainer webhook.
#
# Usage:
#   scripts/release.sh
#       tags the version already sitting in package.json
#   scripts/release.sh <drop|patch|x.y[.z]>
#       bumps + commits + tags directly on main, in one step
set -euo pipefail

branch="$(git rev-parse --abbrev-ref HEAD)"
if [[ "$branch" != "main" ]]; then
    echo "error: on '$branch', not 'main' - merge your branch into main first." >&2
    exit 1
fi

git fetch origin main

if ! git merge-base --is-ancestor origin/main HEAD; then
    echo "error: local main has diverged from origin/main - pull/rebase first." >&2
    exit 1
fi

if [[ $# -gt 0 ]]; then
    node scripts/bump-version.mjs "$1"
    version="$(node -p "require('./package.json').version")"
    git add package.json package-lock.json Directory.Build.props
    git commit -m "chore: bump to $version"
else
    version="$(node -p "require('./package.json').version")"
fi

git tag -m "chore: bump to $version" "v$version"

git push --follow-tags
