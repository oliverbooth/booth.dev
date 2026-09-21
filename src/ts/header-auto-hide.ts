const HIDE_AFTER_PX = 80;

/**
 * Decides whether the header should be hidden after a scroll. It hides on downward movement once past the top of the
 * page, and comes back on any upward movement.
 * @param previousY The previous scroll position.
 * @param currentY The current scroll position.
 * @param hidden Whether the header is currently hidden.
 * @returns Whether the header should now be hidden.
 */
export function nextHeaderHidden(previousY: number, currentY: number, hidden: boolean): boolean {
    if (currentY < previousY) {
        return false;
    }

    if (currentY > previousY && currentY > HIDE_AFTER_PX) {
        return true;
    }

    return hidden;
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
    let previousY = window.scrollY;
    let frame = 0;

    const update = (): void => {
        frame = 0;
        // overscroll bounce can report a negative position
        const currentY = Math.max(window.scrollY, 0);
        const keepVisible = reducedMotion.matches || header.querySelector(':focus-visible') !== null;
        const hidden = keepVisible ? false : nextHeaderHidden(previousY, currentY, header.hasAttribute('data-hidden'));

        header.toggleAttribute('data-hidden', hidden);
        previousY = currentY;
    };

    window.addEventListener('scroll', () => {
        frame ||= requestAnimationFrame(update);
    }, {passive: true});

    // tabbing back up into a hidden header should reveal it
    header.addEventListener('focusin', () => header.removeAttribute('data-hidden'));
}
