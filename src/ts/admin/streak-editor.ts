/**
 * Mirrors the streak editor's tracking-mode select onto a `data-mode` attribute on the form, so CSS alone can show
 * only the fields relevant to that mode (cadence for check-in, start date for check-out).
 */
export function initStreakEditor(): void {
    const form = document.querySelector<HTMLFormElement>('[data-streak-form]');
    const modeSelect = form?.querySelector<HTMLSelectElement>('[data-streak-mode]');
    if (!form || !modeSelect) {
        return;
    }

    const sync = (): void => {
        form.dataset.mode = modeSelect.value;
    };

    modeSelect.addEventListener('change', sync);
    sync();
}
