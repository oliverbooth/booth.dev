const MOVE_MIME = 'application/x-cdn-entry';

interface DeletePreview {
    itemCount: number;
    capped: boolean;
}

/**
 * Initializes the admin CDN file browser: upload (button + desktop drag-and-drop), new folder, rename,
 * move (row-to-row drag-and-drop + an explicit "type a path" control), and delete.
 */
export function initCdnBrowser(): void {
    const container = document.querySelector<HTMLElement>('#cdn-browser');
    const form = document.querySelector<HTMLFormElement>('#cdn-form');
    const table = document.querySelector<HTMLTableElement>('#cdn-table');
    const errorBox = document.querySelector<HTMLElement>('#cdn-error');
    const newFolderButton = document.querySelector<HTMLButtonElement>('#cdn-new-folder-btn');
    const uploadButton = document.querySelector<HTMLButtonElement>('#cdn-upload-btn');
    const fileInput = document.querySelector<HTMLInputElement>('#cdn-file-input');

    if (!container || !form || !table || !errorBox || !newFolderButton || !uploadButton || !fileInput) {
        return;
    }

    const showError = (message: string): void => {
        errorBox.textContent = message;
        errorBox.hidden = false;
    };

    const clearError = (): void => {
        errorBox.hidden = true;
    };

    const currentPath = (): string => container.dataset.currentPath ?? '/';

    newFolderButton.addEventListener('click', () => void handleNewFolder(form, clearError, showError));
    uploadButton.addEventListener('click', () => fileInput.click());

    fileInput.addEventListener('change', () => {
        const files = [...(fileInput.files ?? [])];
        fileInput.value = ''; // reset so selecting the same file again still fires `change`
        void handleUpload(form, files, clearError, showError);
    });

    table.addEventListener('click', (event) => {
        const button = (event.target as HTMLElement).closest<HTMLButtonElement>('button[data-action]');
        const row = button?.closest<HTMLTableRowElement>('tr[data-name]');
        if (!button || !row) {
            return;
        }

        switch (button.dataset.action) {
            case 'rename':
                void handleRename(form, row, clearError, showError);
                break;
            case 'move':
                void handleMovePrompt(form, row, currentPath(), clearError, showError);
                break;
            case 'delete':
                void handleDelete(form, row, clearError, showError);
                break;
        }
    });

    initUploadDropzone(container, files => void handleUpload(form, files, clearError, showError));
    initRowDragMove(table, currentPath(), (name, destination) => void performMove(form, name, destination, clearError, showError));
}

/**
 * Initializes upload-by-dropping-desktop-files-onto-the-page.
 * @param container The element to listen for drag/drop events on.
 * @param onDrop Callback invoked with the dropped files.
 */
function initUploadDropzone(container: HTMLElement, onDrop: (files: File[]) => void): void {
    let dragDepth = 0;
    const isFileDrag = (event: DragEvent): boolean => event.dataTransfer?.types.includes('Files') ?? false;

    container.addEventListener('dragenter', (event) => {
        if (!isFileDrag(event)) {
            return;
        }
        event.preventDefault();
        dragDepth++;
        container.classList.add('cdn-browser--dragover');
    });

    container.addEventListener('dragover', (event) => {
        // required for `drop` to fire at all — browsers otherwise treat the element as a non-drop target.
        if (isFileDrag(event)) {
            event.preventDefault();
        }
    });

    container.addEventListener('dragleave', (event) => {
        if (!isFileDrag(event)) {
            return;
        }
        dragDepth = Math.max(0, dragDepth - 1);
        if (dragDepth === 0) {
            container.classList.remove('cdn-browser--dragover');
        }
    });

    container.addEventListener('drop', (event) => {
        if (!isFileDrag(event)) {
            return;
        }
        event.preventDefault();
        dragDepth = 0;
        container.classList.remove('cdn-browser--dragover');

        const files = [...(event.dataTransfer?.files ?? [])];
        if (files.length > 0) {
            onDrop(files);
        }
    });
}

/**
 * Initializes dragging a row onto a folder row to move it there. Keyed off a custom MIME type that an OS
 * file drag never carries, so this never fires for a desktop-file-onto-page upload drag.
 * @param table The table on which to listen for drag/drop events.
 * @param initialPath The current folder path, used to construct the destination path.
 * @param onMove Callback invoked with the source name and destination path when a row is dropped onto a folder row.
 */
function initRowDragMove(table: HTMLTableElement, initialPath: string, onMove: (name: string, destination: string) => void): void {
    table.addEventListener('dragstart', (event) => {
        const row = (event.target as HTMLElement).closest<HTMLTableRowElement>('tr[data-name]');
        if (!row || !event.dataTransfer) {
            return;
        }
        event.dataTransfer.setData(MOVE_MIME, row.dataset.name ?? '');
        event.dataTransfer.effectAllowed = 'move';
    });

    let overTarget: HTMLTableRowElement | null = null;
    const clearOverTarget = (): void => {
        overTarget?.classList.remove('cdn-row--drop-target');
        overTarget = null;
    };

    table.addEventListener('dragover', (event) => {
        if (!event.dataTransfer?.types.includes(MOVE_MIME)) {
            return;
        }
        const folderRow = (event.target as HTMLElement).closest<HTMLTableRowElement>('tr[data-kind="folder"]');
        if (!folderRow) {
            clearOverTarget();
            return;
        }
        event.preventDefault(); // required for drop to fire
        if (overTarget !== folderRow) {
            clearOverTarget();
            overTarget = folderRow;
            overTarget.classList.add('cdn-row--drop-target');
        }
    });

    table.addEventListener('dragleave', (event) => {
        const related = event.relatedTarget as Node | null;
        if (!related || !table.contains(related)) {
            clearOverTarget();
        }
    });

    table.addEventListener('drop', (event) => {
        if (!event.dataTransfer?.types.includes(MOVE_MIME)) {
            return;
        }
        const folderRow = (event.target as HTMLElement).closest<HTMLTableRowElement>('tr[data-kind="folder"]');
        clearOverTarget();
        if (!folderRow) {
            return;
        }
        event.preventDefault();

        const sourceName = event.dataTransfer.getData(MOVE_MIME);
        const targetName = folderRow.dataset.name;
        if (!sourceName || !targetName || sourceName === targetName) {
            return; // dropped on itself - no-op
        }

        const destination = initialPath === '/' ? `/${targetName}` : `${initialPath}/${targetName}`;
        onMove(sourceName, destination);
    });
}

