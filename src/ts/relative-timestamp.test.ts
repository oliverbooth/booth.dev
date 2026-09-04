import {readFileSync} from 'node:fs';
import {fileURLToPath} from 'node:url';
import {describe, expect, it} from 'vitest';
import {formatRelativeTimestamp} from './utils.ts';

interface RelativeTimestampFixture {
    referenceUtc: string;
    cases: {targetUtc: string; expected: string}[];
}

const fixturePath = fileURLToPath(new URL('../../test-fixtures/relative-timestamp.json', import.meta.url));
const fixture: RelativeTimestampFixture = JSON.parse(readFileSync(fixturePath, 'utf-8'));

describe('formatRelativeTimestamp', () => {
    const reference = new Date(fixture.referenceUtc);

    for (const {targetUtc, expected} of fixture.cases) {
        it(`"${targetUtc}" relative to reference is "${expected}"`, () => {
            expect(formatRelativeTimestamp(new Date(targetUtc), reference)).toBe(expected);
        });
    }
});
