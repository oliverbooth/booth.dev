interface CommandPaletteEntry {
    title: string;
    url: string;
    section: string;
}

interface CommandPaletteRefs {
    dialog: HTMLDialogElement;
    input: HTMLInputElement;
    results: HTMLUListElement;
    empty: HTMLElement;
}

const DIALOG_SELECTOR = '#command-palette';
const TRIGGER_SELECTOR = '[data-palette-trigger]';
const INDEX_URL = '/api/command-palette/index';

let refs: CommandPaletteRefs | null = null;
let entries: CommandPaletteEntry[] | null = null;
let indexPromise: Promise<CommandPaletteEntry[]> | null = null;
let visibleMatches: CommandPaletteEntry[] = [];
let activeIndex = -1;

/**
 * Initializes the command palette (Ctrl+K / Cmd+K).
 */
export function initCommandPalette(): void {
    const dialog: HTMLDialogElement | null = document.querySelector<HTMLDialogElement>(DIALOG_SELECTOR);
    if (!dialog) {
        return;
    }

    const input: HTMLInputElement | null = dialog.querySelector<HTMLInputElement>('.command-palette-input');
    const results: HTMLUListElement | null = dialog.querySelector<HTMLUListElement>('.command-palette-results');
    const empty: HTMLElement | null = dialog.querySelector<HTMLElement>('.command-palette-empty');
    if (!input || !results || !empty) {
        throw new Error('Command palette markup is missing required child elements.');
    }

    refs = {dialog, input, results, empty};

    document.addEventListener('keydown', onGlobalKeyDown);
    for (const trigger of document.querySelectorAll<HTMLElement>(TRIGGER_SELECTOR)) {
        trigger.addEventListener('click', open);
    }

    input.addEventListener('input', () => render(input.value));
    input.addEventListener('keydown', onInputKeyDown);
    results.addEventListener('click', onResultsClick);

    dialog.addEventListener('click', event => {
        if (event.target === dialog) {
            close();
        }
    });

    dialog.addEventListener('close', onDialogClose);
}

function onGlobalKeyDown(event: KeyboardEvent): void {
    if (!refs || !(event.ctrlKey || event.metaKey) || event.key.toLowerCase() !== 'k') {
        return;
    }

    event.preventDefault();

    if (refs.dialog.open) {
        close();
        return;
    }

    // don't stack on top of another already-open dialog (e.g. the lightbox)
    if (document.querySelector('dialog[open]')) {
        return;
    }

    open();
}

function open(): void {
    if (!refs) {
        return;
    }

    refs.dialog.showModal();
    refs.input.value = '';
    refs.input.focus();

    void loadIndex().then(() => render(''));
}

function close(): void {
    if (!refs) {
        return;
    }

    refs.dialog.classList.add('is-closing');
    refs.dialog.addEventListener(
        'transitionend',
        () => {
            refs?.dialog.classList.remove('is-closing');
            refs?.dialog.close();
        },
        {once: true}
    );
}

function onDialogClose(): void {
    refs?.results.replaceChildren();
    visibleMatches = [];
    activeIndex = -1;
}

function loadIndex(): Promise<CommandPaletteEntry[]> {
    if (entries) {
        return Promise.resolve(entries);
    }

    // cache both the in-flight request and its result, so re-opening the palette never re-fetches
    indexPromise ??= fetchIndex();
    return indexPromise;
}

async function fetchIndex(): Promise<CommandPaletteEntry[]> {
    try {
        const response: Response = await fetch(INDEX_URL);
        entries = response.ok ? await response.json() as CommandPaletteEntry[] : [];
    } catch {
        entries = [];
    }

    return entries;
}

function render(query: string): void {
    if (!refs || !entries) {
        return;
    }

    const trimmed: string = query.trim().toLowerCase();
    visibleMatches = trimmed === ''
        ? entries
        : entries
            .filter(entry => entry.title.toLowerCase().includes(trimmed))
            .sort((a, b) => matchRank(a, trimmed) - matchRank(b, trimmed));

    refs.results.replaceChildren();
    refs.empty.hidden = visibleMatches.length !== 0;
    activeIndex = visibleMatches.length > 0 ? 0 : -1;

    let lastSection = '';
    for (const [index, entry] of visibleMatches.entries()) {
        if (entry.section !== lastSection) {
            const heading = document.createElement('li');
            heading.className = 'command-palette-section';
            heading.textContent = entry.section;
            refs.results.appendChild(heading);
            lastSection = entry.section;
        }

        const item = document.createElement('li');
        item.className = 'command-palette-item';
        item.textContent = entry.title;
        item.dataset.index = String(index);
        item.setAttribute('role', 'option');
        item.classList.toggle('is-active', index === activeIndex);
        refs.results.appendChild(item);
    }
}

/**
 * Ranks a match for sorting: exact title matches first, then prefix matches, then everything else - the order
 * they already appear in from the index.
 */
function matchRank(entry: CommandPaletteEntry, query: string): number {
    const title: string = entry.title.toLowerCase();
    if (title === query) {
        return 0;
    }

    return title.startsWith(query) ? 1 : 2;
}

function onInputKeyDown(event: KeyboardEvent): void {
    if (visibleMatches.length === 0) {
        return;
    }

    switch (event.key) {
        case 'ArrowDown':
            event.preventDefault();
            setActive((activeIndex + 1) % visibleMatches.length);
            break;
        case 'ArrowUp':
            event.preventDefault();
            setActive((activeIndex - 1 + visibleMatches.length) % visibleMatches.length);
            break;
        case 'Enter':
            event.preventDefault();
            navigate(visibleMatches[activeIndex]);
            break;
    }
}

function setActive(index: number): void {
    if (!refs) {
        return;
    }

    activeIndex = index;
    for (const item of refs.results.querySelectorAll<HTMLElement>('.command-palette-item')) {
        item.classList.toggle('is-active', Number(item.dataset.index) === index);
    }

    refs.results.querySelector<HTMLElement>(`[data-index="${index}"]`)?.scrollIntoView({block: 'nearest'});
}

function onResultsClick(event: MouseEvent): void {
    const item: HTMLElement | null = (event.target as HTMLElement).closest<HTMLElement>('.command-palette-item');
    if (!item?.dataset.index) {
        return;
    }

    navigate(visibleMatches[Number(item.dataset.index)]);
}

function navigate(entry: CommandPaletteEntry | undefined): void {
    if (entry) {
        window.location.href = entry.url;
    }
}
