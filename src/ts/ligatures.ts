import {initDatasetToggle} from './dataset-toggle.ts';

/**
 * Wires up the ligature toggle buttons (`[data-ligature-toggle]`), letting a reader turn off the mono font's
 * programming ligatures (such as rendering `->` as a single arrow glyph) in code blocks, and persists that choice
 * across every post. Must stay in step with the inline bootstrap script in `_MainLayout.cshtml`, which applies the
 * stored choice before first paint to avoid a flash of ligatures the reader opted out of.
 */
export function initLigatureToggle(): void {
    initDatasetToggle({
        storageKey: 'ligatures',
        datasetKey: 'ligatures',
        onValue: 'off',
        toggleSelector: '[data-ligature-toggle]',
    });
}
