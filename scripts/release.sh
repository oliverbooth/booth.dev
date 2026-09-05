#!/usr/bin/env bash
# Cuts a release and pushes it. GitLab turns the new tag into a Release, which fires the
# Portainer webhook.
#
# Usage:
#   scripts/release.sh
#       tags the version already sitting in package.json
#   scripts/release.sh <patch|minor|major|x.y.z>
#       bumps + commits + tags via `npm version`, in one step
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
    npm version "$1"
else
    version="$(node -p "require('./package.json').version")"
    git tag -m "chore: bump to $version" "v$version"
fi

git push --follow-tags
