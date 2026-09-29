/**
 * Reads a `localStorage` value.
 * @param key The storage key to read.
 * @returns The stored value, or `null` if absent or storage is blocked (e.g. a browser blocking it in private browsing).
 */
export function readStoredSafe(key: string): string | null {
    try {
        return localStorage.getItem(key);
    } catch {
        return null;
    }
}

/**
 * Writes or clears a `localStorage` value.
 * @param key The storage key to write.
 * @param value The value to store, or `null` to remove the key.
 */
export function writeStoredSafe(key: string, value: string | null): void {
    try {
        if (value === null) {
            localStorage.removeItem(key);
        } else {
            localStorage.setItem(key, value);
        }
    } catch {
        // storage blocked; the choice just won't persist
    }
}

/**
 * Sets `aria-pressed` on every element matching `selector` to reflect a toggle's current state.
 * @param selector The selector matching the toggle button(s).
 * @param pressed Whether the toggle is in its "on" state.
 */
export function syncAriaPressed(selector: string, pressed: boolean): void {
    for (const toggle of document.querySelectorAll(selector)) {
        toggle.setAttribute('aria-pressed', String(pressed));
    }
}

/**
 * Wires up a boolean reader-preference toggle that mirrors its state into a `document.documentElement.dataset`
 * attribute (set to `onValue` when on, removed when off) and persists it to `localStorage`, syncing `aria-pressed`
 * on every matching toggle button and reacting to the `storage` event fired when another tab changes the same key.
 *
 * Any such preference that has an inline pre-paint bootstrap script in `_MainLayout.cshtml` (to avoid a flash of
 * the "off" state before this module loads) must keep that script's `storageKey`/`datasetKey`/`onValue` in step
 * with what's passed here.
 * @param options.storageKey The `localStorage` key the preference is persisted under.
 * @param options.datasetKey The `dataset` property on `document.documentElement` that reflects the preference.
 * @param options.onValue The dataset value written when the preference is on.
 * @param options.toggleSelector A selector matching every button that toggles this preference.
 */
export function initDatasetToggle(options: {
    storageKey: string;
    datasetKey: string;
    onValue: string;
    toggleSelector: string;
}): void {
    const {storageKey, datasetKey, onValue, toggleSelector} = options;

    const isOn = (): boolean => document.documentElement.dataset[datasetKey] === onValue;

    const applyOverride = (on: boolean): void => {
        if (on) {
            document.documentElement.dataset[datasetKey] = onValue;
        } else {
            delete document.documentElement.dataset[datasetKey];
        }

        syncAriaPressed(toggleSelector, on);
    };

    syncAriaPressed(toggleSelector, isOn());

    document.addEventListener('click', event => {
        if ((event.target as Element).closest(toggleSelector)) {
            const next = !isOn();
            writeStoredSafe(storageKey, next ? onValue : null);
            applyOverride(next);
        }
    });

    window.addEventListener('storage', event => {
        if (event.key === storageKey) {
            applyOverride(event.newValue === onValue);
        }
    });
}
