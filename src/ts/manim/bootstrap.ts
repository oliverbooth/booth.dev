let loadPromise: Promise<void> | null = null;

/**
 * Lazily loads manim-web and bridges its exports onto the global scope, so a scene codeblock's raw JS can reference them (Scene,
 * Dot, BLUE, ...) with no import of its own.
 */
export function ensureManimWebLoaded(): Promise<void> {
    loadPromise ??= import('manim-web').then(manimWeb => {
        Object.assign(window, manimWeb);
    });

    return loadPromise;
}
