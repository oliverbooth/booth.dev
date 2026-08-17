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
