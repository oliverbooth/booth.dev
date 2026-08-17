import {applyCodeBlockHighlights} from './codeblock-highlight/highlighting.ts';
import {ansiToHtml, formatRelativeTimestamp} from './utils.ts';

declare const Prism: typeof import('prismjs');

addPrismLanguages();

/**
 * Initializes the front-end Markdown content features for the given element, or the entire document if no element is provided.
 * @param element The element within which to initialize content features. If not provided, the entire document body will be used.
 */
export function initContentFeatures(element?: HTMLElement): void {
    element ||= document.body;

    initPrismCodeblocks(element);
    renderTimestamps(element);
    renderSpoilers(element);
}

/**
 * Initializes Prism code blocks within the given element.
 * @param element The element within which to initialize Prism code blocks.
 */
function initPrismCodeblocks(element: HTMLElement): void {
    const blocks: NodeListOf<HTMLElement> = element.querySelectorAll<HTMLElement>('pre code');
    for (const block of blocks) {
        addLineNumbers(block);
        highlightCodeBlock(block);
        applyAnsiHighlighting(block);
    }

    function addLineNumbers(block: HTMLElement) {
        if ('lineNumbers' in block.dataset) {
            block.parentElement?.classList.add('line-numbers');
        }
    }

    function highlightCodeBlock(block: HTMLElement) {
        if (!block.parentElement) {
            console.error('Unexpected code block with no parent:', block);
            return;
        }

        Prism.highlightAllUnder(block.parentElement);
        if (block.dataset.highlight) {
            applyCodeBlockHighlights(block);
        }
    }
}

/**
 * Defines additional Prism languages for highlighting.
 */
function addPrismLanguages(): void {
    Prism.languages.extend('markup', {});
    Prism.languages.hex = {
        'number': {
            pattern: /(?:[a-fA-F0-9]{3}){1,2}\b/i,
            lookbehind: true
        }
    };
    Prism.languages.binary = {
        'number': {
            pattern: /[10]+/i,
            lookbehind: true
        }
    };
}

/**
 * Applies ANSI color highlighting to code blocks with the `language-ansi` class.
 */
function applyAnsiHighlighting(element: HTMLElement): void {
    const blocks: NodeListOf<HTMLElement> = element.querySelectorAll<HTMLElement>('pre code.language-ansi');
    for (const block of blocks) {
        const originalHtml: string = block.innerHTML || '';
        block.innerHTML = ansiToHtml(originalHtml);
    }

    const toolbars: NodeListOf<HTMLDivElement> = element.querySelectorAll<HTMLDivElement>('.code-toolbar .toolbar');

    for (const toolbar of toolbars) {
        const prevSibling: Element | null = toolbar.previousElementSibling;
        const nextSibling: Element | null = toolbar.nextElementSibling;

        if (!prevSibling && !nextSibling) {
            continue;
        }

        if ((prevSibling && prevSibling.classList.contains('language-ansi')) ||
            (nextSibling && nextSibling.classList.contains('language-ansi'))) {
            toolbar.remove();
        }
    }
}

/**
 * Renders spoilers in the given element by adding click event listeners to reveal them.
 * @param element The element within which to render spoilers.
 */
function renderSpoilers(element: Element): void {
    const spoilers: NodeListOf<HTMLElement> = element.querySelectorAll<HTMLElement>('.spoiler');
    for (const spoiler of spoilers) {
        spoiler.addEventListener('click', () => {
            spoiler.classList.add('spoiler-revealed');
        });
    }
}

/**
 * Renders timestamps in the given element by formatting them according to their specified format.
 * @param element The element within which to render timestamps.
 */
function renderTimestamps(element: Element): void {
    const timestamps: NodeListOf<HTMLSpanElement> = element.querySelectorAll<HTMLSpanElement>('span[data-timestamp][data-format]');
    for (const timestamp of timestamps) {
        const seconds: number = parseInt(timestamp.getAttribute('data-timestamp') || '0');
        const format: string | null = timestamp.getAttribute('data-format');
        const date: Date = new Date(seconds * 1000);

        const shortTimeString: string = date.toLocaleTimeString([], {hour: '2-digit', minute: '2-digit'});
        const shortDateString: string = date.toLocaleDateString([], {day: '2-digit', month: '2-digit', year: 'numeric'});
        const longTimeString: string = date.toLocaleTimeString([], {hour: '2-digit', minute: '2-digit', second: '2-digit'});
        const longDateString: string = date.toLocaleDateString([], {day: 'numeric', month: 'long', year: 'numeric'});
        const weekday: string = date.toLocaleString([], {weekday: 'long'});
        timestamp.setAttribute('title', `${weekday}, ${longDateString} ${shortTimeString}`);

        switch (format) {
            case 't':
                timestamp.textContent = shortTimeString;
                break;
            case 'T':
                timestamp.textContent = longTimeString;
                break;
            case 'd':
                timestamp.textContent = shortDateString;
                break;
            case 'D':
                timestamp.textContent = longDateString;
                break;
            case 'f':
                timestamp.textContent = `${longDateString} at ${shortTimeString}`;
                break;
            case 'F':
                timestamp.textContent = `${weekday}, ${longDateString} at ${shortTimeString}`;
                break;
            case 'R':
                setInterval(() => {
                    timestamp.textContent = formatRelativeTimestamp(date);
                }, 1000);
                break;
        }
    }
}