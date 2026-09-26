/**
 * Wires up the admin sidebar as an off-canvas drawer on narrow screens, where it's hidden behind the top bar's menu
 * button. The drawer closes on Escape, on a tap outside it, and when a link inside it is followed.
 */
export function initNavDrawer(): void {
    const shell = document.querySelector<HTMLElement>('.admin-shell');
    const toggle = shell?.querySelector<HTMLButtonElement>('[data-admin-nav-toggle]');
    if (!shell || !toggle) {
        return;
    }

    const setOpen = (open: boolean): void => {
        shell.toggleAttribute('data-nav-open', open);
        toggle.setAttribute('aria-expanded', String(open));
    };

    toggle.addEventListener('click', () => setOpen(!shell.hasAttribute('data-nav-open')));
    shell.querySelector('[data-admin-nav-scrim]')?.addEventListener('click', () => setOpen(false));
    shell.querySelector('.admin-rail')?.addEventListener('click', event => {
        if ((event.target as Element).closest('a')) {
            setOpen(false);
        }
    });

    document.addEventListener('keydown', event => {
        if (event.key === 'Escape' && shell.hasAttribute('data-nav-open')) {
            setOpen(false);
            toggle.focus();
        }
    });
}
