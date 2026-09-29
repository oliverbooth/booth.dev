#!/bin/sh
# Prints the commit a build was made from, for the Docker build, where the build may have no git history to read.
#
#   scripts/source-revision.sh [repository-dir]
#
# In order, it uses:
#   1. $SOURCE_REVISION_ID, when set.
#   2. .git/HEAD and the ref it names, read directly (needs only those files; a branch, a detached HEAD and packed refs
#      all work).
#   3. What the git server at $GIT_REPOSITORY reports for $GIT_REF, for builds whose checkout has no .git at all, as
#      Portainer's has.
# When no commit can be found it prints a short reason instead, so the site's footer says why rather than just that the
# hash is missing:
#   nogit   there is no .git/HEAD in the build context, and no $GIT_REF to ask the server about
#   norem   $GIT_REF was set but the server gave no commit for it
#   noref   HEAD names a branch whose commit isn't stored in the files that were copied
#   badval  what was found isn't a commit hash
set -eu

repo="${1:-.}"
rev="${SOURCE_REVISION_ID:-}"

if [ -z "$rev" ]; then
    if [ -f "$repo/.git/HEAD" ]; then
        head=$(cat "$repo/.git/HEAD")
        case "$head" in
            "ref: "*)
                ref="${head#ref: }"
                if [ -f "$repo/.git/$ref" ]; then
                    rev=$(cat "$repo/.git/$ref")
                elif [ -f "$repo/.git/packed-refs" ]; then
                    rev=$(grep " $ref\$" "$repo/.git/packed-refs" | cut -d' ' -f1 | head -n 1)
                fi
                if [ -z "$rev" ]; then
                    echo "noref"
                    exit 0
                fi
                ;;
            *) rev="$head" ;;
        esac
    elif [ -n "${GIT_REF:-}" ]; then
        rev=$(git ls-remote "${GIT_REPOSITORY:-https://git.booth.dev/oliver/booth.dev.git}" "$GIT_REF" 2>/dev/null |
            awk -v ref="$GIT_REF" '$2 == ref { print $1; exit }' || true)
        if [ -z "$rev" ]; then
            echo "norem"
            exit 0
        fi
    else
        echo "nogit"
        exit 0
    fi
fi

case "$rev" in
    *[!0-9a-f]* | "")
        echo "badval"
        exit 0
        ;;
esac

echo "$rev"
