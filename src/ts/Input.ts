class Input {
    public static readonly KONAMI_CODE = [
        "ArrowUp",
        "ArrowUp",
        "ArrowDown",
        "ArrowDown",
        "ArrowLeft",
        "ArrowRight",
        "ArrowLeft",
        "ArrowRight",
        "b",
        "a",
        "Enter"
    ];

    public static registerShortcut(shortcut: string | string[], callback: Function) {
        let keys: string[];
        if (typeof shortcut === 'string') keys = shortcut.split(' ');
        else keys = shortcut;

        let sequence: string[] = [];
        document.addEventListener('keydown', e => {
            sequence.push(e.key);

            if (sequence.slice(-keys.length).join(' ') === keys.join(' ')) {
                callback();
                sequence = [];
            }
        });
    }
}

export default Input;