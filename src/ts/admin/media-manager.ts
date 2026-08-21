type MediaKind = 'image' | 'video' | 'audio' | 'misc';

interface MediaFile {
    fileName: string;
    url: string | null;
    kind: MediaKind;
    sizeBytes: number | null;
    modifiedAt: string | null;
    missing: boolean;
}

interface MediaListResponse {
    files: MediaFile[];
}

const KIND_ICONS: Record<MediaKind, string> = {
    image: 'ti-photo',
    video: 'ti-video',
    audio: 'ti-music',
    misc: 'ti-file',
};

const REFRESH_DEBOUNCE_MS = 400;

/**
 * Initializes the media manager beneath an admin content editor's form (posts, notes, tutorials,
 * challenges, ...). Self-gates on the presence of its expected markup, so it's a no-op on editors
 * that don't have a media manager section.
 */
export function initMediaManager(): void {
    const section = document.querySelector<HTMLElement>('#content-media');
    const form = document.querySelector<HTMLFormElement>('.form-grid');
    const list = document.querySelector<HTMLUListElement>('#media-list');
    const bodyInput = document.querySelector<HTMLTextAreaElement>('textarea[data-preview-source]');
    const fileInput = document.querySelector<HTMLInputElement>('#media-file-input');
    const uploadButton = document.querySelector<HTMLButtonElement>('#media-upload-btn');
    const errorBox = document.querySelector<HTMLElement>('#media-error');

    if (!section || !form || !list || !bodyInput || !fileInput || !uploadButton || !errorBox) {
        return;
    }

    const showError = (message: string): void => {
        errorBox.textContent = message;
        errorBox.hidden = false;
    };

    const clearError = (): void => {
        errorBox.hidden = true;
    };

    uploadButton.addEventListener('click', () => fileInput.click());

    fileInput.addEventListener('change', () => {
        const files = [...(fileInput.files ?? [])];
        fileInput.value = ''; // reset so selecting the same file again still fires `change`
        void handleUpload(form, list, files, clearError, showError);
    });

    initDragAndDrop(section, form, list, clearError, showError);

    list.addEventListener('click', (event) => {
        const target = event.target as HTMLElement;
        const item = target.closest<HTMLLIElement>('.media-item');
        const fileName = item?.dataset.filename;
        if (!fileName) {
            return;
        }

        if (target.closest('.media-delete')) {
            void handleDelete(form, list, fileName, clearError, showError);
        } else if (target.closest('.media-rename')) {
            void handleRename(form, list, fileName, clearError, showError);
        }
    });

    let debounceHandle: number | undefined;
    form.addEventListener('input', (event) => {
        if (event.target !== bodyInput) {
            return;
        }

        window.clearTimeout(debounceHandle);
        debounceHandle = window.setTimeout(() => void refreshList(form, list, clearError, showError), REFRESH_DEBOUNCE_MS);
    });

    void refreshList(form, list, clearError, showError);
}

async function refreshList(
    form: HTMLFormElement,
    list: HTMLUListElement,
    clearError: () => void,
    showError: (message: string) => void
): Promise<void> {
    try {
        const response = await postMediaAction(handlerUrl(form, 'ListMedia'), new FormData(form));
        clearError();
        renderMediaList(list, response.files);
    } catch (error) {
        showError((error as Error).message);
    }
}

/**
 * Initializes drag-and-drop file upload for the media manager section.
 * @param section The media manager section element.
 * @param form The post edit form.
 * @param list The `<ul>` element containing the list of media files.
 * @param clearError A function to clear any error messages.
 * @param showError A function to display an error message.
 */
function initDragAndDrop(
    section: HTMLElement,
    form: HTMLFormElement,
    list: HTMLUListElement,
    clearError: () => void,
    showError: (message: string) => void
): void {
    let dragDepth = 0;

    section.addEventListener('dragenter', (event) => {
        event.preventDefault();
        dragDepth++;
        section.classList.add('admin-media--dragover');
    });

    section.addEventListener('dragover', (event) => {
        // Required for `drop` to fire at all — browsers otherwise treat the element as a non-drop target.
        event.preventDefault();
    });

    section.addEventListener('dragleave', () => {
        dragDepth = Math.max(0, dragDepth - 1);
        if (dragDepth === 0) {
            section.classList.remove('admin-media--dragover');
        }
    });

    section.addEventListener('drop', (event) => {
        event.preventDefault();
        dragDepth = 0;
        section.classList.remove('admin-media--dragover');

        const files = [...(event.dataTransfer?.files ?? [])];
        if (files.length > 0) {
            void handleUpload(form, list, files, clearError, showError);
        }
    });
}

