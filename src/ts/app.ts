import UI from "./UI";
import {initEasterEggs} from './easter-eggs.ts';
import {initFavicon} from './favicon.ts';
import {initLightbox} from './lightbox.ts';
import {initAltTextPopovers} from './images.ts';
import {initContentFeatures} from "./content-rendering.ts";
import {initFiltering} from "./filtering.ts";

(() => {
    initFavicon();
    initAltTextPopovers();
    initContentFeatures();
    initEasterEggs();
    initFiltering();
    initLightbox();

    UI.updateUI();
})();
