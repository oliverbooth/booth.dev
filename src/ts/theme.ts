import {readStoredSafe, syncAriaPressed, writeStoredSafe} from './dataset-toggle.ts';

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
    syncAriaPressed('[data-theme-toggle]', currentTheme() === 'dark');

    document.addEventListener('click', event => {
        if ((event.target as Element).closest('[data-theme-toggle]')) {
            const next: Theme = currentTheme() === 'dark' ? 'light' : 'dark';
            writeStoredSafe(STORAGE_KEY, next);
            applyTheme(next, true);
        }
    });

    window.matchMedia('(prefers-color-scheme: light)').addEventListener('change', () => {
        if (readStoredSafe(STORAGE_KEY) === null) {
            applyTheme(resolveTheme(null, systemPrefersLight()), true);
        }
    });

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

function applyTheme(theme: Theme, animate: boolean): void {
    const root = document.documentElement;
    if (root.dataset.theme === theme) {
        return;
    }

    const swap = (): void => {
        root.dataset.theme = theme;
        syncAriaPressed('[data-theme-toggle]', theme === 'dark');
        document.dispatchEvent(new CustomEvent('themechange', {detail: {theme}}));
    };

    if (!animate || window.matchMedia('(prefers-reduced-motion: reduce)').matches) {
        swap();
        return;
    }

    // one GPU cross-fade of the whole page, rather than transitioning every element: far cheaper, and the header and
    // page can't drift out of step with each other
    if (document.startViewTransition) {
        document.startViewTransition(swap);
        return;
    }

    // fallback for browsers without view transitions; the fade rule only exists while this attribute is set
    root.dataset.themeFading = '';
    window.clearTimeout(fadeTimer);
    fadeTimer = window.setTimeout(() => delete root.dataset.themeFading, FADE_DURATION_MS);
    swap();
}

function systemPrefersLight(): boolean {
    return window.matchMedia('(prefers-color-scheme: light)').matches;
}