async function handleUpload(
    form: HTMLFormElement,
    list: HTMLUListElement,
    files: File[],
    clearError: () => void,
    showError: (message: string) => void
): Promise<void> {
    for (const file of files) {
        const pendingItem = buildPendingRow(file.name);
        list.append(pendingItem);

        try {
            const formData = new FormData(form);
            formData.set('file', file);

            const response = await uploadWithProgress(handlerUrl(form, 'UploadMedia'), formData, (fraction) => {
                const bar = pendingItem.querySelector<HTMLElement>('.media-item-progress span');
                if (bar) {
                    bar.style.width = `${Math.round(fraction * 100)}%`;
                }
            });

            clearError();
            renderMediaList(list, response.files);
            notifyMediaChanged();
        } catch (error) {
            pendingItem.remove();
            showError((error as Error).message);
        }
    }
}

async function handleDelete(
    form: HTMLFormElement,
    list: HTMLUListElement,
    fileName: string,
    clearError: () => void,
    showError: (message: string) => void
): Promise<void> {
    if (!confirm(`Delete "${fileName}"? This can't be undone.`)) {
        return;
    }

    try {
        const formData = new FormData(form);
        formData.set('fileName', fileName);

        const response = await postMediaAction(handlerUrl(form, 'DeleteMedia'), formData);
        clearError();
        renderMediaList(list, response.files);
        notifyMediaChanged();
    } catch (error) {
        showError((error as Error).message);
    }
}

async function handleRename(
    form: HTMLFormElement,
    list: HTMLUListElement,
    fileName: string,
    clearError: () => void,
    showError: (message: string) => void
): Promise<void> {
    const newFileName = prompt('Rename file (extension must stay the same):', fileName);
    if (!newFileName || newFileName === fileName) {
        return;
    }

    try {
        const formData = new FormData(form);
        formData.set('fileName', fileName);
        formData.set('newFileName', newFileName);

        const response = await postMediaAction(handlerUrl(form, 'RenameMedia'), formData);
        clearError();
        renderMediaList(list, response.files);
        notifyMediaChanged();
    } catch (error) {
        showError((error as Error).message);
    }
}

/**
 * Invalidates the live preview's media cache, triggering a re-query of the server for the list of files.
 */
function notifyMediaChanged(): void {
    document.dispatchEvent(new CustomEvent('booth:media-changed'));
}

/**
 * Builds the URL for a named page handler on the post edit form.
 * @param form The post edit form.
 * @param handler The page handler name.
 * @returns The URL to post to.
 */
function handlerUrl(form: HTMLFormElement, handler: string): string {
    const url = new URL(form.action);
    url.searchParams.set('handler', handler);
    return url.toString();
}

async function postMediaAction(url: string, formData: FormData): Promise<MediaListResponse> {
    const response = await fetch(url, {method: 'POST', body: formData, headers: {Accept: 'application/json'}});
    const body: unknown = await response.json().catch(() => null);

    if (!response.ok) {
        throw new Error(extractErrorMessage(body));
    }

    return body as MediaListResponse;
}

