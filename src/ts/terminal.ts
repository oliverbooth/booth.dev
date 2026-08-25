/**
 * Initializes the typewriter animation for all hero terminals under the given element.
 */
export function initTerminalTypewriters(): void {
    const containers: NodeListOf<HTMLElement> = document.querySelectorAll<HTMLElement>('[data-terminal-typewriter]');
    document.fonts.ready.then(() => containers.forEach(container => runTerminalTypewriter(container)));
}

async function runTerminalTypewriter(container: HTMLElement): Promise<void> {
    const reducedMotion: boolean = window.matchMedia('(prefers-reduced-motion: reduce)').matches;
    const username: string = container.dataset.username ?? 'user@host';
    const commandLines: NodeListOf<HTMLElement> = container.querySelectorAll<HTMLElement>('[data-command]');
    const revealBlocks: NodeListOf<HTMLElement> = container.querySelectorAll<HTMLElement>('[data-reveal]');
    const promptLine: HTMLElement | null = container.querySelector<HTMLElement>('[data-prompt]');

    if (reducedMotion) {
        [...commandLines, ...revealBlocks].forEach(el => el.classList.add('is-visible'));

        if (promptLine) {
            promptLine.classList.add('is-visible');
        }

        return; // static text already correct in markup, just reveal everything at once
    }

    const sequence: HTMLElement[] = [];
    for (const node of container.childNodes) {
        if (node instanceof HTMLElement) {
            sequence.push(node);
        }
    }

    for (const element of sequence) {
        if (element.dataset.command !== undefined) {
            await typeCommand(element, username, element.dataset.command);
            await sleep(150);
        } else if (element.hasAttribute('data-prompt')) {
            renderPrompt(element, username);
            element.classList.add('is-visible');
            await sleep(300);
        } else if (element.hasAttribute('data-reveal')) {
            element.classList.add('is-visible');
            await sleep(300);
        }
    }
}

async function typeCommand(el: HTMLElement, username: string, text: string): Promise<void> {
    el.innerHTML = `<span class="prompt">${username}</span><span class="path">:~$</span> `;
    el.classList.add('is-visible');
    for (const char of text) {
        el.innerHTML += char;
        await sleep(35 + Math.random() * 25);
    }
}

function renderPrompt(el: HTMLElement, username: string): void {
    el.innerHTML = `<span class="prompt">${username}</span><span class="path">:~$</span> <span class="cursor">&nbsp;</span>`;
}

function sleep(ms: number): Promise<void> {
    return new Promise(resolve => setTimeout(resolve, ms));
}
