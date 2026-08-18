import {initFavicon} from './favicon.ts';
import {initCopyButtons} from './clipboard.ts';
import {initContentFeatures} from './content-rendering.ts';

(() => {
    initFavicon();
    initCopyButtons();
    initContentFeatures();
})();
