import {initMarkdownEditors} from './markdown-editor.ts';

/**
 * Initializes the post authoring interface.
 */
export function initPostAuthoring(): void {
    initMarkdownEditors();
    initSlugGenerator();
    initSetNowButton();
}

/**
 * Initializes the slug generator functionality for the post authoring interface.
 */
function initSlugGenerator(): void {
    const titleInput: HTMLInputElement | null = document.querySelector<HTMLInputElement>('#title');
    const slugInput: HTMLInputElement | null = document.querySelector<HTMLInputElement>('#slug');
    const generateButton: HTMLButtonElement | null = document.querySelector<HTMLButtonElement>('#generate-slug');
    const slugPreview: HTMLElement | null = document.querySelector<HTMLElement>('#slug-preview');

    if (!titleInput || !slugInput || !generateButton) {
        return;
    }

    generateButton.addEventListener('click', () => {
        slugInput.value = kebaberize(titleInput.value);
        if (slugPreview) {
            slugPreview.textContent = `booth.dev/blog/${slugInput.value}`;
        }
    });
}

/**
 * Initializes the "Set Now" button functionality for the post authoring interface.
 */
function initSetNowButton(): void {
    const dateInput = document.querySelector<HTMLInputElement>('#date');
    const setNowButton = document.querySelector<HTMLButtonElement>('#set-now');

    if (!dateInput || !setNowButton) {
        return;
    }

    setNowButton.addEventListener('click', () => {
        dateInput.value = toDatetimeLocalValue(new Date());
    });
}

/**
 * Converts a string into a kebab-case slug.
 * @param input The input string to convert.
 * @returns The kebab-case version of the input string.
 */
function kebaberize(input: string): string {
    return input
        .trim()
        .toLowerCase()
        .replace(/[^\p{L}\p{N}]+/gu, '-')
        .replace(/^-+|-+$/g, '');
}

/**
 * Converts a Date object to a string suitable for a datetime-local input field.
 * @param date The Date object to convert.
 * @returns A string in the format "YYYY-MM-DDTHH:mm:ss.sss" representing the local date and time.
 */
function toDatetimeLocalValue(date: Date): string {
    const offsetMs = date.getTimezoneOffset() * 60_000;
    const localIso = new Date(date.getTime() - offsetMs).toISOString();
    return localIso.slice(0, 23); // "YYYY-MM-DDTHH:mm:ss.sss" format
}
