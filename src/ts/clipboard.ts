/**
 * Adds copy functionality to all .copy-icon elements in the element.
 */
export function initCopyButtons() {
    for (const icon of document.querySelectorAll<HTMLElement>('.copy-icon')) {
        icon.addEventListener('click', () => {
            const row: Element | null = icon.closest('.crypto-row');
            const address: HTMLElement | null | undefined = row?.querySelector<HTMLElement>('.crypto-address');
            const text: string | undefined = address?.textContent?.trim();

            if (!text) return;

            navigator.clipboard.writeText(text).then(() => {
                showCopyFeedback(icon);
            }).catch(() => {
                // clipboard API unavailable or permission denied — fail silently, icon just won't confirm
            });
        });
    }
}

function showCopyFeedback(button: HTMLElement): void {
    const glyph: HTMLElement = button.querySelector<HTMLElement>('i') ?? button;
    const originalClasses = glyph.className;
    glyph.classList.remove('ti-copy');
    glyph.classList.add('ti-check');
    button.style.color = 'var(--teal-text)';

    setTimeout(() => {
        glyph.className = originalClasses;
        button.style.color = '';
    }, 1200);
}