async function handleNewFolder(
    form: HTMLFormElement,
    clearError: () => void,
    showError: (message: string) => void
): Promise<void> {
    const name = prompt('New folder name:');
    if (!name) {
        return;
    }

    clearError();
    try {
        const formData = new FormData(form);
        formData.set('name', name);
        await postJson(handlerUrl(form, 'NewFolder'), formData);
        location.reload();
    } catch (error) {
        showError((error as Error).message);
    }
}

async function handleUpload(
    form: HTMLFormElement,
    files: File[],
    clearError: () => void,
    showError: (message: string) => void
): Promise<void> {
    if (files.length === 0) {
        return;
    }

    clearError();
    const failures: string[] = [];

    for (const file of files) {
        try {
            const formData = new FormData(form);
            formData.set('file', file);
            await postJson(handlerUrl(form, 'Upload'), formData);
        } catch (error) {
            failures.push(`${file.name}: ${(error as Error).message}`);
        }
    }

    if (failures.length > 0) {
        showError(failures.join(' '));
        return;
    }

    location.reload();
}

async function handleRename(
    form: HTMLFormElement,
    row: HTMLTableRowElement,
    clearError: () => void,
    showError: (message: string) => void
): Promise<void> {
    const name = row.dataset.name;
    if (!name) {
        return;
    }

    const newName = prompt('Rename to:', name);
    if (!newName || newName === name) {
        return;
    }

    clearError();
    try {
        const formData = new FormData(form);
        formData.set('name', name);
        formData.set('newName', newName);
        await postJson(handlerUrl(form, 'Rename'), formData);
        location.reload();
    } catch (error) {
        showError((error as Error).message);
    }
}

async function handleMovePrompt(
    form: HTMLFormElement,
    row: HTMLTableRowElement,
    currentPath: string,
    clearError: () => void,
    showError: (message: string) => void
): Promise<void> {
    const name = row.dataset.name;
    if (!name) {
        return;
    }

    const destination = prompt(`Move "${name}" to (folder path):`, currentPath);
    if (!destination) {
        return;
    }

    await performMove(form, name, destination, clearError, showError);
}

async function performMove(
    form: HTMLFormElement,
    name: string,
    destination: string,
    clearError: () => void,
    showError: (message: string) => void
): Promise<void> {
    clearError();
    try {
        const formData = new FormData(form);
        formData.set('name', name);
        formData.set('destination', destination);
        await postJson(handlerUrl(form, 'Move'), formData);
        location.reload();
    } catch (error) {
        showError((error as Error).message);
    }
}

async function handleDelete(
    form: HTMLFormElement,
    row: HTMLTableRowElement,
    clearError: () => void,
    showError: (message: string) => void
): Promise<void> {
    const name = row.dataset.name;
    if (!name) {
        return;
    }

    if (row.dataset.kind === 'folder') {
        let preview: DeletePreview;
        try {
            const formData = new FormData(form);
            formData.set('name', name);
            preview = await postJson<DeletePreview>(handlerUrl(form, 'DeletePreview'), formData);
        } catch (error) {
            showError((error as Error).message);
            return;
        }

        if (preview.itemCount > 0) {
            const label = preview.capped ? `${preview.itemCount}+` : `${preview.itemCount}`;
            const typed = prompt(
                `This folder contains ${label} item(s). Type "${name}" to permanently delete it and everything inside. This can't be undone.`
            );
            if (typed !== name) {
                return;
            }
        } else if (!confirm(`Delete empty folder "${name}"? This can't be undone.`)) {
            return;
        }
    } else if (!confirm(`Delete "${name}"? This can't be undone.`)) {
        return;
    }

    clearError();
    try {
        const formData = new FormData(form);
        formData.set('name', name);
        await postJson(handlerUrl(form, 'Delete'), formData);
        location.reload();
    } catch (error) {
        showError((error as Error).message);
    }
}

/**
 * Builds the URL for a named page handler on the CDN browser form, preserving the current `?path=`.
 * @param form The form element to use for the base URL and query parameters.
 * @param handler The name of the page handler to invoke.
 * @returns The full URL for the page handler.
 */
function handlerUrl(form: HTMLFormElement, handler: string): string {
    const url = new URL(form.action);
    url.searchParams.set('handler', handler);
    return url.toString();
}

async function postJson<T = { ok: boolean }>(url: string, formData: FormData): Promise<T> {
    const response = await fetch(url, {method: 'POST', body: formData, headers: {Accept: 'application/json'}});
    const body: unknown = await response.json().catch(() => null);

    if (!response.ok) {
        throw new Error(extractErrorMessage(body));
    }

    return body as T;
}

function extractErrorMessage(body: unknown): string {
    if (typeof body === 'string') {
        return body;
    }

    if (Array.isArray(body)) {
        return body.join(' ');
    }

    return 'Something went wrong.';
}
