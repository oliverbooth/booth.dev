import UI from "./UI";
import {initEasterEggs} from './easter-eggs.ts';
import {initFavicon} from './favicon.ts';
import {initLightbox} from './lightbox.ts';
import {initAltTextPopovers} from './images.ts';

declare const Prism: any;

(() => {
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

    initFavicon();
    initAltTextPopovers();
    initEasterEggs();
    initLightbox();

    UI.updateUI();
})();
