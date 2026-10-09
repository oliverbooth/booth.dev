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
 * The server round-trips these fields as UTC, so the value is deliberately not shifted to the browser's time zone.
 * @param date The Date object to convert.
 * @returns A string in the format "YYYY-MM-DDTHH:mm:ss.sss" representing the UTC date and time.
 */
function toDatetimeLocalValue(date: Date): string {
    return date.toISOString().slice(0, 23);
}
