interface LightboxRefs {
    dialog: HTMLDialogElement;
    image: HTMLImageElement;
    caption: HTMLElement;
    closeButton: HTMLButtonElement;
}

const LIGHTBOX_SELECTOR = '#lightbox';
const TRIGGER_SELECTOR = '[data-lightbox]';

let refs: LightboxRefs | null = null;
let lastFocusedTrigger: HTMLElement | null = null;

/**
 * Initializes the lightbox component.
 */
export function initLightbox(): void {
    const dialog: HTMLDialogElement | null = document.querySelector<HTMLDialogElement>(LIGHTBOX_SELECTOR);
    if (!dialog) {
        return;
    }

    const image: HTMLImageElement | null = dialog.querySelector<HTMLImageElement>('.lightbox-image');
    const caption: HTMLElement | null = dialog.querySelector<HTMLElement>('.lightbox-caption');
    const closeButton: HTMLButtonElement | null = dialog.querySelector<HTMLButtonElement>('.lightbox-close');

    if (!image || !caption || !closeButton) {
        throw new Error('Lightbox markup is missing required child elements.');
    }

    refs = {dialog, image, caption, closeButton};

    document.addEventListener('click', onDocumentClick);
    closeButton.addEventListener('click', () => close());

    dialog.addEventListener('click', event => {
        if (event.target === dialog) {
            close();
        }
    });

    dialog.addEventListener('close', onDialogClose);
}

function onDocumentClick(event: MouseEvent): void {
    const trigger = (event.target as HTMLElement).closest<HTMLElement>(TRIGGER_SELECTOR);
    if (!trigger) {
        return;
    }

    open(trigger);
}

function open(trigger: HTMLElement): void {
    if (!refs) {
        return;
    }

    const src: string = trigger.dataset.lightboxSrc ?? (trigger as HTMLImageElement).src;
    const captionTemplate: HTMLTemplateElement | null | undefined = trigger
        .closest('figure')
        ?.querySelector<HTMLTemplateElement>('[data-lightbox-caption-template]');

    refs.image.src = src;
    refs.image.alt = (trigger as HTMLImageElement).alt ?? '';

    refs.caption.replaceChildren();
    if (captionTemplate) {
        refs.caption.appendChild(captionTemplate.content.cloneNode(true));
    }
    refs.caption.hidden = !captionTemplate;

    lastFocusedTrigger = trigger;
    refs.dialog.showModal();
    refs.closeButton.focus();
}

function close(): void {
    if (!refs) {
        return;
    }

    refs.dialog.classList.add('is-closing');
    refs.dialog.addEventListener(
        'transitionend',
        () => {
            refs?.dialog.classList.remove('is-closing');
            refs?.dialog.close();
        },
        { once: true }
    );
}

function onDialogClose(): void {
    if (!refs) {
        return;
    }

    refs.image.src = '';
    lastFocusedTrigger?.focus();
    lastFocusedTrigger = null;
}
