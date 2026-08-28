import type {Renderer, RendererBackends} from 'vexflow';
import {ensureVexFlowLoaded} from './bootstrap.ts';

/**
 * Default dimensions for a vexflow notation renderer - wide and short, suited to a staff system rather than a
 * square scene. Not enforced: a block's own code can call `renderer.resize(w, h)` to override it for content that
 * needs more (or less) room, since how much space a piece of notation needs is inherently content-driven.
 */
const DEFAULT_WIDTH = 700;
const DEFAULT_HEIGHT = 300;

/**
 * Finds vexflow codeblocks within the given element and mounts each as a live, tabbed notation renderer. No-ops -
 * without loading VexFlow at all - if the element contains none.
 * @param element The element within which to find and mount vexflow codeblocks.
 */
export async function initVexFlowScenes(element: HTMLElement): Promise<void> {
    const blocks: NodeListOf<HTMLElement> = element.querySelectorAll<HTMLElement>('code[data-vexflow]');
    const unmounted: HTMLElement[] = Array.from(blocks).filter(block => !('vexflowMounted' in block.dataset));
    if (unmounted.length === 0) {
        return;
    }

    await ensureVexFlowLoaded();

    // mounted sequentially, not in parallel - each renderer's own instance is handed off via a transient global that
    // must survive only until that block's script reads it on its very first line (see runNotationScript), so two
    // blocks mounting concurrently would race over that handoff
    for (const block of unmounted) {
        block.dataset.vexflowMounted = '';
        await mountNotation(block);
    }
}

/**
 * Replaces a single vexflow codeblock with a tabbed Notation/Source view, and runs its code against a freshly
 * constructed renderer.
 * @param codeElement The `<code data-vexflow>` element to mount.
 */
async function mountNotation(codeElement: HTMLElement): Promise<void> {
    const pre = codeElement.closest('pre');
    if (!pre?.parentElement) {
        console.error('Unexpected vexflow codeblock with no <pre> parent:', codeElement);
        return;
    }

    const codeToolbar = pre.parentElement.classList.contains('code-toolbar') ? pre.parentElement : null;
    const toolbar = codeToolbar?.querySelector<HTMLElement>(':scope > .toolbar') ?? null;

    const {notationTab, sourceTab, notationPanel, sourcePanel, tabList, wrapper, themeToggle} = buildTabs();
    (codeToolbar ?? pre).replaceWith(wrapper);
    sourcePanel.append(codeToolbar ?? pre);

    if (toolbar) {
        toolbar.classList.add('scene-toolbar');
        toolbar.hidden = true; // Notation tab is active by default
        tabList.append(toolbar);
    }

    // the toggle repaints the Notation tab's own ink/background, so it's meaningless while looking at Source
    notationTab.addEventListener('click', () => {
        activateTab(notationTab, sourceTab, notationPanel, sourcePanel);
        themeToggle.hidden = false;
        if (toolbar) {
            toolbar.hidden = true;
        }
    });
    sourceTab.addEventListener('click', () => {
        activateTab(sourceTab, notationTab, sourcePanel, notationPanel);
        themeToggle.hidden = true;
        if (toolbar) {
            toolbar.hidden = false;
        }
    });

    const globals = window as unknown as {
        Renderer: (new (element: HTMLElement, backend: RendererBackends) => Renderer) & {Backends: typeof RendererBackends};
    };

    const rendererInstance = new globals.Renderer(notationPanel, globals.Renderer.Backends.SVG);
    rendererInstance.resize(DEFAULT_WIDTH, DEFAULT_HEIGHT);

    await runNotationScript(codeElement.textContent ?? '', notationPanel, rendererInstance);
    fitToContent(notationPanel);
}

/**
 * Crops the rendered SVG's viewBox down to the actual drawn content's bounding box, with a little padding.
 * VexFlow always lays out into the full `DEFAULT_WIDTH`x`DEFAULT_HEIGHT` canvas regardless of how much of it a given
 * piece of notation actually uses, which otherwise pins the notation to the top-left corner with dead space to its
 * right and below; centering the (still full-sized) SVG within its panel wouldn't fix that; the SVG itself has to
 * shrink to match its content first.
 * @param container The element the renderer drew into.
 */
function fitToContent(container: HTMLElement): void {
    const svg = container.querySelector('svg');
    const bbox = svg?.getBBox();
    if (!svg || !bbox || bbox.width === 0 || bbox.height === 0) {
        return;
    }

    const padding = 10;
    const width = bbox.width + padding * 2;
    const height = bbox.height + padding * 2;

    svg.setAttribute('viewBox', `${bbox.x - padding} ${bbox.y - padding} ${width} ${height}`);
    svg.setAttribute('width', String(width));
    svg.setAttribute('height', String(height));
}

