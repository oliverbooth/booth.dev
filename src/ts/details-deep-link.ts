/**
 * Opens the `<details>` element a URL fragment points at, or contains, so a link such as `/challenge/…#solution`
 * lands on visible content instead of a collapsed section.
 */
export function initDetailsDeepLinks(): void {
    const reveal = (): void => {
        const id: string = decodeURIComponent(window.location.hash.slice(1));
        const target: HTMLElement | null = id ? document.getElementById(id) : null;
        const details: HTMLDetailsElement | null = target?.closest('details') ?? null;
        if (!target || !details || details.open) {
            return;
        }

        details.open = true;
        if (target !== details) {
            target.scrollIntoView();
        }
    };

    reveal();
    window.addEventListener('hashchange', reveal);
}
