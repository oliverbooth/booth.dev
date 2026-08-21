import {initFavicon} from './favicon.ts';
import {initCopyButtons} from './clipboard.ts';
import {initContentFeatures} from './content-rendering.ts';
import {initPostAuthoring} from './admin/post-authoring.ts';
import {initAltTextPopovers} from './images.ts';
import {initSearch} from './search.ts';

(() => {
    initAltTextPopovers();
    initFavicon();
    initCopyButtons();
    initContentFeatures();
    initPostAuthoring();
    initSearch();
})();
