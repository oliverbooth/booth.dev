const PROSE_SELECTOR = '.note-card .prose';

/** Blocks whose height CSS already keeps to whole lines. */
const TEXT_BLOCKS = 'p, ul, ol';

/** Sub-pixel slack below which a block counts as already being a whole number of lines tall. */
const TOLERANCE = 0.5;

/**
 * Pads every non-text block in a note's ruled paper out to a whole number of lines, so an image, code block or
 * heading can't knock the text after it off the lines. Text blocks need no help: the note card's CSS already pins
 * their line pitch and margins to the same `--lh` this reads back.
 */
export function initRuledPaper(): void {
    const prose = document.querySelector<HTMLElement>(PROSE_SELECTOR);
    if (!prose) {
        return;
    }

    // border-box size ignores the card's (and a blockquote's) rotation, unlike getBoundingClientRect
    const heights = new WeakMap<Element, number>();

    const snap = (): void => {
        const lineHeight = parseFloat(getComputedStyle(prose).lineHeight);
        if (!lineHeight) {
            return;
        }

        for (const block of prose.children) {
            const height = heights.get(block);
            if (!height || block.matches(TEXT_BLOCKS)) {
                continue;
            }

            const lines = Math.ceil((height - TOLERANCE) / lineHeight);
            // the last block ends on the card's own bottom padding, so it gets no gap of its own
            const gap = block === prose.lastElementChild ? 0 : lineHeight;
            const style = (block as HTMLElement).style;

            // its own margin-top would push it off the grid, and the previous block already leaves a gap
            style.marginTop = '0';
            style.marginBottom = `${lines * lineHeight - height + gap}px`;
        }
    };

    const resizes = new ResizeObserver(entries => {
        for (const entry of entries) {
            heights.set(entry.target, entry.borderBoxSize[0].blockSize);
        }

        snap();
    });

    for (const block of prose.children) {
        resizes.observe(block);
    }

    // toggling the voice font changes the line pitch without necessarily resizing an image
    new MutationObserver(snap).observe(document.documentElement, {
        attributes: true,
        attributeFilter: ['data-voice-font'],
    });
}
