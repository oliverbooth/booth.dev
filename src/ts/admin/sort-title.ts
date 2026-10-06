interface SortTitleResponse {
    sortTitle: string | null;
}

/**
 * Wires up the "recompute" and "clear" buttons on a sort title field.
 */
export function initSortTitle(): void {
    const field = document.querySelector<HTMLElement>('[data-sort-title]');
    const form = field?.closest('form');
    const input = field?.querySelector<HTMLInputElement>('input[type="text"]');
    const recomputeButton = field?.querySelector<HTMLButtonElement>('[data-sort-title-recompute]');
    const clearButton = field?.querySelector<HTMLButtonElement>('[data-sort-title-clear]');
    const titleInput = form?.querySelector<HTMLInputElement>('#title');

    if (!form || !input || !recomputeButton || !clearButton || !titleInput) {
        return;
    }

    clearButton.addEventListener('click', () => {
        input.value = '';
        input.focus();
    });

    recomputeButton.addEventListener('click', async () => {
        recomputeButton.disabled = true;

        try {
            const url = new URL(form.action);
            url.searchParams.set('handler', 'SortTitle');

            const formData = new FormData(form);
            formData.set('title', titleInput.value);

            const response = await fetch(url, {
                method: 'POST',
                body: formData,
                headers: {Accept: 'application/json'},
            });

            const payload = await response.json() as SortTitleResponse;
            input.value = payload.sortTitle ?? '';
        } finally {
            recomputeButton.disabled = false;
        }
    });
}
