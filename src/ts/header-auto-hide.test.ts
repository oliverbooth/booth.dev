import {describe, expect, it} from 'vitest';
import {nextHeaderHidden} from './header-auto-hide.ts';

describe('nextHeaderHidden', () => {
    it('hides when scrolling down past the threshold', () => {
        expect(nextHeaderHidden(200, 260, false)).toBe(true);
    });

    it('stays visible near the top of the page, even when scrolling down', () => {
        expect(nextHeaderHidden(10, 60, false)).toBe(false);
    });

    it('comes back on the smallest upward scroll', () => {
        expect(nextHeaderHidden(500, 499, true)).toBe(false);
    });

    it('keeps its current state when the position has not changed', () => {
        expect(nextHeaderHidden(300, 300, true)).toBe(true);
        expect(nextHeaderHidden(300, 300, false)).toBe(false);
    });
});
