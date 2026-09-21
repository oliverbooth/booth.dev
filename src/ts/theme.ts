export type Theme = 'light' | 'dark';

const STORAGE_KEY = 'theme';
const FADE_DURATION_MS = 400;

let fadeTimer: number | undefined;

/**
 * Picks the theme to display. Must stay in step with the inline bootstrap script in `_MainLayout.cshtml`, which runs
 * this same logic before first paint.
 * @param stored The theme the visitor explicitly chose, if any.
 * @param systemPrefersLight Whether the OS is set to a light color scheme.
 * @returns The stored theme if valid, otherwise the OS preference, defaulting to dark.
 */
export function resolveTheme(stored: string | null, systemPrefersLight: boolean): Theme {
    if (stored === 'light' || stored === 'dark') {
        return stored;
    }

    return systemPrefersLight ? 'light' : 'dark';
}

/**
 * Wires up the theme toggle buttons (`[data-theme-toggle]`), persists the visitor's choice, and follows the OS setting
 * for as long as they haven't made one. Dispatches a `themechange` event on `document` whenever the theme changes.
 */
export function initTheme(): void {
    syncToggles(currentTheme());

    document.addEventListener('click', event => {
        if ((event.target as Element).closest('[data-theme-toggle]')) {
            const next: Theme = currentTheme() === 'dark' ? 'light' : 'dark';
            writeStored(next);
            applyTheme(next, true);
        }
    });

    window.matchMedia('(prefers-color-scheme: light)').addEventListener('change', () => {
        if (readStored() === null) {
            applyTheme(resolveTheme(null, systemPrefersLight()), true);
        }
    });

    // keeps other open tabs in step
    window.addEventListener('storage', event => {
        if (event.key === STORAGE_KEY) {
            applyTheme(resolveTheme(event.newValue, systemPrefersLight()), false);
        }
    });
}

/**
 * Gets the theme currently applied to the document.
 * @returns The current theme.
 */
export function currentTheme(): Theme {
    return document.documentElement.dataset.theme === 'light' ? 'light' : 'dark';
}

function applyTheme(theme: Theme, fade: boolean): void {
    const root = document.documentElement;
    if (root.dataset.theme === theme) {
        return;
    }

    // the fade rule only exists while this attribute is set, so it never interferes with ordinary hover transitions
    if (fade) {
        root.dataset.themeFading = '';
        window.clearTimeout(fadeTimer);
        fadeTimer = window.setTimeout(() => delete root.dataset.themeFading, FADE_DURATION_MS);
    }

    root.dataset.theme = theme;
    syncToggles(theme);
    document.dispatchEvent(new CustomEvent('themechange', {detail: {theme}}));
}

function syncToggles(theme: Theme): void {
    for (const toggle of document.querySelectorAll('[data-theme-toggle]')) {
        toggle.setAttribute('aria-pressed', String(theme === 'dark'));
    }
}

function systemPrefersLight(): boolean {
    return window.matchMedia('(prefers-color-scheme: light)').matches;
}

function readStored(): string | null {
    try {
        return localStorage.getItem(STORAGE_KEY);
    } catch {
        return null;
    }
}

function writeStored(theme: Theme): void {
    try {
        localStorage.setItem(STORAGE_KEY, theme);
    } catch {
        // storage blocked; the choice just won't persist
    }
}
