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

        const empty: HTMLElement | null = scope.querySelector<HTMLElement>('[data-filter-empty]');
        const syncEmpty = (): void => {
            if (empty) {
                empty.hidden = Array.from(sections).some(section => !section.classList.contains('is-collapsed'));
            }
        };

        const animateGrid: ((matches: (section: HTMLElement) => boolean) => void) | null =
            scope.hasAttribute('data-filter-flip') ? createGridFilterAnimator(Array.from(sections), syncEmpty) : null;

        const applyFilter = (pill: HTMLElement, updateHistory: boolean, animate: boolean = true): void => {
            const filter: string = pill.dataset.filter ?? 'all';
            const filterKind: string = pill.dataset.filterKind ?? 'post';

            for (const p of pills) {
                p.classList.toggle('active', p === pill);
                p.setAttribute('aria-pressed', String(p === pill));
            }

            const matches = (section: HTMLElement): boolean => {
                const sectionKind: string = section.dataset.kind ?? 'post';
                const kindMatches: boolean = sectionKind === filterKind;
                const stateMatches: boolean = filterKind === 'note' || filter === 'all' || section.dataset.state === filter;
                return kindMatches && stateMatches;
            };

            if (animate && animateGrid) {
                animateGrid(matches);
            } else {
                for (const section of sections) {
                    section.classList.toggle('is-collapsed', !matches(section));
                }

                syncEmpty();
            }

            if (updateHistory) {
                const hash = filterKind === 'note' ? '#filter=notes' : filter === 'all' ? '' : `#filter=${filter}`;
                const url = window.location.pathname + window.location.search + hash;
                history.replaceState(null, '', url);
            }
        };

        for (const pill of pills) {
            pill.addEventListener('click', () => applyFilter(pill, true));
        }

        const hashFilter = window.location.hash.replace('#filter=', '');
        const matchingPill = hashFilter
            ? Array.from(pills).find(p => (p.dataset.filterKind === 'note' && hashFilter === 'notes') || p.dataset.filter === hashFilter)
            : null;

        const initialPill = matchingPill ?? filterRow.querySelector<HTMLElement>('.pill.active') ?? pills[0];
        if (initialPill) {
            applyFilter(initialPill, false, false); // false: don't rewrite history on initial load, we're just reading it
        }
    }
}

const EXIT_DURATION_MS = 160;
const MOVE_DURATION_MS = 300;
const ENTER_DELAY_MS = 100;
const ENTER_DURATION_MS = 250;

function createGridFilterAnimator(
    items: HTMLElement[],
    onApplied: () => void): (matches: (item: HTMLElement) => boolean) => void {
    let running: Animation[] = [];
    let generation = 0;

    const settle = (): void => {
        running.forEach(animation => animation.cancel());
        running = [];
    };

    return matches => {
        settle();
        const token: number = ++generation;

        const apply = (): void => {
            items.forEach(item => item.classList.toggle('is-collapsed', !matches(item)));
            onApplied();
        };
        if (window.matchMedia('(prefers-reduced-motion: reduce)').matches) {
            apply();
            return;
        }

        const shown = (item: HTMLElement): boolean => !item.classList.contains('is-collapsed');
        const staying: HTMLElement[] = items.filter(item => shown(item) && matches(item));
        const leaving: HTMLElement[] = items.filter(item => shown(item) && !matches(item));
        const entering: HTMLElement[] = items.filter(item => !shown(item) && matches(item));

        const reflow = (): void => {
            const before = new Map(staying.map(item => [item, item.getBoundingClientRect()]));
            apply();

            for (const item of staying) {
                const from: DOMRect = before.get(item)!;
                const to: DOMRect = item.getBoundingClientRect();
                const dx: number = from.left - to.left;
                const dy: number = from.top - to.top;
                if (Math.abs(dx) > 0.5 || Math.abs(dy) > 0.5) {
                    running.push(item.animate({translate: [`${dx}px ${dy}px`, '0 0']}, {duration: MOVE_DURATION_MS, easing: 'ease'}));
                }
            }

            for (const item of entering) {
                running.push(item.animate(
                    {opacity: ['0', '1'], scale: ['0.9', '1']},
                    {duration: ENTER_DURATION_MS, delay: ENTER_DELAY_MS, easing: 'ease', fill: 'backwards'}));
            }
        };

        if (leaving.length === 0) {
            reflow();
            return;
        }

        const exits: Animation[] = leaving.map(item => item.animate(
            {opacity: ['1', '0'], scale: ['1', '0.9']},
            {duration: EXIT_DURATION_MS, easing: 'ease', fill: 'forwards'}));
        running.push(...exits);

        Promise.all(exits.map(exit => exit.finished)).then(() => {
            if (token === generation) {
                settle();
                reflow();
            }
        }, () => {});
    };
}
