/**
 * Initializes fallback handling for `.avatar` elements containing an `<img>`.
 */
export function initAvatarFallback(): void {
    const avatars: NodeListOf<HTMLElement> = document.querySelectorAll<HTMLElement>('.avatar[data-initial]');
    for (const avatar of avatars) {
        const img: HTMLImageElement | null = avatar.querySelector('img');
        if (!img) {
            continue;
        }

        img.addEventListener('error', () => {
            avatar.textContent = avatar.dataset.initial ?? '';
        }, {once: true});
    }
}
