/**
 * Initializes fallback handling for `.avatar` elements: shows the initial in `data-initial` immediately for an
 * avatar with no `<img>` at all, or when its `<img>` fails to load.
 */
export function initAvatarFallback(): void {
    const avatars: NodeListOf<HTMLElement> = document.querySelectorAll<HTMLElement>('.avatar[data-initial]');
    for (const avatar of avatars) {
        const img: HTMLImageElement | null = avatar.querySelector('img');
        if (!img) {
            avatar.textContent = avatar.dataset.initial ?? '';
            continue;
        }

        img.addEventListener('error', () => {
            avatar.textContent = avatar.dataset.initial ?? '';
        }, {once: true});
    }
}
