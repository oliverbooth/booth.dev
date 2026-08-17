import UI from "./UI";
import Input from "./Input";
import Callout from "./Callout";
import {initLightbox} from './lightbox.ts';
import {initFavicon} from './favicon.ts';

declare const Prism: any;
declare const lucide: any;
declare const JXG: any;

(() => {
    Callout.foldAll();
    lucide.createIcons();
    JXG.Options.text.useMathJax = true;

    Prism.languages.extend('markup', {});
    Prism.languages.hex = {
        'number': {
            pattern: /(?:[a-fA-F0-9]{3}){1,2}\b/i,
            lookbehind: true
        }
    };
    Prism.languages.binary = {
        'number': {
            pattern: /[10]+/i,
            lookbehind: true
        }
    };
    Prism.languages.insertBefore('custom', 'tag', {
        'mark': {
            pattern: /<\/?mark(?:\s+\w+(?:=(?:"[^"]*"|'[^']*'|[^\s'">=]+))?\s*|\s*)\/?>/,
            greedy: true
        }
    });

    Input.registerShortcut(Input.KONAMI_CODE, () => {
        window.open("https://www.youtube.com/watch?v=dQw4w9WgXcQ", "_blank");
    });

    initFavicon();
    UI.updateUI();
    initLightbox();

    document.addEventListener("click", (event: MouseEvent) => {
        const target = event.target as Element | null;
        const badge = target?.closest<HTMLButtonElement>(".alt-badge") ?? null;
        const clickedPopover = target?.closest<HTMLElement>(".alt-popover") ?? null;

        if (clickedPopover !== null) {
            clickedPopover.classList.remove("is-open");
            clickedPopover.remove();
            return;
        }

        const openPopover = document.querySelector<HTMLElement>(".alt-popover.is-open");

        if (openPopover !== null && openPopover.dataset.owner !== badge?.dataset.altText) {
            openPopover.classList.remove("is-open");
            openPopover.remove();
        }

        if (badge === null) {
            return;
        }

        event.stopPropagation();

        const wrap = badge.closest<HTMLElement>(".figure-img-wrap");
        if (wrap === null) {
            return;
        }

        let popover = wrap.querySelector<HTMLElement>(".alt-popover");

        if (popover !== null) {
            popover.classList.remove("is-open");
            popover.remove();
            return;
        }

        popover = document.createElement("div");
        popover.className = "alt-popover";
        popover.setAttribute("role", "status");
        popover.innerHTML = badge.dataset.altText ?? "";
        wrap.appendChild(popover);

        requestAnimationFrame(() => popover!.classList.add("is-open"));
    });

    document.addEventListener("keydown", (event: KeyboardEvent) => {
        if (event.key === "Escape") {
            document.querySelectorAll<HTMLElement>(".alt-popover.is-open").forEach((el) => el.remove());
        }
    });
})();
