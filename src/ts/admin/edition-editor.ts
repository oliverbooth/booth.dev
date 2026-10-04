/**
 * Wires up the "other editions" list on the game list edit form: adds and removes edition blocks, and keeps their
 * indexed field names contiguous so ASP.NET's list model binding sees every one of them.
 */
export function initEditionEditor(): void {
    const root = document.querySelector<HTMLElement>('[data-editions]');
    const list = root?.querySelector<HTMLElement>('[data-edition-list]');
    const template = root?.querySelector<HTMLTemplateElement>('[data-edition-template]');
    const addButton = root?.querySelector<HTMLButtonElement>('[data-edition-add]');

    if (!root || !list || !template || !addButton) {
        return;
    }

    const renumber = (): void => {
        list.querySelectorAll<HTMLElement>('[data-edition-block]').forEach((block, index) => {
            for (const field of block.querySelectorAll<HTMLInputElement>('[name^="Input.Editions["]')) {
                field.name = field.name.replace(/^Input\.Editions\[[^\]]*\]/, `Input.Editions[${index}]`);
            }
        });
    };

    addButton.addEventListener('click', () => {
        list.append(template.content.cloneNode(true));
        renumber();
        list.querySelector<HTMLInputElement>('[data-edition-block]:last-child input[type="text"]')?.focus();
    });

    list.addEventListener('click', (event) => {
        const remove = (event.target as HTMLElement).closest('[data-edition-remove]');
        if (!remove) {
            return;
        }

        remove.closest('[data-edition-block]')?.remove();
        renumber();
    });
}
