interface LightboxRefs {
    dialog: HTMLDialogElement;
    image: HTMLImageElement;
    videoSlot: HTMLElement;
    caption: HTMLElement;
    closeButton: HTMLButtonElement;
}

interface MovedVideo {
    element: HTMLVideoElement;
    parent: Node;
    nextSibling: Node | null;
}

const LIGHTBOX_SELECTOR = '#lightbox';
const TRIGGER_SELECTOR = '[data-lightbox]';

let refs: LightboxRefs | null = null;
let lastFocusedTrigger: HTMLElement | null = null;
let movedVideo: MovedVideo | null = null;

/**
 * Initializes the lightbox component.
 */
export function initLightbox(): void {
    const dialog: HTMLDialogElement | null = document.querySelector<HTMLDialogElement>(LIGHTBOX_SELECTOR);
    if (!dialog) {
        return;
    }

    const image: HTMLImageElement | null = dialog.querySelector<HTMLImageElement>('.lightbox-image');
    const videoSlot: HTMLElement | null = dialog.querySelector<HTMLElement>('.lightbox-video-slot');
    const caption: HTMLElement | null = dialog.querySelector<HTMLElement>('.lightbox-caption');
    const closeButton: HTMLButtonElement | null = dialog.querySelector<HTMLButtonElement>('.lightbox-close');

    if (!image || !videoSlot || !caption || !closeButton) {
        throw new Error('Lightbox markup is missing required child elements.');
    }

    refs = {dialog, image, videoSlot, caption, closeButton};

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

    if (trigger.dataset.lightbox === 'video') {
        openVideo(trigger);
    } else {
        openImage(trigger);
    }

    const captionTemplate: HTMLTemplateElement | null | undefined = trigger
        .closest('figure')
        ?.querySelector<HTMLTemplateElement>('[data-lightbox-caption-template]');

    refs.caption.replaceChildren();
    if (captionTemplate) {
        refs.caption.appendChild(captionTemplate.content.cloneNode(true));
    }
    refs.caption.hidden = !captionTemplate;

    lastFocusedTrigger = trigger;
    refs.dialog.showModal();
    refs.closeButton.focus();
}

function openImage(trigger: HTMLElement): void {
    if (!refs) {
        return;
    }

    const src: string = trigger.dataset.lightboxSrc ?? (trigger as HTMLImageElement).src;
    refs.image.src = src;
    refs.image.alt = (trigger as HTMLImageElement).alt ?? '';
    refs.image.hidden = false;
    refs.videoSlot.hidden = true;
}

function openVideo(trigger: HTMLElement): void {
    if (!refs) {
        return;
    }

    const video = trigger.closest('.figure-img-wrap')?.querySelector<HTMLVideoElement>('video');
    if (!video || !video.parentNode) {
        return;
    }

    // move (not clone) the real element, so an in-progress playback carries over into the modal untouched.
    movedVideo = {element: video, parent: video.parentNode, nextSibling: video.nextSibling};
    refs.videoSlot.appendChild(video);
    refs.videoSlot.hidden = false;
    refs.image.hidden = true;
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

    if (movedVideo) {
        movedVideo.element.pause();
        movedVideo.parent.insertBefore(movedVideo.element, movedVideo.nextSibling);
        movedVideo = null;
    }

    lastFocusedTrigger?.focus();
    lastFocusedTrigger = null;
}
