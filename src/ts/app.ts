import UI from "./UI";
import {initCopyButtons} from './clipboard.ts';
import {initContentFeatures} from './content-rendering.ts';
import {initEasterEggs} from './easter-eggs.ts';
import {initFavicon} from './favicon.ts';
import {initFiltering} from './filtering.ts';
import {initAltTextPopovers} from './images.ts';
import {initLightbox} from './lightbox.ts';
import {initTerminalTypewriters} from './terminal.ts';

(() => {
    initFavicon();
    initAltTextPopovers();
    initCopyButtons();
    initContentFeatures();
    initEasterEggs();
    initFiltering();
    initLightbox();
    initTerminalTypewriters();

    UI.updateUI();
})();
