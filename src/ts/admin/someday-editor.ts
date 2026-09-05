/**
 * Initializes the Someday Editor preview functionality.
 */
export function initSomedayEditorPreview(): void {
    const titleInput = document.querySelector<HTMLInputElement>('#entry-title');
    const previewTitle = document.querySelector<HTMLElement>('#preview-title');

    if (!titleInput || !previewTitle) {
        return;
    }

    titleInput.addEventListener('input', () => {
        previewTitle.textContent = `Someday, ${titleInput.value}`;
    });
}
