/**
 * Initializes ALT text popovers for images.
 */
export function initAltTextPopovers(): void {
    let openPopover: HTMLElement | null = null;
    let openBadge: HTMLButtonElement | null = null;

    function closeOpenPopover(): void {
        openPopover?.remove();
        openPopover = null;
        openBadge = null;
    }

    document.addEventListener('click', (event: MouseEvent) => {
        const target = event.target as Element | null;
        const badge: HTMLButtonElement | null = target?.closest<HTMLButtonElement>('.alt-badge') ?? null;
        const clickedInsidePopover: boolean = target?.closest<HTMLElement>('.alt-popover') !== null;

        if (clickedInsidePopover) {
            closeOpenPopover();
            return;
        }

        if (badge && badge === openBadge) {
            closeOpenPopover();
            return;
        }

        if (openPopover) {
            closeOpenPopover();
        }

        if (!badge) {
            return;
        }

        event.stopPropagation();

        const wrap: HTMLElement | null = badge.closest<HTMLElement>('.figure-img-wrap');
        if (!wrap) {
            return;
        }

        const popover: HTMLDivElement = document.createElement('div');
        popover.className = 'alt-popover';
        popover.setAttribute('role', 'status');
        popover.textContent = badge.dataset.altText ?? '';
        wrap.appendChild(popover);

        openPopover = popover;
        openBadge = badge;

        requestAnimationFrame(() => popover.classList.add('is-open'));
    });

    document.addEventListener('keydown', (event: KeyboardEvent) => {
        if (event.key === 'Escape') {
            closeOpenPopover();
        }
    });
}
