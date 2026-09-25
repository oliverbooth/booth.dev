#!/usr/bin/env bash
# Replaces the staging database with a copy of prod's, then starts the staging app, which applies any migrations prod
# doesn't have yet - the same upgrade prod will go through. Only ever reads from prod.
#
#   scripts/refresh-staging-db.sh [--yes] [--from-dump <file>] [--no-start]
#
# Required:
#   PROD_DB_USER, PROD_DB_NAME           prod's database login and name
#   STAGING_DB_USER, STAGING_DB_NAME     staging's database login and name
# Optional:
#   PROD_DB_CONTAINER      default booth.dev-db
#   STAGING_DB_CONTAINER   default booth.dev-staging-db
#   STAGING_APP_CONTAINER  default booth.dev-staging
#   PROD_SSH               user@host, when prod's containers are on another machine
#   DUMP_DIR               where dumps are kept, default ~/booth.dev-dumps
set -euo pipefail

prod_db="${PROD_DB_CONTAINER:-booth.dev-db}"
staging_db="${STAGING_DB_CONTAINER:-booth.dev-staging-db}"
staging_app="${STAGING_APP_CONTAINER:-booth.dev-staging}"
dump_dir="${DUMP_DIR:-$HOME/booth.dev-dumps}"
assume_yes=false
start_app=true
dump=""

while [ $# -gt 0 ]; do
    case "$1" in
        --yes) assume_yes=true ;;
        --no-start) start_app=false ;;
        --from-dump) dump="${2:?--from-dump needs a file}"; shift ;;
        *) echo "Unknown option: $1" >&2; exit 1 ;;
    esac
    shift
done

: "${STAGING_DB_USER:?STAGING_DB_USER is required}"
: "${STAGING_DB_NAME:?STAGING_DB_NAME is required}"
if [ -z "$dump" ]; then
    : "${PROD_DB_USER:?PROD_DB_USER is required}"
    : "${PROD_DB_NAME:?PROD_DB_NAME is required}"
fi

if [ "$staging_db" = "$prod_db" ] && [ -z "${PROD_SSH:-}" ]; then
    echo "Refusing to run: the staging and prod database containers are both '$prod_db'." >&2
    exit 1
fi

prod_docker() {
    if [ -n "${PROD_SSH:-}" ]; then ssh "$PROD_SSH" docker "$@"; else docker "$@"; fi
}

if [ "$assume_yes" != true ]; then
    read -r -p "Replace the '$STAGING_DB_NAME' database in '$staging_db' with a copy of prod's? [y/N] " answer
    [ "$answer" = "y" ] || { echo "Cancelled."; exit 1; }
fi

if [ -z "$dump" ]; then
    mkdir -p "$dump_dir"
    dump="$dump_dir/prod-$(date +%Y%m%d-%H%M%S).sql.gz"
    echo "Dumping prod to $dump"
    prod_docker exec "$prod_db" pg_dump -U "$PROD_DB_USER" -d "$PROD_DB_NAME" --no-owner --no-privileges | gzip > "$dump"
fi

# the app would otherwise reconnect mid-restore, and on an empty database it would run migrations before the dump lands
if [ -n "$(docker ps -q --filter "name=^${staging_app}$")" ]; then
    echo "Stopping $staging_app"
    docker stop "$staging_app" > /dev/null
fi

staging_psql() {
    docker exec -i "$staging_db" psql -U "$STAGING_DB_USER" -d "$STAGING_DB_NAME" -v ON_ERROR_STOP=1 -q "$@"
}

echo "Resetting the staging database"
staging_psql -c "DROP SCHEMA public CASCADE; CREATE SCHEMA public;"

echo "Restoring $dump"
gunzip -c "$dump" | staging_psql > /dev/null

# staging must not hold prod's live Trakt tokens: refreshing them from here would invalidate the ones prod uses
echo "Removing credentials that must not be shared with prod"
staging_psql -c "TRUNCATE public.trakt_credential;"

if [ "$start_app" = true ] && [ -n "$(docker ps -aq --filter "name=^${staging_app}$")" ]; then
    echo "Starting $staging_app, which applies any pending migrations"
    docker start "$staging_app" > /dev/null
fi

echo "Done."
