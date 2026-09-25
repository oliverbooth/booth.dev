import {describe, expect, it} from 'vitest';
import {resolveTheme} from './theme.ts';

describe('resolveTheme', () => {
    it('honours an explicit stored theme over the OS preference', () => {
        expect(resolveTheme('dark', true)).toBe('dark');
        expect(resolveTheme('light', false)).toBe('light');
    });

    it('falls back to the OS preference when nothing is stored', () => {
        expect(resolveTheme(null, true)).toBe('light');
        expect(resolveTheme(null, false)).toBe('dark');
    });

    it('ignores an invalid stored value', () => {
        expect(resolveTheme('sepia', true)).toBe('light');
        expect(resolveTheme('', false)).toBe('dark');
    });
});
