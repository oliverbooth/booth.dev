/**
 * Initializes drag-to-reorder for the admin someday list. Dragging a row live-reorders the DOM; dropping it reveals
 * a "Save order" bar that submits the new order as a plain form post, so nothing is persisted until confirmed.
 */
export function initSomedayReorder(): void {
    const list = document.querySelector<HTMLElement>('[data-reorder-list]');
    const saveBar = document.querySelector<HTMLElement>('[data-reorder-save]');
    const form = list?.dataset.reorderForm ? document.getElementById(list.dataset.reorderForm) as HTMLFormElement | null : null;

    if (!list || !saveBar || !form) {
        return;
    }

    let dragged: HTMLElement | null = null;

    list.addEventListener('dragstart', (event) => {
        const item = (event.target as HTMLElement).closest<HTMLElement>('[data-reorder-item]');
        if (!item) {
            return;
        }

        dragged = item;
        item.classList.add('is-dragging');
        event.dataTransfer?.setData('text/plain', item.dataset.id ?? '');
        if (event.dataTransfer) {
            event.dataTransfer.effectAllowed = 'move';
        }
    });

    list.addEventListener('dragend', () => {
        dragged?.classList.remove('is-dragging');
        dragged = null;
    });

    list.addEventListener('dragover', (event) => {
        if (!dragged) {
            return;
        }

        event.preventDefault();

        const target = (event.target as HTMLElement).closest<HTMLElement>('[data-reorder-item]');
        if (!target || target === dragged) {
            return;
        }

        const rect = target.getBoundingClientRect();
        const before = event.clientY < rect.top + rect.height / 2;
        target.parentElement?.insertBefore(dragged, before ? target : target.nextSibling);
    });

    list.addEventListener('drop', (event) => {
        if (!dragged) {
            return;
        }

        event.preventDefault();
        syncHiddenInputs(list, form);
        saveBar.hidden = false;
    });
}

/**
 * Replaces the reorder form's `ids` hidden inputs with one per row, in current DOM order. Any other hidden input
 * already in the form - namely the antiforgery token - is left untouched.
 */
function syncHiddenInputs(list: HTMLElement, form: HTMLFormElement): void {
    form.querySelectorAll('input[name="ids"]').forEach(input => input.remove());

    for (const item of list.querySelectorAll<HTMLElement>('[data-reorder-item]')) {
        const input = document.createElement('input');
        input.type = 'hidden';
        input.name = 'ids';
        input.value = item.dataset.id ?? '';
        form.appendChild(input);
    }
}
