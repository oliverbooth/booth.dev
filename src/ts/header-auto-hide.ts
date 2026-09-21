const HIDE_AFTER_PX = 80;

/** How far the visitor has to scroll down in one go before the header hides, so jitter doesn't flap it. */
const HIDE_AFTER_DISTANCE_PX = 8;

/**
 * What the auto-hide tracks between scroll events.
 * @property y The scroll position last seen.
 * @property height The document height last seen.
 * @property down How far the visitor has scrolled down since they last scrolled up.
 * @property hidden Whether the header is currently hidden.
 */
export interface HeaderScrollState {
    y: number;
    height: number;
    down: number;
    hidden: boolean;
}

/**
 * Works out the header's state after a scroll. It hides on sustained downward movement once past the top of the page,
 * and comes back on any upward movement. When the document grows (a font swapping in, an image loading above the
 * viewport) the browser can nudge the scroll position down to keep the view steady, by up to the height that was added;
 * that part of a downward movement isn't the visitor scrolling, so it's discounted.
 * @param state The state after the previous scroll.
 * @param y The current scroll position.
 * @param height The current document height.
 * @returns The new state.
 */
export function nextHeaderState(state: HeaderScrollState, y: number, height: number): HeaderScrollState {
    const delta = y - state.y;
    if (delta < 0) {
        return {y, height, down: 0, hidden: false};
    }

    const scrolled = Math.max(0, delta - Math.max(0, height - state.height));
    if (scrolled === 0) {
        return {...state, y, height};
    }

    const down = state.down + scrolled;
    return {y, height, down, hidden: state.hidden || (y > HIDE_AFTER_PX && down >= HIDE_AFTER_DISTANCE_PX)};
}

/**
 * Slides the site header out of view while scrolling down and back in on any upward scroll. It never hides for visitors
 * who prefer reduced motion, nor while a keyboard-focused control is inside it.
 */
export function initHeaderAutoHide(): void {
    const header = document.querySelector<HTMLElement>('.site-header');
    if (!header) {
        return;
    }

    const reducedMotion = window.matchMedia('(prefers-reduced-motion: reduce)');
    let state: HeaderScrollState = {y: window.scrollY, height: document.documentElement.scrollHeight, down: 0, hidden: false};
    let frame = 0;

    const update = (): void => {
        frame = 0;
        // overscroll bounce can report a negative position
        const currentY = Math.max(window.scrollY, 0);
        state = nextHeaderState(state, currentY, document.documentElement.scrollHeight);

        const keepVisible = reducedMotion.matches || header.querySelector(':focus-visible') !== null;
        if (keepVisible) {
            state = {...state, hidden: false};
        }

        header.toggleAttribute('data-hidden', state.hidden);
    };

    window.addEventListener('scroll', () => {
        frame ||= requestAnimationFrame(update);
    }, {passive: true});

    // tabbing back up into a hidden header should reveal it
    header.addEventListener('focusin', () => {
        state = {...state, hidden: false};
        header.removeAttribute('data-hidden');
    });
}
