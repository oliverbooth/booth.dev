import {initContentFeatures} from '../content-rendering.ts';

const DEBOUNCE_MS = 400;

interface PreviewResponse {
    html: string;
    proseClass: string;
}

/**
 * Initializes the live preview pane for the post authoring interface.
 */
export function initLivePreview(): void {
    const editorRoot = document.querySelector<HTMLElement>('#post-editor');
    const form = document.querySelector<HTMLFormElement>('.form-grid');
    const pane = document.querySelector<HTMLElement>('#preview-pane');
    const titleInput = document.querySelector<HTMLInputElement>('#title');
    const bodyInput = document.querySelector<HTMLTextAreaElement>('#body');
    const categorySelect = document.querySelector<HTMLSelectElement>('#category');
    const previewTitle = document.querySelector<HTMLElement>('#preview-title');
    const previewBody = document.querySelector<HTMLElement>('#preview-body');

    if (!editorRoot || !form || !pane || !bodyInput || !previewTitle || !previewBody) {
        return;
    }

    editorRoot.classList.add('admin-page--split');
    pane.hidden = false;

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
            void fetchPreview(form, previewBody, inFlightAbort.signal);
        }, DEBOUNCE_MS);
    };

    form.addEventListener('input', (event) => {
        if (event.target === bodyInput) {
            requestPreview();
        }
    });
    categorySelect?.addEventListener('change', requestPreview);

    document.addEventListener('booth:media-changed', requestPreview);

    requestPreview();
}

/**
 * Posts the form to the Preview handler and applies the result to the preview pane.
 * @param form The post edit form.
 * @param previewBody The element to render the previewed body into.
 * @param signal An abort signal that fires if a newer preview request supersedes this one — without it, a slower older request resolving
 * after a newer one would stomp the newer, correct preview.
 */
async function fetchPreview(form: HTMLFormElement, previewBody: HTMLElement, signal: AbortSignal): Promise<void> {
    const url = new URL(form.action);
    url.searchParams.set('handler', 'Preview');

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
    } catch (error) {
        if ((error as Error).name !== 'AbortError') {
            throw error;
        }
    }
}