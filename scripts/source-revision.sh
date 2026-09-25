#!/bin/sh
# Prints the commit a build was made from, for the Docker build, where git itself isn't available.
#
#   scripts/source-revision.sh [repository-dir]
#
# Uses $SOURCE_REVISION_ID when set. Otherwise reads .git/HEAD (and the ref it names) directly, which needs only
# those files and works for a branch checkout, a detached one, and packed refs. Prints "unknown" when neither
# yields a commit, so a missing hash is visible in the site's footer rather than silently absent.
set -eu

repo="${1:-.}"
rev="${SOURCE_REVISION_ID:-}"

if [ -z "$rev" ] && [ -f "$repo/.git/HEAD" ]; then
    head=$(cat "$repo/.git/HEAD")
    case "$head" in
        "ref: "*)
            ref="${head#ref: }"
            if [ -f "$repo/.git/$ref" ]; then
                rev=$(cat "$repo/.git/$ref")
            elif [ -f "$repo/.git/packed-refs" ]; then
                rev=$(grep " $ref\$" "$repo/.git/packed-refs" | cut -d' ' -f1 | head -n 1)
            fi
            ;;
        *) rev="$head" ;;
    esac
fi

case "$rev" in
    *[!0-9a-f]* | "") rev="unknown" ;;
esac

echo "$rev"
