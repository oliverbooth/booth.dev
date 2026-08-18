/**
 * Implements filtering of sections based on the selected pill.
 */
export function initFiltering(): void {
    const scopes: NodeListOf<HTMLElement> = document.querySelectorAll<HTMLElement>('[data-filter-scope]');
    for (const scope of scopes) {
        const filterRow: HTMLElement | null = scope.querySelector<HTMLElement>('.filter-row');
        if (!filterRow) {
            continue;
        }

        const sections: NodeListOf<HTMLElement> = scope.querySelectorAll<HTMLElement>('[data-state]');
        sections.forEach(section => section.classList.add('is-visible'));

        const pills: NodeListOf<HTMLElement> = filterRow.querySelectorAll<HTMLElement>('.pill');

        const applyFilter = (pill: HTMLElement): void => {
            const filter: string = pill.dataset.filter ?? 'all';
            const filterKind: string = pill.dataset.filterKind ?? 'post';

            pills.forEach(p => p.classList.remove('active'));
            pill.classList.add('active');

            for (const section of sections) {
                const sectionKind: string = section.dataset.kind ?? 'post';
                const kindMatches: boolean = sectionKind === filterKind;
                const stateMatches: boolean = filterKind === 'note' || filter === 'all' || section.dataset.state === filter;
                section.classList.toggle('is-collapsed', !(kindMatches && stateMatches));
            }
        };

        for (const pill of pills) {
            pill.addEventListener('click', () => applyFilter(pill));
        }

        const initialPill = filterRow.querySelector<HTMLElement>('.pill.active') ?? pills[0];
        if (initialPill) {
            applyFilter(initialPill);
        }
    }
}
