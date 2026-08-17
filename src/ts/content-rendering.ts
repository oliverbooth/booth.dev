import {applyCodeBlockHighlights} from "./codeblock-highlight/highlighting.ts";

declare const Prism: typeof import('prismjs');

addPrismLanguages();

/**
 * Initializes the front-end Markdown content features for the given element, or the entire document if no element is provided.
 * @param element The element within which to initialize content features. If not provided, the entire document body will be used.
 */
export function initContentFeatures(element?: HTMLElement) {
    element ||= document.body;

    initPrismCodeblocks(element);
}

/**
 * Initializes Prism code blocks within the given element.
 * @param element The element within which to initialize Prism code blocks.
 */
function initPrismCodeblocks(element: HTMLElement) {
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
function addPrismLanguages() {
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
function applyAnsiHighlighting(element: HTMLElement) {
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

function ansiToHtml(input: string): string {
    const ansiColorMap: { [key: string]: string } = {
        '0': 'unset',
        '30': '#0c0c0c',
        '31': '#c50f1f',
        '32': '#13a10e',
        '33': '#c19c00',
        '34': '#0037da',
        '35': '#881798',
        '36': '#3a96dd',
        '37': '#cccccc',
        '90': '#767676'
    };

    let wasOpen: boolean = false;
    return input
        .replace(/\x1b\[(\d+?)m/g, (_, code) => {
            if (code == '0') {
                return '</span>';
            }

            const color: string = ansiColorMap[code];
            const prefix: string = wasOpen ? '</span>' : '';

            if (wasOpen) {
                wasOpen = false;
            }
            if (color) {
                wasOpen = true;
            }

            return color ? `${prefix}<span style="color:${color};">` : '</span>';
        })
        .concat('</span>') // close any open tags at the end
        .replace(/<\/span>(?=<\/span>)/g, ""); // remove redundant closing tags
}