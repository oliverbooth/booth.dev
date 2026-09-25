/**
 * Implements client-side search/filtering of a list of items, scoped to any container marked `data-search-scope`. Items opt in
 * by being marked `data-search-item` - this works regardless of the underlying markup (table row, card, grid tile, ...). Items opt
 * individual fields into the search by marking them `data-search` - this lets one item (e.g. a title + slug stacked together)
 * mark only some of its content as searchable, rather than searching everything inside it. An optional element marked
 * `data-search-empty` inside the scope is shown when a query matches zero items.
 */
export function initSearch(): void {
    const scopes: NodeListOf<HTMLElement> = document.querySelectorAll<HTMLElement>('[data-search-scope]');
    for (const scope of scopes) {
        const input: HTMLInputElement | null = scope.querySelector<HTMLInputElement>('#search');
        const rows: NodeListOf<HTMLElement> = scope.querySelectorAll<HTMLElement>('[data-search-item]');
        const emptyState: HTMLElement | null = scope.querySelector<HTMLElement>('[data-search-empty]');
        if (!input || rows.length === 0) {
            continue;
        }

        // pre-compute each row's searchable text once, rather than re-querying on every keystroke
        const rowText: Map<HTMLElement, string> = new Map();
        for (const row of rows) {
            const fields: NodeListOf<HTMLElement> = row.querySelectorAll<HTMLElement>('[data-search]');
            const text: string = Array.from(fields).map(field => field.textContent ?? '').join(' ').toLowerCase();
            rowText.set(row, text);
        }

        const applySearch = (): void => {
            const query: string = input.value.trim().toLowerCase();
            let visibleCount = 0;

            for (const row of rows) {
                const matches: boolean = query === '' || (rowText.get(row) ?? '').includes(query);
                row.hidden = !matches;
                if (matches) {
                    visibleCount++;
                }
            }

            if (emptyState) {
                emptyState.hidden = visibleCount !== 0;
            }
        };

        input.addEventListener('input', applySearch);
        applySearch(); // in case the input already has a value (e.g. browser form-restore on back/refresh)
    }
}
