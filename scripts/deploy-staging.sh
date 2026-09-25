#!/usr/bin/env bash
# Deploys a branch to staging by pointing the `staging` branch at it; CI then runs the tests and, if they pass, calls
# the staging stack's Portainer webhook.
#
#   scripts/deploy-staging.sh            deploys the current branch
#   scripts/deploy-staging.sh <branch>   deploys that branch
set -euo pipefail

branch="${1:-$(git branch --show-current)}"

if [ -n "$(git status --porcelain)" ]; then
    echo "Uncommitted changes aren't deployed - commit them first." >&2
    exit 1
fi

git push --force origin "$branch:staging"
