import {initCommandPalette} from './command-palette.ts';
import {initCopyButtons} from './clipboard.ts';
import {initContentFeatures} from './content-rendering.ts';
import {initEasterEggs} from './easter-eggs.ts';
import {initFavicon} from './favicon.ts';
import {initFiltering} from './filtering.ts';
import {initAltTextPopovers} from './images.ts';
import {initLightbox} from './lightbox.ts';
import {initTerminalTypewriters} from './terminal.ts';
import {initAvatarFallback} from './avatar-fallback.ts';
import {initNowClock} from './now-clock.ts';

(() => {
    initFavicon();
    initAltTextPopovers();
    initCopyButtons();
    initContentFeatures();
    initEasterEggs();
    initFiltering();
    initLightbox();
    initCommandPalette();
    initTerminalTypewriters();
    initAvatarFallback();
    initNowClock();
})();
