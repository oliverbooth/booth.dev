import {describe, expect, it} from 'vitest';
import {type HeaderScrollState, nextHeaderState} from './header-auto-hide.ts';

const state = (overrides: Partial<HeaderScrollState> = {}): HeaderScrollState =>
    ({y: 300, height: 4000, down: 0, hidden: false, ...overrides});

describe('nextHeaderState', () => {
    it('hides once scrolling down has gone past the threshold and the top of the page', () => {
        expect(nextHeaderState(state({y: 200}), 260, 4000).hidden).toBe(true);
    });

    it('stays visible near the top of the page, even when scrolling down', () => {
        expect(nextHeaderState(state({y: 10}), 60, 4000).hidden).toBe(false);
    });

    it('ignores a few pixels of downward jitter', () => {
        expect(nextHeaderState(state({y: 300}), 303, 4000).hidden).toBe(false);
    });

    it('comes back on the smallest upward scroll, and forgets the downward distance', () => {
        const next = nextHeaderState(state({y: 500, hidden: true, down: 200}), 499, 4000);
        expect(next.hidden).toBe(false);
        expect(next.down).toBe(0);
    });

    it('discounts a downward shift that the browser made to compensate for the document growing', () => {
        // e.g. a webfont swapping in above the viewport pushes the content down by 120px, so scrollY follows
        const next = nextHeaderState(state({y: 500}), 620, 4120);
        expect(next.hidden).toBe(false);
        expect(next.y).toBe(620);
        expect(next.height).toBe(4120);
    });

    it('still counts a real scroll that arrives alongside a small change in height', () => {
        expect(nextHeaderState(state({y: 0}), 1500, 4040).hidden).toBe(true);
    });

    it('keeps its state when nothing has moved', () => {
        expect(nextHeaderState(state({hidden: true}), 300, 4000).hidden).toBe(true);
    });
});
