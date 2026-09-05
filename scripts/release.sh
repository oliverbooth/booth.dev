#!/usr/bin/env bash
# Cuts a release: bumps + commits + tags via `npm version`, then pushes the commit and tag
# together. GitLab turns the new tag into a Release, which fires the Portainer webhook.
#
# Usage: scripts/release.sh <patch|minor|major|x.y.z>
set -euo pipefail

bump="${1:?Usage: scripts/release.sh <patch|minor|major|x.y.z>}"

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

npm version "$bump"
git push --follow-tags
