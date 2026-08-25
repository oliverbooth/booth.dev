import type {Scene, SceneOptions, ThreeDScene} from 'manim-web';
import {ensureManimWebLoaded} from './bootstrap.ts';

/**
 * Standardized dimensions for every manim-web scene on the site.
 */
const SCENE_OPTIONS: SceneOptions = {width: 700, height: 400};

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

    await ensureManimWebLoaded();

    // mounted sequentially, not in parallel - each scene's code references the ambient `scene` global, which this
    // reassigns per block, so two scenes constructing concurrently would race over which one it actually points to
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

    const {sceneTab, sourceTab, scenePanel, sourcePanel, wrapper} = buildTabs();
    pre.replaceWith(wrapper);
    sourcePanel.append(pre);

    sceneTab.addEventListener('click', () => activateTab(sceneTab, sourceTab, scenePanel, sourcePanel));
    sourceTab.addEventListener('click', () => activateTab(sourceTab, sceneTab, sourcePanel, scenePanel));

    const globals = window as unknown as {
        Scene: new (container: HTMLElement, options: SceneOptions) => Scene;
        ThreeDScene: new (container: HTMLElement, options: SceneOptions) => ThreeDScene;
        scene: Scene | ThreeDScene;
    };

    globals.scene = codeElement.dataset.manim === '3d'
        ? new globals.ThreeDScene(scenePanel, SCENE_OPTIONS)
        : new globals.Scene(scenePanel, SCENE_OPTIONS);

    await runSceneScript(codeElement.textContent ?? '', scenePanel);
}

/**
 * Builds the Scene/Source tab chrome for a single scene, unpopulated.
 */
function buildTabs(): {
    wrapper: HTMLElement;
    sceneTab: HTMLButtonElement;
    sourceTab: HTMLButtonElement;
    scenePanel: HTMLElement;
    sourcePanel: HTMLElement;
} {
    const wrapper = document.createElement('div');
    wrapper.className = 'manim-scene';

    const tabList = document.createElement('div');
    tabList.className = 'manim-scene-tabs';
    tabList.setAttribute('role', 'tablist');

    const sceneTab = createTabButton('Scene', true);
    const sourceTab = createTabButton('Source', false);
    tabList.append(sceneTab, sourceTab);

    const scenePanel = document.createElement('div');
    scenePanel.className = 'manim-scene-panel';

    const sourcePanel = document.createElement('div');
    sourcePanel.className = 'manim-source-panel';
    sourcePanel.hidden = true;

    wrapper.append(tabList, scenePanel, sourcePanel);

    return {wrapper, sceneTab, sourceTab, scenePanel, sourcePanel};
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
 */
function runSceneScript(code: string, container: HTMLElement): Promise<void> {
    return new Promise((resolve, reject) => {
        // `load` turned out not to be a reliable "fully done" signal for a module with a top-level await - it fired
        // before the paused await actually resumed, letting the sequential-mounting loop move on (and reassign/clear
        // the ambient `scene`) while this script was still suspended mid-run. A sentinel call appended after the raw
        // code only runs once every preceding top-level statement - awaits included - has actually finished, since
        // top-level statements execute strictly in order.
        const globals = window as unknown as {__manimSceneDone?: () => void};
        globals.__manimSceneDone = () => {
            delete globals.__manimSceneDone;
            URL.revokeObjectURL(url);
            resolve();
        };

        const url = URL.createObjectURL(new Blob([`${code}\nwindow.__manimSceneDone();`], {type: 'text/javascript'}));

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
