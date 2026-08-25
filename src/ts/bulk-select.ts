/**
 * Implements bulk row selection for admin tables, scoped to any container marked `data-bulk-select-scope`.
 * @remarks A checkbox marked `data-select-all` toggles every row checkbox marked `data-row-select`; a bulk-action bar marked
 * `data-bulk-actions` is shown only while at least one row is selected, with an element marked `data-selected-count` kept in sync
 * with the current selection count. The bulk-action form (anywhere inside the scope, associated with its submit button via the
 * standard HTML `form="..."` attribute rather than DOM nesting, since row checkboxes live inside the `<table>`, not the form) can
 * carry a `data-confirm` template containing `{count}` - that placeholder is kept up to date with the live selection count so the
 * confirm prompt (see confirm.ts) reads naturally regardless of how many rows are selected.
 */
export function initBulkSelect(): void {
    const scopes: NodeListOf<HTMLElement> = document.querySelectorAll<HTMLElement>('[data-bulk-select-scope]');
    for (const scope of scopes) {
        const selectAll: HTMLInputElement | null = scope.querySelector<HTMLInputElement>('[data-select-all]');
        const bulkBar: HTMLElement | null = scope.querySelector<HTMLElement>('[data-bulk-actions]');
        const countLabel: HTMLElement | null = scope.querySelector<HTMLElement>('[data-selected-count]');
        const confirmForm: HTMLFormElement | null = scope.querySelector<HTMLFormElement>('form[data-confirm]');
        const confirmTemplate: string | undefined = confirmForm?.dataset.confirm;

        const rowCheckboxes = (): HTMLInputElement[] =>
            [...scope.querySelectorAll<HTMLInputElement>('[data-row-select]')];

        if (rowCheckboxes().length === 0) {
            continue;
        }

        const update = (): void => {
            const all: HTMLInputElement[] = rowCheckboxes();
            const checked: HTMLInputElement[] = all.filter(cb => cb.checked);

            if (bulkBar) {
                bulkBar.hidden = checked.length === 0;
            }

            if (countLabel) {
                countLabel.textContent = String(checked.length);
            }

            if (confirmForm && confirmTemplate) {
                confirmForm.dataset.confirm = confirmTemplate.replace('{count}', String(checked.length));
            }

            if (selectAll) {
                selectAll.checked = all.length > 0 && checked.length === all.length;
                selectAll.indeterminate = checked.length > 0 && checked.length < all.length;
            }
        };

        selectAll?.addEventListener('change', () => {
            for (const cb of rowCheckboxes()) {
                cb.checked = selectAll.checked;
            }
            update();
        });

        for (const cb of rowCheckboxes()) {
            cb.addEventListener('change', update);
        }

        update();
    }
}
