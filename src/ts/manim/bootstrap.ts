let loadPromise: Promise<void> | null = null;

/**
 * Lazily loads manim-web and bridges its exports onto the global scope, so a scene codeblock's raw JS can reference them (Scene,
 * Dot, BLUE, ...) with no import of its own. A name already present on `window` before this runs is left alone, and the manim-web
 * export is bridged as `Manim<Name>` instead, so it stays reachable rather than becoming permanently unusable the day some other
 * library claims its bare name.
 */
export function ensureManimWebLoaded(): Promise<void> {
    loadPromise ??= import('manim-web').then(manimWeb => {
        const globals = window as unknown as Record<string, unknown>;
        for (const [name, value] of Object.entries(manimWeb)) {
            if (name in globals) {
                const fallbackName = `Manim${name}`;
                console.warn(`manim-web export "${name}" collides with an existing global - bridged as "${fallbackName}" instead.`);
                globals[fallbackName] = value;
                continue;
            }

            globals[name] = value;
        }
    });

    return loadPromise;
}