function uploadWithProgress(
    url: string,
    formData: FormData,
    onProgress: (fraction: number) => void
): Promise<MediaListResponse> {
    return new Promise((resolve, reject) => {
        const xhr = new XMLHttpRequest();
        xhr.open('POST', url);
        xhr.responseType = 'json';
        xhr.setRequestHeader('Accept', 'application/json');

        xhr.upload.addEventListener('progress', (event) => {
            if (event.lengthComputable) {
                onProgress(event.loaded / event.total);
            }
        });

        xhr.addEventListener('load', () => {
            if (xhr.status >= 200 && xhr.status < 300) {
                resolve(xhr.response as MediaListResponse);
            } else {
                reject(new Error(extractErrorMessage(xhr.response)));
            }
        });

        xhr.addEventListener('error', () => reject(new Error('A network error interrupted the upload.')));
        xhr.send(formData);
    });
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

function renderMediaList(list: HTMLUListElement, files: MediaFile[]): void {
    list.innerHTML = '';

    if (files.length === 0) {
        const empty = document.createElement('li');
        empty.className = 'media-item media-item--pending';
        empty.textContent = 'No files uploaded yet.';
        list.append(empty);
        return;
    }

    for (const file of files) {
        list.append(buildMediaRow(file));
    }
}

/**
 * Builds a row for a media file, either as an uploaded file or a missing reference.
 * @param file The media file for which to build a row.
 * @returns The `<li>` element representing the media file.
 */
function buildMediaRow(file: MediaFile): HTMLLIElement {
    return file.missing ? buildMissingRow(file) : buildUploadedRow(file);
}

/**
 * Builds a row for a file that has been uploaded and is present in the media manager.
 * @param file The uploaded file.
 * @returns The `<li>` element representing the uploaded file.
 */
function buildUploadedRow(file: MediaFile): HTMLLIElement {
    const item = document.createElement('li');
    item.className = 'media-item';
    item.dataset.filename = file.fileName;

    const icon = document.createElement('div');
    icon.className = 'media-item-icon';
    icon.innerHTML = `<i class="ti ${KIND_ICONS[file.kind] ?? KIND_ICONS.misc}"></i>`;

    const body = document.createElement('div');
    body.className = 'media-item-body';

    const name = document.createElement('p');
    name.className = 'name';
    name.textContent = file.fileName;

    const meta = document.createElement('p');
    meta.className = 'meta';
    meta.textContent = `${file.kind} · ${formatFileSize(file.sizeBytes ?? 0)}`;

    body.append(name, meta);

    const actions = document.createElement('div');
    actions.className = 'row-actions';

    const viewLink = document.createElement('a');
    viewLink.className = 'icon-btn';
    viewLink.href = file.url ?? '#';
    viewLink.target = '_blank';
    viewLink.rel = 'noopener';
    viewLink.setAttribute('aria-label', `View ${file.fileName}`);
    viewLink.innerHTML = '<i class="ti ti-external-link"></i>';

    const renameButton = document.createElement('button');
    renameButton.type = 'button';
    renameButton.className = 'icon-btn media-rename';
    renameButton.setAttribute('aria-label', `Rename ${file.fileName}`);
    renameButton.innerHTML = '<i class="ti ti-edit"></i>';

    const deleteButton = document.createElement('button');
    deleteButton.type = 'button';
    deleteButton.className = 'icon-btn danger media-delete';
    deleteButton.setAttribute('aria-label', `Delete ${file.fileName}`);
    deleteButton.innerHTML = '<i class="ti ti-trash"></i>';

    actions.append(viewLink, renameButton, deleteButton);
    item.append(icon, body, actions);

    return item;
}

/**
 * Builds a row for a file the body references (via `![alt](file)` or `![[file]]`) but that hasn't actually been uploaded yet.
 * @param file The missing file.
 * @returns The `<li>` element representing the missing file.
 */
function buildMissingRow(file: MediaFile): HTMLLIElement {
    const item = document.createElement('li');
    item.className = 'media-item media-item--missing';

    const icon = document.createElement('div');
    icon.className = 'media-item-icon';
    icon.innerHTML = '<i class="ti ti-alert-triangle"></i>';

    const body = document.createElement('div');
    body.className = 'media-item-body';

    const name = document.createElement('p');
    name.className = 'name';
    name.textContent = file.fileName;

    const meta = document.createElement('p');
    meta.className = 'meta';
    meta.textContent = 'Referenced in the body, but not uploaded';

    body.append(name, meta);

    const badge = document.createElement('span');
    badge.className = 'badge badge-missing';
    badge.textContent = 'Missing';

    item.append(icon, body, badge);

    return item;
}

/**
 * Builds a row for a file that is currently being uploaded, showing a progress bar.
 * @param fileName The name of the file being uploaded.
 * @returns The `<li>` element representing the pending upload.
 */
function buildPendingRow(fileName: string): HTMLLIElement {
    const item = document.createElement('li');
    item.className = 'media-item media-item--pending';

    const icon = document.createElement('div');
    icon.className = 'media-item-icon';
    icon.innerHTML = '<i class="ti ti-upload"></i>';

    const body = document.createElement('div');
    body.className = 'media-item-body';

    const name = document.createElement('p');
    name.className = 'name';
    name.textContent = fileName;

    const progress = document.createElement('div');
    progress.className = 'media-item-progress';
    progress.innerHTML = '<span></span>';

    body.append(name, progress);
    item.append(icon, body);

    return item;
}

/**
 * Formats a file size in bytes into a human-readable string with appropriate units (B, KiB, MiB, GiB).
 * @param bytes The file size in bytes.
 * @returns A formatted string representing the file size with units.
 */
function formatFileSize(bytes: number): string {
    if (bytes < 1024) {
        return `${bytes} B`;
    }

    const units = ['KiB', 'MiB', 'GiB'];
    let value = bytes / 1024;
    let unitIndex = 0;

    while (value >= 1024 && unitIndex < units.length - 1) {
        value /= 1024;
        unitIndex++;
    }

    return `${value.toFixed(value < 10 ? 1 : 0)} ${units[unitIndex]}`;
}
