/**
 * Initializes confirmation prompts for destructive form submissions. Any form marked
 * `data-confirm="..."` shows that text in a native confirm dialog before submitting, and the
 * submission is cancelled if the user declines.
 */
export function initConfirmForms(): void {
    const forms: NodeListOf<HTMLFormElement> = document.querySelectorAll<HTMLFormElement>('form[data-confirm]');
    for (const form of forms) {
        form.addEventListener('submit', event => {
            const message = form.dataset.confirm;
            if (message && !confirm(message)) {
                event.preventDefault();
            }
        });
    }
}
