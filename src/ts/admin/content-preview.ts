import {initContentFeatures} from '../content-rendering.ts';

declare const MathJax: {typesetPromise(elements?: HTMLElement[]): Promise<void>} | undefined;

const DEBOUNCE_MS = 400;

interface PreviewResponse {
    html: string;
    proseClass: string;
}

/**
 * Initializes the live preview pane for an admin content editor (posts, notes, tutorials, challenges, ...). Looks
 * for a `Preview` page handler on whatever form is present, so it works unmodified on any editor that implements
 * one.
 *
 * An editor can mark more than one textarea with `data-preview-source` (challenges do this for description and
 * solution) and pair each with a `[data-preview-tab]` pill to switch which one drives the pane; the pill's value
 * must match the textarea's `data-preview-field`, which is sent to the Preview handler as `field` so it knows what
 * to render. Editors with a single source need neither attribute — the pane just tracks that one textarea, as before.
 */
export function initContentPreview(): void {
    const form = document.querySelector<HTMLFormElement>('.form-grid');
    const pane = document.querySelector<HTMLElement>('#preview-pane');
    const titleInput = document.querySelector<HTMLInputElement>('#title');
    const sources = [...document.querySelectorAll<HTMLTextAreaElement>('textarea[data-preview-source]')];
    const tabs = [...document.querySelectorAll<HTMLElement>('[data-preview-tab]')];
    const previewTitle = document.querySelector<HTMLElement>('#preview-title');
    const previewBody = document.querySelector<HTMLElement>('#preview-body');

    if (!form || !pane || sources.length === 0 || !previewTitle || !previewBody) {
        return;
    }

    let activeSource = sources[0];

    if (titleInput) {
        titleInput.addEventListener('input', () => {
            previewTitle.textContent = titleInput.value;
        });
    }

    let debounceHandle: number | undefined;
    let inFlightAbort: AbortController | undefined;

    const requestPreview = (): void => {
        window.clearTimeout(debounceHandle);
        debounceHandle = window.setTimeout(() => {
            inFlightAbort?.abort();
            inFlightAbort = new AbortController();
            void fetchPreview(form, previewBody, activeSource.dataset.previewField, inFlightAbort.signal);
        }, DEBOUNCE_MS);
    };

    form.addEventListener('input', (event) => {
        if (event.target === activeSource) {
            requestPreview();
        }
    });

    // Any <select> in the editor (category, font style, ...) can affect the rendered prose class, so re-fetch on
    // any of them changing rather than hardcoding which fields matter for which content type.
    form.addEventListener('change', (event) => {
        if (event.target instanceof HTMLSelectElement) {
            requestPreview();
        }
    });

    tabs.forEach((tab) => {
        tab.addEventListener('click', () => {
            const nextSource = sources.find((source) => source.dataset.previewField === tab.dataset.previewTab);
            if (!nextSource || nextSource === activeSource) {
                return;
            }

            activeSource = nextSource;
            tabs.forEach((t) => t.classList.toggle('active', t === tab));
            requestPreview();
        });
    });

    document.addEventListener('booth:media-changed', requestPreview);

    requestPreview();
}

/**
 * Posts the form to the Preview handler and applies the result to the preview pane.
 * @param form The content edit form.
 * @param previewBody The element to render the previewed body into.
 * @param field Which source field to render, per the active preview tab; omitted on editors with a single source,
 * which the handler treats as its one and only field.
 * @param signal An abort signal that fires if a newer preview request supersedes this one — without it, a slower older request resolving
 * after a newer one would stomp the newer, correct preview.
 */
async function fetchPreview(form: HTMLFormElement, previewBody: HTMLElement, field: string | undefined, signal: AbortSignal): Promise<void> {
    const url = new URL(form.action);
    url.searchParams.set('handler', 'Preview');
    if (field) {
        url.searchParams.set('field', field);
    }

    try {
        const response = await fetch(url, {
            method: 'POST',
            body: new FormData(form),
            headers: {Accept: 'application/json'},
            signal,
        });

        if (!response.ok) {
            return;
        }

        const {html, proseClass} = await response.json() as PreviewResponse;
        previewBody.className = `prose ${proseClass}`;
        previewBody.innerHTML = html;

        initContentFeatures(previewBody);

        if (typeof MathJax !== 'undefined') {
            void MathJax.typesetPromise([previewBody]);
        }
    } catch (error) {
        if ((error as Error).name !== 'AbortError') {
            throw error;
        }
    }
}
