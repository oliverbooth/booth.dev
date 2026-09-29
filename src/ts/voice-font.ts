import {initDatasetToggle} from './dataset-toggle.ts';

/**
 * Wires up the voice-font toggle buttons (`[data-voice-font-toggle]`), letting a reader opt any `.prose--serif`
 * content out of its voice font in favour of the default sans-serif, and persists that choice across every post.
 * Must stay in step with the inline bootstrap script in `_MainLayout.cshtml`, which applies the stored choice
 * before first paint to avoid a flash of the serif font.
 */
export function initVoiceFontToggle(): void {
    initDatasetToggle({
        storageKey: 'voiceFont',
        datasetKey: 'voiceFont',
        onValue: 'sans',
        toggleSelector: '[data-voice-font-toggle]',
    });
}
