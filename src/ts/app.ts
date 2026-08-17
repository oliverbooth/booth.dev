import UI from "./UI";
import {initEasterEggs} from './easter-eggs.ts';
import {initFavicon} from './favicon.ts';
import {initLightbox} from './lightbox.ts';
import {initAltTextPopovers} from './images.ts';

declare const Prism: any;
declare const lucide: any;
declare const JXG: any;

(() => {
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

    initFavicon();
    initAltTextPopovers();
    initEasterEggs();
    initLightbox();

    UI.updateUI();
})();
