/**
 * Initializes reordering for every admin reorder list on the page, by dragging a row or by its move-up/move-down
 * buttons (a keyboard- and screen-reader-reachable equivalent to the drag, since HTML5 drag-and-drop has no keyboard
 * path of its own).
 */
export function initReorder(): void {
    for (const list of document.querySelectorAll<HTMLElement>('[data-reorder-list]')) {
        const scope: ParentNode = list.closest('[data-reorder-scope]') ?? document;
        const saveBar = scope.querySelector<HTMLElement>('[data-reorder-save]');
        const form = list.dataset.reorderForm ? document.getElementById(list.dataset.reorderForm) as HTMLFormElement | null : null;

        if (saveBar && form) {
            initList(list, saveBar, form);
        }
    }
}

function initList(list: HTMLElement, saveBar: HTMLElement, form: HTMLFormElement): void {
    list.addEventListener('click', (event) => {
        const button = (event.target as HTMLElement).closest<HTMLButtonElement>('[data-move]');
        const item = button?.closest<HTMLElement>('[data-reorder-item]');
        if (!button || !item) {
            return;
        }

        const sibling = button.dataset.move === 'up' ? item.previousElementSibling : item.nextElementSibling;
        if (!sibling) {
            return; // already at that end of the list
        }

        if (button.dataset.move === 'up') {
            item.parentElement?.insertBefore(item, sibling);
        } else {
            item.parentElement?.insertBefore(sibling, item);
        }

        button.focus(); // the button moved with the row; keep focus on it rather than losing it to the body
        syncHiddenInputs(list, form);
        saveBar.hidden = false;
    });

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
 * Replaces the reorder form's `ids` hidden inputs with one per row, in current DOM order.
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