/**
 * Builds the Notation/Source tab chrome for a single block, unpopulated.
 */
function buildTabs(): {
    wrapper: HTMLElement;
    tabList: HTMLElement;
    notationTab: HTMLButtonElement;
    sourceTab: HTMLButtonElement;
    notationPanel: HTMLElement;
    sourcePanel: HTMLElement;
    themeToggle: HTMLButtonElement;
} {
    const wrapper = document.createElement('div');
    wrapper.className = 'vexflow-scene';

    const tabList = document.createElement('div');
    tabList.className = 'vexflow-scene-tabs';
    tabList.setAttribute('role', 'tablist');

    const notationTab = createTabButton('Notation', true);
    const sourceTab = createTabButton('Source', false);
    const themeToggle = createThemeToggle(wrapper);
    tabList.append(notationTab, sourceTab, themeToggle);

    const notationPanel = document.createElement('div');
    notationPanel.className = 'vexflow-scene-panel';

    const sourcePanel = document.createElement('div');
    sourcePanel.className = 'vexflow-source-panel';
    sourcePanel.hidden = true;

    wrapper.append(tabList, notationPanel, sourcePanel);

    return {wrapper, tabList, notationTab, sourceTab, notationPanel, sourcePanel, themeToggle};
}

/**
 * Builds the light/dark toggle for a block's rendered ink and background. Independent of the site's own
 * (permanently dark) theme: plain black notation ink is unreadable against a dark panel, so blocks default to a
 * light "paper" look, with this button available to flip individual blocks to dark ink-on-black where that reads
 * better in context. Flipping is a pure `data-theme` attribute swap on the wrapper - `_vexflow.css` does the actual
 * repainting, relying on VexFlow's SVG output rarely setting `fill`/`stroke` below the root `<svg>` element unless a
 * shape is deliberately hollow (e.g. a half-note head), so overriding just the root inherits correctly everywhere
 * else without touching those shapes.
 * @param wrapper The block's outermost element, whose `data-theme` attribute is toggled.
 */
function createThemeToggle(wrapper: HTMLElement): HTMLButtonElement {
    const button = document.createElement('button');
    button.type = 'button';
    button.className = 'vexflow-theme-toggle';

    function setTheme(theme: 'light' | 'dark'): void {
        wrapper.dataset.theme = theme;
        const switchTo = theme === 'light' ? 'dark' : 'light';
        button.innerHTML = `<i class="ti ti-${theme === 'light' ? 'moon' : 'sun'}"></i>`;
        button.setAttribute('aria-label', `Switch notation to ${switchTo} mode`);
    }

    button.addEventListener('click', () => setTheme(wrapper.dataset.theme === 'dark' ? 'light' : 'dark'));
    setTheme('light');

    return button;
}

function createTabButton(label: string, active: boolean): HTMLButtonElement {
    const button = document.createElement('button');
    button.type = 'button';
    button.className = active ? 'vexflow-tab active' : 'vexflow-tab';
    button.textContent = label;
    button.setAttribute('role', 'tab');
    button.setAttribute('aria-selected', String(active));
    return button;
}

function activateTab(tab: HTMLButtonElement, otherTab: HTMLButtonElement, panel: HTMLElement, otherPanel: HTMLElement): void {
    tab.classList.add('active');
    tab.setAttribute('aria-selected', 'true');
    otherTab.classList.remove('active');
    otherTab.setAttribute('aria-selected', 'false');
    panel.hidden = false;
    otherPanel.hidden = true;
}

/**
 * Runs a notation block's raw code as a real `<script type="module">`, verbatim (not eval/new Function) - resolves
 * once the module (including any top-level await) has fully finished executing.
 * @param code The raw JS to run.
 * @param container The element under which to mount the script.
 * @param renderer The renderer this code runs against, bound as a module-local `const renderer` (see below).
 */
function runNotationScript(code: string, container: HTMLElement, renderer: Renderer): Promise<void> {
    return new Promise((resolve, reject) => {
        const globals = window as unknown as {
            __vexflowRenderDone?: () => void;
            __vexflowPendingRenderer?: Renderer;
        };

        globals.__vexflowRenderDone = () => {
            delete globals.__vexflowRenderDone;
            URL.revokeObjectURL(url);
            resolve();
        };

        globals.__vexflowPendingRenderer = renderer;
        const preamble = 'const renderer = window.__vexflowPendingRenderer; delete window.__vexflowPendingRenderer;\n';
        const url = URL.createObjectURL(new Blob([`${preamble}${code}\nwindow.__vexflowRenderDone();`], {type: 'text/javascript'}));

        const script = document.createElement('script');
        script.type = 'module';
        script.src = url;
        script.addEventListener('error', () => {
            URL.revokeObjectURL(url);
            reject(new Error('vexflow notation script failed to run'));
        });
        container.append(script);
    });
}
