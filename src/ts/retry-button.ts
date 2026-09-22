/**
 * Wires up `[data-retry]` buttons (the error pages' "try again") to reload the page.
 */
export function initRetryButtons(): void {
    for (const button of document.querySelectorAll<HTMLButtonElement>('[data-retry]')) {
        button.addEventListener('click', () => window.location.reload());
    }
}
