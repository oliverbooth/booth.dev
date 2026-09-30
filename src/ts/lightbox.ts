import {resetTerminal} from './terminal-chrome.ts';

interface LightboxRefs {
    dialog: HTMLDialogElement;
    image: HTMLImageElement;
    videoSlot: HTMLElement;
    terminalSlot: HTMLElement;
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
let flightSource: HTMLElement | null = null;
let closing = false;

const FLIGHT_MS = 380;
const FLIGHT_EASING = 'cubic-bezier(0.2, 0.8, 0.2, 1)';

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
    const terminalSlot: HTMLElement | null = dialog.querySelector<HTMLElement>('.lightbox-terminal-slot');
    const caption: HTMLElement | null = dialog.querySelector<HTMLElement>('.lightbox-caption');
    const closeButton: HTMLButtonElement | null = dialog.querySelector<HTMLButtonElement>('.lightbox-close');

    if (!image || !videoSlot || !terminalSlot || !caption || !closeButton) {
        throw new Error('Lightbox markup is missing required child elements.');
    }

    refs = {dialog, image, videoSlot, terminalSlot, caption, closeButton};

    document.addEventListener('click', onDocumentClick);
    closeButton.addEventListener('click', () => close());

    dialog.addEventListener('click', event => {
        if (event.target === dialog) {
            close();
        }
    });

    dialog.addEventListener('close', onDialogClose);
    dialog.addEventListener('cancel', event => {
        event.preventDefault();
        close();
    });
}

function onDocumentClick(event: MouseEvent): void {
    const target = event.target as HTMLElement;
    if (target.closest('[data-lightbox-close]')) {
        close();
        return;
    }

    const trigger = target.closest<HTMLElement>(TRIGGER_SELECTOR);
    if (!trigger) {
        return;
    }

    if (refs?.dialog.contains(trigger)) {
        close();
        return;
    }

    open(trigger);
}

