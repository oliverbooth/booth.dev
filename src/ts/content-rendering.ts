import Prism from 'prismjs';
import 'prismjs/plugins/toolbar/prism-toolbar.js';
import 'prismjs/plugins/copy-to-clipboard/prism-copy-to-clipboard.js';
import 'prismjs/plugins/previewers/prism-previewers.js';
import 'prismjs/plugins/inline-color/prism-inline-color.js';
import 'prismjs/plugins/line-numbers/prism-line-numbers.js';
import 'prismjs/plugins/line-highlight/prism-line-highlight.js';
import 'prismjs/plugins/show-language/prism-show-language.js';
import 'prismjs/plugins/keep-markup/prism-keep-markup.js';
import 'prismjs/plugins/autoloader/prism-autoloader.js';
import {applyCodeBlockHighlights} from './codeblock-highlight/highlighting.ts';
import {initManimScenes} from './manim/scenes.ts';
import {initMermaidScenes} from './mermaid/scenes.ts';
import {ansiToHtml, formatRelativeTimestamp} from './utils.ts';
import {initVexFlowScenes} from './vexflow/scenes.ts';

// Prism auto-runs highlightAll() on DOMContentLoaded unless told otherwise; <script data-manual> tag used to suppress this, but
// that signal doesn't exist for a bundled import, so it's set explicitly here. this MUST run before Prism's own DOMContentLoaded
// listener fires, which it always does since this executes synchronously at module-evaluation time - all we need is to beat
// DOM-ready, not the listener registration
Prism.manual = true;

// language grammars are fetched on demand instead of bundled, so any content type can use any Prism-supported language without
// shipping every grammar Prism has to every page. self-hosted alongside the rest of wwwroot
Prism.plugins.autoloader.languages_path = '/js/prism-components/';

addPrismLanguages();

/**
 * Initializes the front-end Markdown content features for the given element, or the entire document if no element is provided.
 * @param element The element within which to initialize content features. If not provided, the entire document body will be used.
 */
export function initContentFeatures(element?: HTMLElement): void {
    element ||= document.body;

    initPrismCodeblocks(element);
    applyAnsiHighlighting(element);
    renderTimestamps(element);
    renderSpoilers(element);
    initManimScenes(element).catch(error => console.error('Failed to initialize manim-web scenes:', error));
    initVexFlowScenes(element).catch(error => console.error('Failed to initialize vexflow scenes:', error));
    initMermaidScenes(element).catch(error => console.error('Failed to initialize mermaid scenes:', error));
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

        Prism.highlightAllUnder(block.parentElement, false, () => {
            if (block.dataset.highlight) {
                applyCodeBlockHighlights(block);
            }
        });
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
