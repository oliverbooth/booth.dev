import type {Scene, SceneOptions, ThreeDScene} from 'manim-web';
import {ensureManimWebLoaded} from './bootstrap.ts';
import {DraggableExpression} from './draggable-expression.ts';
import {DisplayValue, DraggableValue} from './draggable-value.ts';
import {ensureMathJsLoaded} from './mathjs-loader.ts';

/**
 * Standardized options for every manim-web scene on the site.
 */
const SCENE_OPTIONS: SceneOptions = {backgroundColor: '#000'};

/**
 * Every currently-mounted scene, keyed by its `.manim-scene-panel` container.
 */
const mountedScenes = new WeakMap<HTMLElement, Scene | ThreeDScene>();

/**
 * Disposes every manim-web scene mounted within the given element.
 * @param element The element within which to find and dispose manim-web scenes.
 */
export function disposeManimScenes(element: HTMLElement): void {
    const panels = element.matches('.manim-scene-panel')
        ? [element, ...element.querySelectorAll<HTMLElement>('.manim-scene-panel')]
        : [...element.querySelectorAll<HTMLElement>('.manim-scene-panel')];

    for (const panel of panels) {
        mountedScenes.get(panel)?.dispose();
        mountedScenes.delete(panel);
    }
}

/**
 * Finds manim-web codeblocks within the given element and mounts each as a live, tabbed scene. No-ops - without
 * loading manim-web at all - if the element contains none.
 * @param element The element within which to find and mount manim-web codeblocks.
 */
export async function initManimScenes(element: HTMLElement): Promise<void> {
    const blocks: NodeListOf<HTMLElement> = element.querySelectorAll<HTMLElement>('code[data-manim]');
    const unmounted: HTMLElement[] = Array.from(blocks).filter(block => !('manimMounted' in block.dataset));
    if (unmounted.length === 0) {
        return;
    }

    await Promise.all([ensureManimWebLoaded(), ensureMathJsLoaded()]);

    // DraggableValue/DisplayValue/DraggableExpression are ours, not part of manim-web's own export set, so they
    // aren't bridged by ensureManimWebLoaded - set up once per page rather than once per scene
    const globals = window as unknown as {
        DraggableValue: typeof DraggableValue;
        DisplayValue: typeof DisplayValue;
        DraggableExpression: typeof DraggableExpression;
    };
    globals.DraggableValue = DraggableValue;
    globals.DisplayValue = DisplayValue;
    globals.DraggableExpression = DraggableExpression;

    // mounted sequentially, not in parallel - each scene's own instance is handed off via a transient global that
    // must survive only until that scene's module reads it on its very first line (see runSceneScript), so two
    // scenes mounting concurrently would race over that handoff
    for (const block of unmounted) {
        block.dataset.manimMounted = '';
        await mountScene(block);
    }
}

/**
 * Replaces a single manim-web codeblock with a tabbed Scene/Source view, and runs its code against a freshly
 * constructed scene.
 * @param codeElement The `<code data-manim>` element to mount.
 */
async function mountScene(codeElement: HTMLElement): Promise<void> {
    const pre = codeElement.closest('pre');
    if (!pre?.parentElement) {
        console.error('Unexpected manim codeblock with no <pre> parent:', codeElement);
        return;
    }

    const codeToolbar = pre.parentElement.classList.contains('code-toolbar') ? pre.parentElement : null;
    const toolbar = codeToolbar?.querySelector<HTMLElement>(':scope > .toolbar') ?? null;

    const {sceneTab, sourceTab, scenePanel, sourcePanel, tabList, wrapper, fullscreenToggle} = buildTabs();
    (codeToolbar ?? pre).replaceWith(wrapper);
    sourcePanel.append(codeToolbar ?? pre);

    if (toolbar) {
        toolbar.classList.add('scene-toolbar');
        toolbar.hidden = true; // scene tab is active by default
        tabList.append(toolbar);
    }

    sceneTab.addEventListener('click', () => {
        activateTab(sceneTab, sourceTab, scenePanel, sourcePanel);
        fullscreenToggle.hidden = false;
        if (toolbar) {
            toolbar.hidden = true;
        }
    });
    sourceTab.addEventListener('click', () => {
        activateTab(sourceTab, sceneTab, sourcePanel, scenePanel);
        fullscreenToggle.hidden = true;
        if (toolbar) {
            toolbar.hidden = false;
        }
    });

    const globals = window as unknown as {
        Scene: new (container: HTMLElement, options: SceneOptions) => Scene;
        ThreeDScene: new (container: HTMLElement, options: SceneOptions) => ThreeDScene;
    };

    const scene = codeElement.dataset.manim === '3d'
        ? new globals.ThreeDScene(scenePanel, SCENE_OPTIONS)
        : new globals.Scene(scenePanel, SCENE_OPTIONS);
    mountedScenes.set(scenePanel, scene);

    const resizeObserver = new ResizeObserver(() => {
        if (!scenePanel.isConnected) {
            resizeObserver.disconnect();
            return;
        }

        scene.resize(scenePanel.clientWidth, scenePanel.clientHeight);
    });
    resizeObserver.observe(scenePanel);

    await runSceneScript(codeElement.textContent ?? '', scenePanel, scene);
}

