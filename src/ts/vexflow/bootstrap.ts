type VexFlowModule = typeof import('vexflow');

let loadPromise: Promise<VexFlowModule> | null = null;

/**
 * Lazily loads VexFlow - via its `bravura` entry, which brings in the core engine plus only the Bravura/Academico
 * fonts, skipping the two alternate notation-font families the default `vexflow` entry bundles unconditionally - and
 * bridges its exports onto the global scope, so a notation codeblock's raw JS can reference them (Renderer, Stave,
 * Voice, ...) with no import of its own. A name already present on `window` before this runs is left alone, and the
 * VexFlow export is bridged as `VexFlow<Name>` instead, so it stays reachable rather than becoming permanently
 * unusable the day some other library claims its bare name.
 */
export function ensureVexFlowLoaded(): Promise<VexFlowModule> {
    loadPromise ??= import('vexflow/bravura').then(vexflow => {
        const globals = window as unknown as Record<string, unknown>;
        for (const [name, value] of Object.entries(vexflow)) {
            // `default` is VexFlow's own namespace object (VexFlow.Renderer, VexFlow.Stave, ...) duplicating every
            // named export below it; bridging it too would leak a bare `window.default`, so it's skipped
            if (name === 'default') {
                continue;
            }

            if (name in globals) {
                const fallbackName = `VexFlow${name}`;
                console.warn(`VexFlow export "${name}" collides with an existing global - bridged as "${fallbackName}" instead.`);
                globals[fallbackName] = value;
                continue;
            }

            globals[name] = value;
        }

        return vexflow;
    });

    return loadPromise;
}
