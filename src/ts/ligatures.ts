const STORAGE_KEY = 'ligatures';

/**
 * Wires up the ligature toggle buttons (`[data-ligature-toggle]`), letting a reader turn off the mono font's
 * programming ligatures (such as rendering `->` as a single arrow glyph) in code blocks, and persists that choice
 * across every post. Must stay in step with the inline bootstrap script in `_MainLayout.cshtml`, which applies the
 * stored choice before first paint to avoid a flash of ligatures the reader opted out of.
 */
export function initLigatureToggle(): void {
    syncToggles(isOff());

    document.addEventListener('click', event => {
        if ((event.target as Element).closest('[data-ligature-toggle]')) {
            const next = !isOff();
            writeStored(next);
            applyOverride(next);
        }
    });

    window.addEventListener('storage', event => {
        if (event.key === STORAGE_KEY) {
            applyOverride(event.newValue === 'off');
        }
    });
}

function isOff(): boolean {
    return document.documentElement.dataset.ligatures === 'off';
}

function applyOverride(off: boolean): void {
    if (off) {
        document.documentElement.dataset.ligatures = 'off';
    } else {
        delete document.documentElement.dataset.ligatures;
    }

    syncToggles(off);
}

function syncToggles(off: boolean): void {
    for (const toggle of document.querySelectorAll('[data-ligature-toggle]')) {
        toggle.setAttribute('aria-pressed', String(off));
    }
}

function writeStored(off: boolean): void {
    try {
        if (off) {
            localStorage.setItem(STORAGE_KEY, 'off');
        } else {
            localStorage.removeItem(STORAGE_KEY);
        }
    } catch {
        // storage blocked; the choice just won't persist
    }
}