/**
 * Builds the Scene/Source tab chrome for a single scene, unpopulated.
 */
function buildTabs(): {
    wrapper: HTMLElement;
    tabList: HTMLElement;
    sceneTab: HTMLButtonElement;
    sourceTab: HTMLButtonElement;
    scenePanel: HTMLElement;
    sourcePanel: HTMLElement;
    fullscreenToggle: HTMLButtonElement;
} {
    const wrapper = document.createElement('div');
    wrapper.className = 'manim-scene';

    const tabList = document.createElement('div');
    tabList.className = 'manim-scene-tabs';
    tabList.setAttribute('role', 'tablist');

    const sceneTab = createTabButton('Scene', true);
    const sourceTab = createTabButton('Source', false);
    const fullscreenToggle = createFullscreenToggle(wrapper);
    tabList.append(sceneTab, sourceTab, fullscreenToggle);

    const scenePanel = document.createElement('div');
    scenePanel.className = 'manim-scene-panel';

    const sourcePanel = document.createElement('div');
    sourcePanel.className = 'manim-source-panel';
    sourcePanel.hidden = true;

    wrapper.append(tabList, scenePanel, sourcePanel);

    return {wrapper, tabList, sceneTab, sourceTab, scenePanel, sourcePanel, fullscreenToggle};
}

/**
 * Builds the fullscreen toggle for a scene - fullscreens the whole wrapper (tabs included), not just the canvas
 * panel, so the toggle itself (and the Scene/Source tabs) stay reachable to exit again rather than disappearing the
 * moment you enter fullscreen. Tracks the real `fullscreenchange` event rather than just flipping state on click,
 * since fullscreen can also be exited via Esc or the browser's own UI, not only this button.
 * @param wrapper The scene's outermost wrapper element - what actually goes fullscreen.
 */
function createFullscreenToggle(wrapper: HTMLElement): HTMLButtonElement {
    const button = document.createElement('button');
    button.type = 'button';
    button.className = 'manim-fullscreen-toggle';

    function update(): void {
        const isFullscreen = document.fullscreenElement === wrapper;
        button.innerHTML = `<i class="ti ti-${isFullscreen ? 'minimize' : 'maximize'}"></i>`;
        button.setAttribute('aria-label', isFullscreen ? 'Exit fullscreen' : 'View scene fullscreen');
    }

    button.addEventListener('click', () => {
        void (document.fullscreenElement === wrapper ? document.exitFullscreen() : wrapper.requestFullscreen());
    });

    wrapper.addEventListener('fullscreenchange', update);
    update();

    return button;
}

function createTabButton(label: string, active: boolean): HTMLButtonElement {
    const button = document.createElement('button');
    button.type = 'button';
    button.className = active ? 'manim-tab active' : 'manim-tab';
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
 * Runs a scene's raw code as a real `<script type="module">`, verbatim (not eval/new Function) - resolves once the
 * module (including any top-level await) has fully finished executing.
 * @param code The raw JS to run.
 * @param container The element under which to mount the script.
 * @param scene The scene this code runs against, bound as a module-local `const scene` (see below).
 */
function runSceneScript(code: string, container: HTMLElement, scene: Scene | ThreeDScene): Promise<void> {
    return new Promise((resolve, reject) => {
        const globals = window as unknown as {
            __manimSceneDone?: () => void;
            __manimPendingScene?: Scene | ThreeDScene;
        };

        globals.__manimSceneDone = () => {
            delete globals.__manimSceneDone;
            URL.revokeObjectURL(url);
            resolve();
        };

        globals.__manimPendingScene = scene;
        const preamble = 'const scene = window.__manimPendingScene; delete window.__manimPendingScene;\n';
        const url = URL.createObjectURL(new Blob([`${preamble}${code}\nwindow.__manimSceneDone();`], {type: 'text/javascript'}));

        const script = document.createElement('script');
        script.type = 'module';
        script.src = url;
        script.addEventListener('error', () => {
            URL.revokeObjectURL(url);
            reject(new Error('manim scene script failed to run'));
        });
        container.append(script);
    });
}
