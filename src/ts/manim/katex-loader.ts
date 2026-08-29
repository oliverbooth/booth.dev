let katexPromise: Promise<typeof import('katex')> | null = null;

/**
 * Injects KaTeX's stylesheet, if it isn't already present.
 */
function loadKatexStylesheet(): void {
    if (document.querySelector('link[data-katex-stylesheet]')) {
        return;
    }

    const link = document.createElement('link');
    link.rel = 'stylesheet';
    link.href = '/css/katex/katex.min.css';
    link.dataset.katexStylesheet = '';
    document.head.append(link);
}

/**
 * Lazily loads KaTeX.
 * @returns A promise that resolves to the KaTeX module once it's loaded.
 * @remarks Chosen over the site's own MathJax (already loaded sitewide for `$...$` article math) deliberately: MathJax's
 * `typesetPromise` is asynchronous and comparatively heavy per call, built for typesetting a page once - not for the dozens of
 * re-renders a single drag gesture produces per second. KaTeX's synchronous `render()` is fast enough for that; it's the reason
 * KaTeX exists as a separate project from MathJax at all.
 */
export function ensureKatexLoaded(): Promise<typeof import('katex')> {
    katexPromise ??= (async () => {
        loadKatexStylesheet();
        return import('katex');
    })();

    return katexPromise;
}
