const COLLAPSED_LABEL = 'alt';

/**
 * Initializes the ALT badges on figures: clicking one expands it in place to show the image's description, replacing
 * the "alt" label; clicking it again, clicking elsewhere, or pressing Escape collapses it back.
 */
export function initAltTextPopovers(): void {
    let openBadge: HTMLButtonElement | null = null;

    function collapse(badge: HTMLButtonElement): void {
        badge.classList.remove('is-open');
        badge.setAttribute('aria-expanded', 'false');
        badge.textContent = COLLAPSED_LABEL;
        if (openBadge === badge) {
            openBadge = null;
        }
    }

    function expand(badge: HTMLButtonElement): void {
        if (openBadge && openBadge !== badge) {
            collapse(openBadge);
        }

        badge.classList.add('is-open');
        badge.setAttribute('aria-expanded', 'true');
        badge.textContent = badge.dataset.altText ?? COLLAPSED_LABEL;
        openBadge = badge;
    }

    document.addEventListener('click', (event: MouseEvent) => {
        const target = event.target as Element | null;
        const badge: HTMLButtonElement | null = target?.closest<HTMLButtonElement>('.alt-badge') ?? null;

        if (badge) {
            event.stopPropagation();
            if (badge === openBadge) {
                collapse(badge);
            } else {
                expand(badge);
            }

            return;
        }

        if (openBadge) {
            collapse(openBadge);
        }
    });

    document.addEventListener('keydown', (event: KeyboardEvent) => {
        if (event.key === 'Escape' && openBadge) {
            collapse(openBadge);
        }
    });
}
