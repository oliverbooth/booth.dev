#!/bin/sh
# Prints the commit a build was made from, for the Docker build, where git itself isn't available.
#
#   scripts/source-revision.sh [repository-dir]
#
# Uses $SOURCE_REVISION_ID when set. Otherwise reads .git/HEAD (and the ref it names) directly, which needs only
# those files and works for a branch checkout, a detached one, and packed refs. When no commit can be found it prints a
# short reason instead, so the site's footer says why rather than just that the hash is missing:
#   nogit   there is no .git/HEAD in the build context
#   noref   HEAD names a branch whose commit isn't stored in the files that were copied
#   badval  what was found isn't a commit hash
set -eu

repo="${1:-.}"
rev="${SOURCE_REVISION_ID:-}"

if [ -z "$rev" ]; then
    if [ ! -f "$repo/.git/HEAD" ]; then
        echo "nogit"
        exit 0
    fi

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
fi

case "$rev" in
    *[!0-9a-f]* | "")
        echo "badval"
        exit 0
        ;;
esac

echo "$rev"