async function open(trigger: HTMLElement): Promise<void> {
    if (!refs || refs.dialog.open) {
        return;
    }

    const fly: boolean = !prefersReducedMotion();
    const source: HTMLElement = flightSourceFor(trigger);
    const from: DOMRect = visibleRect(source);

    switch (trigger.dataset.lightbox) {
        case 'video':
            openVideo(trigger);
            break;
        case 'terminal':
            openTerminal(trigger);
            break;
        default:
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
    refs.dialog.classList.toggle('lightbox--terminal', trigger.dataset.lightbox === 'terminal');
    refs.dialog.classList.toggle('lightbox--voice', usesVoiceFont(trigger));

    lastFocusedTrigger = trigger;
    closing = false;

    if (fly && trigger.dataset.lightbox !== 'video') {
        source.style.visibility = 'hidden';
        flightSource = source;
    }

    // a decoded image has real dimensions at layout time, so the landing rect is right on the first frame
    if (fly && !refs.image.hidden) {
        await refs.image.decode().catch(() => undefined);
    }

    refs.dialog.classList.toggle('lightbox--fly', fly);
    refs.dialog.showModal();
    refs.closeButton.focus();

    if (fly) {
        const target: HTMLElement = flightTarget();
        fling(target, from, visibleRect(target), 'in', source);
        refs.caption.animate({opacity: [0, 1]}, {duration: 200, delay: FLIGHT_MS * 0.5, easing: 'ease', fill: 'backwards'});
    }
}

function prefersReducedMotion(): boolean {
    return matchMedia('(prefers-reduced-motion: reduce)').matches;
}

function flightSourceFor(trigger: HTMLElement): HTMLElement {
    if (trigger.dataset.lightbox === 'terminal') {
        return trigger.closest<HTMLElement>('.code-toolbar') ?? trigger;
    }

    return trigger;
}

function flightTarget(): HTMLElement {
    if (!refs) {
        throw new Error('Lightbox is not initialized.');
    }

    if (!refs.image.hidden) {
        return refs.image;
    }

    if (!refs.videoSlot.hidden) {
        return refs.videoSlot;
    }

    return refs.terminalSlot.firstElementChild as HTMLElement;
}

/**
 * The on-screen rect of what the user actually sees, which for an <c>object-fit: contain</c> image is the letterboxed
 * picture rather than its element box.
 */
function visibleRect(element: HTMLElement): DOMRect {
    const box: DOMRect = element.getBoundingClientRect();
    if (!(element instanceof HTMLImageElement) || !element.naturalWidth || !element.naturalHeight) {
        return box;
    }

    const scale: number = Math.min(box.width / element.naturalWidth, box.height / element.naturalHeight);
    const width: number = element.naturalWidth * scale;
    const height: number = element.naturalHeight * scale;
    return new DOMRect(box.left + (box.width - width) / 2, box.top + (box.height - height) / 2, width, height);
}

/**
 * Animates <paramref name="target"/> so its visible rect travels between <paramref name="a"/> and <paramref name="b"/>.
 * The scale is uniform (width-driven) so text and pictures never stretch.
 */
function fling(target: HTMLElement, a: DOMRect, b: DOMRect, direction: 'in' | 'out', source: HTMLElement): Animation {
    if (target.classList.contains('code-toolbar')) {
        return flingWindow(target, a, b, direction, source);
    }

    const box: DOMRect = target.getBoundingClientRect();
    const [start, end] = direction === 'in' ? [a, b] : [b, a];
    const pose = (rect: DOMRect, anchor: DOMRect): string => {
        const scale: number = rect.width / anchor.width;
        const x: number = rect.left - box.left - (anchor.left - box.left) * scale;
        const y: number = rect.top - box.top - (anchor.top - box.top) * scale;
        return `translate(${x}px, ${y}px) scale(${scale})`;
    };

    // `b` is always the modal rect, so every pose is expressed relative to it
    const frames: Keyframe[] = [
        {transformOrigin: '0 0', transform: pose(start, b)},
        {transformOrigin: '0 0', transform: pose(end, b)},
    ];
    return target.animate(frames, {
        duration: FLIGHT_MS,
        easing: FLIGHT_EASING,
        fill: 'both',
    });
}

function usesVoiceFont(trigger: HTMLElement): boolean {
    const voice: string = getComputedStyle(document.documentElement).getPropertyValue('--font-voice').split(',')[0].trim();
    const context: HTMLElement = trigger.closest<HTMLElement>('.prose') ?? trigger;
    return voice !== '' && getComputedStyle(context).fontFamily.includes(voice.replace(/["']/g, ''));
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
    refs.terminalSlot.hidden = true;
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
    refs.terminalSlot.hidden = true;
}

function openTerminal(trigger: HTMLElement): void {
    if (!refs) {
        return;
    }

    const terminal = trigger.closest<HTMLElement>('.code-toolbar');
    if (!terminal) {
        return;
    }

    const copy = terminal.cloneNode(true) as HTMLElement;
    resetTerminal(copy);
    // a power cycle makes no sense in a modal, so the red orb becomes a second way out
    const power = copy.querySelector<HTMLElement>('[data-terminal-action="power"]');
    power?.removeAttribute('data-terminal-action');
    power?.setAttribute('data-lightbox-close', '');
    power?.setAttribute('aria-label', 'Close');
    power?.setAttribute('title', 'Close');
    refs.terminalSlot.replaceChildren(copy);
    refs.terminalSlot.hidden = false;
    refs.image.hidden = true;
    refs.videoSlot.hidden = true;
}

function close(): void {
    if (!refs || closing) {
        return;
    }

    closing = true;

    if (refs.dialog.classList.contains('lightbox--fly') && flightSource) {
        const dialog: HTMLDialogElement = refs.dialog;
        const target: HTMLElement = flightTarget();
        const home: DOMRect = visibleRect(flightSource);
        dialog.classList.add('is-closing');
        refs.caption.animate({opacity: [1, 0]}, {duration: 120, easing: 'ease', fill: 'forwards'});
        fling(target, home, visibleRect(target), 'out', flightSource).finished.then(() => {
            // the close event is async, so restoring the source there leaves a blank frame between the two
            restoreSource();
            dialog.close();
            dialog.classList.remove('is-closing');
        });
        return;
    }

    refs.dialog.classList.add('is-closing');
    refs.dialog.addEventListener(
        'transitionend',
        () => {
            refs?.dialog.classList.remove('is-closing');
            refs?.dialog.close();
        },
        {once: true}
    );
}

/**
 * Resizes a terminal like a real window, rather than scaling it: the frame grows or shrinks and the text only changes
 * size to match the other end, so it never renders at a size it won't actually have.
 */
function flingWindow(target: HTMLElement, a: DOMRect, b: DOMRect, direction: 'in' | 'out', source: HTMLElement): Animation {
    const [start, end] = direction === 'in' ? [a, b] : [b, a];
    const pre = target.querySelector<HTMLElement>(':scope > pre');
    const sourcePre = source.querySelector<HTMLElement>(':scope > pre');
    const fontSizes: string[] = [sourcePre, pre].map(el => (el ? getComputedStyle(el).fontSize : ''));
    const [startFont, endFont] = direction === 'in' ? fontSizes : [...fontSizes].reverse();

    const frame = (rect: DOMRect): Keyframe => ({
        transform: `translate(${rect.left - b.left}px, ${rect.top - b.top}px)`,
        width: `${rect.width}px`,
        height: `${rect.height}px`,
    });

    // the frame is briefly smaller than its text, which would otherwise summon the <pre>'s scrollbars mid-flight
    const previousOverflow: string = target.style.overflow;
    const previousPreOverflow: string = pre?.style.overflow ?? '';
    target.style.overflow = 'hidden';
    if (pre) {
        pre.style.overflow = 'hidden';
    }

    if (pre && startFont && endFont) {
        pre.animate([{fontSize: startFont}, {fontSize: endFont}], {duration: FLIGHT_MS, easing: FLIGHT_EASING, fill: 'both'});
    }

    const animation: Animation = target.animate([frame(start), frame(end)], {
        duration: FLIGHT_MS,
        easing: FLIGHT_EASING,
        fill: 'both',
    });
    if (direction === 'in') {
        animation.finished.then(() => {
            target.style.overflow = previousOverflow;
            if (pre) {
                pre.style.overflow = previousPreOverflow;
            }
        }, () => undefined);
    }

    return animation;
}

function restoreSource(): void {
    if (flightSource) {
        flightSource.style.visibility = '';
        flightSource = null;
    }
}

function onDialogClose(): void {
    if (!refs) {
        return;
    }

    refs.image.src = '';
    refs.terminalSlot.replaceChildren();

    if (movedVideo) {
        movedVideo.element.pause();
        movedVideo.parent.insertBefore(movedVideo.element, movedVideo.nextSibling);
        movedVideo = null;
    }

    restoreSource();
    refs.dialog.getAnimations({subtree: true}).forEach(animation => animation.cancel());
    lastFocusedTrigger?.focus();
    lastFocusedTrigger = null;
}
