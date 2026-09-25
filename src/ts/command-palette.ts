interface CommandPaletteEntry {
    title: string;
    url: string;
    section: string;
    body: string;
}

/**
 * A palette entry alongside the lowercased fields it's matched against, computed once when the index loads rather
 * than on every keystroke.
 */
interface IndexedEntry {
    entry: CommandPaletteEntry;
    titleLower: string;
    bodyLower: string;
}

/**
 * A palette entry that matched the current query, plus the body snippet (if any) explaining why - present only
 * when the match came from the body rather than the title.
 */
interface Match {
    entry: CommandPaletteEntry;
    snippet: Snippet | null;
}

interface Snippet {
    before: string;
    match: string;
    after: string;
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

const SNIPPET_RADIUS_CHARACTERS = 60;

let refs: CommandPaletteRefs | null = null;
let paletteIndex: IndexedEntry[] | null = null;
let indexPromise: Promise<IndexedEntry[]> | null = null;
let visibleMatches: Match[] = [];
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

function loadIndex(): Promise<IndexedEntry[]> {
    if (paletteIndex) {
        return Promise.resolve(paletteIndex);
    }

    // cache both the in-flight request and its result, so re-opening the palette never re-fetches
    indexPromise ??= fetchIndex();
    return indexPromise;
}

async function fetchIndex(): Promise<IndexedEntry[]> {
    try {
        const response: Response = await fetch(INDEX_URL);
        const entries: CommandPaletteEntry[] = response.ok ? await response.json() as CommandPaletteEntry[] : [];
        paletteIndex = entries.map(entry => ({
            entry,
            titleLower: entry.title.toLowerCase(),
            bodyLower: entry.body.toLowerCase()
        }));
    } catch {
        paletteIndex = [];
    }

    return paletteIndex;
}

function render(query: string): void {
    if (!refs || !paletteIndex) {
        return;
    }

    const trimmed: string = query.trim().toLowerCase();
    visibleMatches = trimmed === ''
        ? paletteIndex.map(indexed => ({entry: indexed.entry, snippet: null}))
        : paletteIndex
            .filter(indexed => indexed.titleLower.includes(trimmed) || indexed.bodyLower.includes(trimmed))
            .sort((a, b) => matchRank(a, trimmed) - matchRank(b, trimmed))
            .map(indexed => ({
                entry: indexed.entry,
                // a snippet only earns its keep when the title alone doesn't already explain the match
                snippet: indexed.titleLower.includes(trimmed) ? null : buildSnippet(indexed, trimmed)
            }));

    refs.results.replaceChildren();
    refs.empty.hidden = visibleMatches.length !== 0;
    activeIndex = visibleMatches.length > 0 ? 0 : -1;

    let lastSection = '';
    for (const [matchIndex, match] of visibleMatches.entries()) {
        if (match.entry.section !== lastSection) {
            const heading = document.createElement('li');
            heading.className = 'command-palette-section';
            heading.textContent = match.entry.section;
            refs.results.appendChild(heading);
            lastSection = match.entry.section;
        }

        refs.results.appendChild(buildResultItem(match, matchIndex));
    }
}

function buildResultItem(match: Match, index: number): HTMLLIElement {
    const item = document.createElement('li');
    item.className = 'command-palette-item';
    item.dataset.index = String(index);
    item.setAttribute('role', 'option');
    item.classList.toggle('is-active', index === activeIndex);

    const title = document.createElement('div');
    title.className = 'command-palette-item-title';
    title.textContent = match.entry.title;
    item.appendChild(title);

    if (match.snippet) {
        item.appendChild(buildSnippetElement(match.snippet));
    }

    return item;
}

function buildSnippetElement(snippet: Snippet): HTMLParagraphElement {
    const element = document.createElement('p');
    element.className = 'command-palette-item-snippet';
    element.appendChild(document.createTextNode(snippet.before));

    const mark = document.createElement('mark');
    mark.textContent = snippet.match;
    element.appendChild(mark);

    element.appendChild(document.createTextNode(snippet.after));
    return element;
}

/**
 * Builds the body snippet surrounding an entry's first match, for display beneath its title. Returns
 * <see langword="null" /> if the query doesn't appear in the body at all (i.e. the entry matched on title only).
 */
function buildSnippet(indexed: IndexedEntry, query: string): Snippet | null {
    const matchIndex: number = indexed.bodyLower.indexOf(query);
    if (matchIndex === -1) {
        return null;
    }

    const body: string = indexed.entry.body;
    const start: number = Math.max(0, matchIndex - SNIPPET_RADIUS_CHARACTERS);
    const end: number = Math.min(body.length, matchIndex + query.length + SNIPPET_RADIUS_CHARACTERS);

    return {
        before: (start > 0 ? '…' : '') + body.slice(start, matchIndex),
        match: body.slice(matchIndex, matchIndex + query.length),
        after: body.slice(matchIndex + query.length, end) + (end < body.length ? '…' : '')
    };
}

/**
 * Ranks a match for sorting: exact title matches first, then prefix title matches, then other title matches,
 * then body-only matches - the order they already appear in from the index within each tier.
 */
function matchRank(indexed: IndexedEntry, query: string): number {
    const title: string = indexed.titleLower;
    if (title === query) {
        return 0;
    }

    if (title.startsWith(query)) {
        return 1;
    }

    return title.includes(query) ? 2 : 3;
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
            navigate(visibleMatches[activeIndex]?.entry);
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

    navigate(visibleMatches[Number(item.dataset.index)]?.entry);
}

function navigate(entry: CommandPaletteEntry | undefined): void {
    if (entry) {
        window.location.href = entry.url;
    }
}
