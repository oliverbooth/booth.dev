#!/usr/bin/env node

import { readFileSync, writeFileSync } from 'node:fs';
import { fileURLToPath } from 'node:url';

const root = fileURLToPath(new URL('..', import.meta.url));
const { version } = JSON.parse(readFileSync(`${root}/package.json`, 'utf8'));

const propsPath = `${root}/Directory.Build.props`;
const props = readFileSync(propsPath, 'utf8');
const pattern = /<VersionPrefix>[^<]+<\/VersionPrefix>/;

if (!pattern.test(props)) {
    console.error('Directory.Build.props: <VersionPrefix> element not found.');
    process.exit(1);
}

writeFileSync(propsPath, props.replace(pattern, `<VersionPrefix>${version}</VersionPrefix>`));
console.log(`Directory.Build.props synced to ${version}`);
