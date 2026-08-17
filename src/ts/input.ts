/**
 * Register a keyboard shortcut.
 * @param shortcut The shortcut to register.
 * @param callback The callback to invoke when the shortcut is performed.
 */
export function registerShortcut(shortcut: string | string[], callback: () => void): void {
    const keys: string[] = typeof shortcut === 'string' ? shortcut.split(' ') : shortcut;

    let sequence: string[] = [];
    document.addEventListener('keydown', e => {
        sequence.push(e.key);
        sequence = sequence.slice(-keys.length);

        if (sequence.join(' ') === keys.join(' ')) {
            callback();
            sequence = [];
        }
    });
}
