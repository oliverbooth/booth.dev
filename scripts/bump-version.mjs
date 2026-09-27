#!/usr/bin/env node
// Computes and writes the next CalVer (year.drop[.patch]) version into package.json, then resyncs
// package-lock.json and Directory.Build.props. Replaces `npm version`, which enforces strict
// three-part semver and can't express an optional patch segment.
//
// Usage:
//   scripts/bump-version.mjs drop     next feature drop this year (or .1 of the new year, if it rolled over)
//   scripts/bump-version.mjs patch    next patch on the current drop
//   scripts/bump-version.mjs X.Y[.Z]  set an explicit version

import {execFileSync} from 'node:child_process';
import {readFileSync, writeFileSync} from 'node:fs';
import {fileURLToPath} from 'node:url';

const root = fileURLToPath(new URL('..', import.meta.url));
const kind = process.argv[2];

if (!kind) {
    console.error('Usage: scripts/bump-version.mjs <drop|patch|X.Y[.Z]>');
    process.exit(1);
}

const pkgPath = `${root}/package.json`;
const pkgRaw = readFileSync(pkgPath, 'utf8');
const currentVersion = JSON.parse(pkgRaw).version;

const nextVersion = kind === 'drop' || kind === 'patch'
    ? computeBump(currentVersion, kind)
    : parseExplicit(kind);

writeFileSync(pkgPath, pkgRaw.replace(/"version": "[^"]+"/, `"version": "${nextVersion}"`));
execFileSync('npm', ['i', '--package-lock-only', '--no-audit', '--no-fund'], {cwd: root, stdio: 'inherit'});
execFileSync('node', ['scripts/sync-version.mjs'], {cwd: root, stdio: 'inherit'});

console.log(`Bumped to ${nextVersion}`);

/**
 * Computes the next `drop` or `patch` version, rolling over to `{year}.1` if the calendar year has
 * moved on since `current` was cut.
 */
function computeBump(current, kind) {
    const match = /^(\d+)\.(\d+)(?:\.(\d+))?$/.exec(current);
    if (!match) {
        console.error(`error: can't parse current version '${current}' as year.drop[.patch]`);
        process.exit(1);
    }

    const [, yearPart, dropPart, patchPart] = match;
    const year = Number(yearPart);
    const drop = Number(dropPart);
    const patch = patchPart === undefined ? undefined : Number(patchPart);
    const currentYear = new Date().getFullYear() % 100;

    if (year !== currentYear) {
        return `${currentYear}.1`;
    }

    if (kind === 'drop') {
        return `${year}.${drop + 1}`;
    }

    return `${year}.${drop}.${(patch ?? 0) + 1}`;
}

function parseExplicit(version) {
    if (!/^\d+\.\d+(\.\d+)?$/.test(version)) {
        console.error(`error: '${version}' isn't a valid year.drop[.patch] version`);
        process.exit(1);
    }

    return version;
}
