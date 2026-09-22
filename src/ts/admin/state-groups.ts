/**
 * Wires up drag-and-drop between grouped state lists (reading list, watchlist): dragging an item into a different
 * group's drop zone sets its state select to that group's state and submits its form. Each item's select also
 * auto-submits on change by itself, so the same action stays available without a mouse.
 */
export function initStateGroups(): void {
    for (const container of document.querySelectorAll<HTMLElement>('[data-state-groups]')) {
        const items = (): HTMLElement[] => [...container.querySelectorAll<HTMLElement>('[data-state-item]')];

        for (const item of items()) {
            const select = item.querySelector<HTMLSelectElement>('[data-state-form] select');
            select?.addEventListener('change', () => {
                item.querySelector<HTMLFormElement>('[data-state-form]')?.requestSubmit();
            });

            item.addEventListener('dragstart', (event) => {
                if (!event.dataTransfer) {
                    return;
                }
                event.dataTransfer.setData('text/plain', item.dataset.id ?? '');
                event.dataTransfer.effectAllowed = 'move';
                item.classList.add('is-dragging');
            });

            item.addEventListener('dragend', () => item.classList.remove('is-dragging'));
        }

        for (const zone of container.querySelectorAll<HTMLElement>('[data-state-drop-zone]')) {
            zone.addEventListener('dragover', (event) => {
                event.preventDefault(); // required for `drop` to fire
                zone.classList.add('is-drop-target');
            });

            zone.addEventListener('dragleave', (event) => {
                const related = event.relatedTarget as Node | null;
                if (!related || !zone.contains(related)) {
                    zone.classList.remove('is-drop-target');
                }
            });

            zone.addEventListener('drop', (event) => {
                event.preventDefault();
                zone.classList.remove('is-drop-target');

                const id = event.dataTransfer?.getData('text/plain');
                const targetState = zone.dataset.state;
                const item = items().find(i => i.dataset.id === id);
                const select = item?.querySelector<HTMLSelectElement>('[data-state-form] select');
                const form = item?.querySelector<HTMLFormElement>('[data-state-form]');
                if (!id || !targetState || !select || !form || select.value === targetState) {
                    return; // dropped in its own group, or something's missing - no-op
                }

                select.value = targetState;
                form.requestSubmit();
            });
        }
    }
}
