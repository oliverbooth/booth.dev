const STORAGE_KEY = 'voiceFont';

/**
 * Wires up the voice-font toggle buttons (`[data-voice-font-toggle]`), letting a reader opt any `.prose--serif`
 * content out of its voice font in favour of the default sans-serif, and persists that choice across every post.
 * Must stay in step with the inline bootstrap script in `_MainLayout.cshtml`, which applies the stored choice
 * before first paint to avoid a flash of the serif font.
 */
export function initVoiceFontToggle(): void {
    syncToggles(isSansOverride());

    document.addEventListener('click', event => {
        if ((event.target as Element).closest('[data-voice-font-toggle]')) {
            const next = !isSansOverride();
            writeStored(next);
            applyOverride(next);
        }
    });

    window.addEventListener('storage', event => {
        if (event.key === STORAGE_KEY) {
            applyOverride(event.newValue === 'sans');
        }
    });
}

function isSansOverride(): boolean {
    return document.documentElement.dataset.voiceFont === 'sans';
}

function applyOverride(sans: boolean): void {
    if (sans) {
        document.documentElement.dataset.voiceFont = 'sans';
    } else {
        delete document.documentElement.dataset.voiceFont;
    }

    syncToggles(sans);
}

function syncToggles(sans: boolean): void {
    for (const toggle of document.querySelectorAll('[data-voice-font-toggle]')) {
        toggle.setAttribute('aria-pressed', String(sans));
    }
}

function writeStored(sans: boolean): void {
    try {
        if (sans) {
            localStorage.setItem(STORAGE_KEY, 'sans');
        } else {
            localStorage.removeItem(STORAGE_KEY);
        }
    } catch {
        // storage blocked; the choice just won't persist
    }
}
