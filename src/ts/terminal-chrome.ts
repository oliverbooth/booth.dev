const MINIMISE_SELECTOR = '[data-terminal-action="minimise"]';
const POWER_SELECTOR = '[data-terminal-action="power"]';
const ANIMATION_MS = 220;
const POWER_CYCLE_MS = 1300;
const POWER_CYCLE_REDUCED_MS = 700;

interface HeightFrame {
    height: string;
    paddingTop: string;
    paddingBottom: string;
}

/**
 * Wires up the minimise and power orbs of every terminal window, including ones added to the page later.
 */
export function initTerminalChrome(): void {
    document.addEventListener('click', event => {
        const target = event.target as HTMLElement;
        const minimise = target.closest<HTMLButtonElement>(MINIMISE_SELECTOR);
        const power = target.closest<HTMLButtonElement>(POWER_SELECTOR);
        const terminal = (minimise ?? power)?.closest<HTMLElement>('.code-toolbar');
        if (!terminal) {
            return;
        }

        if (minimise) {
            toggleMinimised(terminal, minimise);
        } else {
            powerCycle(terminal);
        }
    });
}

/**
 * Adds the window chrome (orbs and title) to a terminal code block.
 * @param terminal The `.code-toolbar` wrapper Prism created around the block.
 * @param title The window title, which may be empty.
 */
export function attachTerminalChrome(terminal: HTMLElement, title: string): void {
    if (terminal.querySelector(':scope > .terminal-chrome')) {
        return;
    }

    const chrome = document.createElement('div');
    chrome.className = 'terminal-chrome';
    chrome.innerHTML = `
        <button type="button" class="terminal-orb terminal-orb--power" data-terminal-action="power"
                aria-label="Power cycle terminal" title="Power off"></button>
        <button type="button" class="terminal-orb terminal-orb--minimise" data-terminal-action="minimise"
                aria-label="Minimise terminal" aria-expanded="true" title="Minimise"></button>
        <button type="button" class="terminal-orb terminal-orb--expand" data-lightbox="terminal"
                aria-label="Expand terminal" title="Expand"></button>
        <span class="terminal-title"></span>`;
    chrome.querySelector<HTMLElement>('.terminal-title')!.textContent = title;

    terminal.prepend(chrome);
}

/**
 * Puts a terminal window back to its expanded state, for a copy of one that was minimised.
 * @param terminal The `.code-toolbar` wrapper to reset.
 */
export function resetTerminal(terminal: HTMLElement): void {
    terminal.classList.remove('is-minimised');
    terminal.querySelector<HTMLElement>(':scope > pre')?.removeAttribute('inert');
    terminal.querySelector(MINIMISE_SELECTOR)?.setAttribute('aria-expanded', 'true');
}

function toggleMinimised(terminal: HTMLElement, button: HTMLElement): void {
    const pre = terminal.querySelector<HTMLElement>(':scope > pre');
    if (!pre) {
        return;
    }

    const minimising = !terminal.classList.contains('is-minimised');
    const from = measure(pre);

    terminal.classList.toggle('is-minimised', minimising);
    pre.inert = minimising;
    button.setAttribute('aria-expanded', String(!minimising));

    const to = measure(pre);
    const duration = matchMedia('(prefers-reduced-motion: reduce)').matches ? 0 : ANIMATION_MS;

    pre.style.overflow = 'hidden';
    pre.animate([from, to], {duration, easing: 'ease'}).finished
        .catch(() => undefined)
        .finally(() => pre.style.removeProperty('overflow'));
}

function powerCycle(terminal: HTMLElement): void {
    if (terminal.getAnimations().length > 0) {
        return;
    }

    if (matchMedia('(prefers-reduced-motion: reduce)').matches) {
        terminal.animate([{opacity: 1}, {opacity: 0.25}, {opacity: 1}], {duration: POWER_CYCLE_REDUCED_MS});
        return;
    }

    // the window collapses to a bar this tall, then narrows to a square this wide, like a CRT losing its picture
    const beam = 10;
    const line = `scale(1, ${beam / terminal.offsetHeight})`;
    const dot = `scale(${beam / terminal.offsetWidth}, ${beam / terminal.offsetHeight})`;
    const glow = '0 0 24px 6px rgb(255 255 255 / 0.55)';

    // easing is per segment, since a whole-animation curve would shift the stages away from their offsets
    terminal.animate(
        [
            {offset: 0, transform: 'scale(1)', filter: 'brightness(1)', opacity: 1, easing: 'ease-in'},
            {offset: 0.12, transform: line, filter: 'brightness(3)', boxShadow: glow, opacity: 1, easing: 'ease-in'},
            {offset: 0.26, transform: dot, filter: 'brightness(5)', boxShadow: glow, opacity: 1},
            {offset: 0.32, transform: dot, filter: 'brightness(5)', boxShadow: glow, opacity: 0},
            {offset: 0.7, transform: dot, filter: 'brightness(5)', boxShadow: glow, opacity: 0},
            {offset: 0.76, transform: dot, filter: 'brightness(5)', boxShadow: glow, opacity: 1, easing: 'ease-out'},
            {offset: 0.86, transform: line, filter: 'brightness(3)', boxShadow: glow, opacity: 1, easing: 'ease-out'},
            {offset: 1, transform: 'scale(1)', filter: 'brightness(1)', opacity: 1}
        ],
        {duration: POWER_CYCLE_MS}
    );
}

function measure(element: HTMLElement): HeightFrame {
    const style = getComputedStyle(element);
    return {height: style.height, paddingTop: style.paddingTop, paddingBottom: style.paddingBottom};
}
