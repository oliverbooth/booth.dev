import mermaid from 'mermaid';

mermaid.initialize({
    startOnLoad: false,
    theme: 'base',
    themeVariables: {
        darkMode: true,
        background: cssColor('--surface-1'),
        primaryColor: cssColor('--surface-2'),
        primaryTextColor: cssColor('--code-text'),
        primaryBorderColor: cssColor('--accent'),
        secondaryColor: cssColor('--surface-2'),
        tertiaryColor: cssColor('--surface-2'),
        lineColor: cssColor('--syntax-punct'),
        textColor: cssColor('--code-text'),
        edgeLabelBackground: cssColor('--surface-1'),
        fontFamily: cssVar('--font-sans'),
    },
});

/**
 * Reads the resolved value of a CSS custom property off the document root.
 * @param name The custom property's name, e.g. `--accent`.
 */
function cssVar(name: string): string {
    return getComputedStyle(document.documentElement).getPropertyValue(name).trim();
}

/**
 * Reads a CSS custom property holding a color and converts it to sRGB hex. Mermaid's color parser doesn't understand
 * modern color spaces like `oklch()`, which is what the theme tokens resolve to, so the browser does the conversion.
 * @param name The custom property's name, e.g. `--code-text`.
 */
function cssColor(name: string): string {
    const context = document.createElement('canvas').getContext('2d');
    if (!context) {
        return cssVar(name);
    }

    context.fillStyle = cssVar(name);
    context.fillRect(0, 0, 1, 1);
    const [red, green, blue, alpha] = context.getImageData(0, 0, 1, 1).data;
    const channels = alpha === 255 ? [red, green, blue] : [red, green, blue, alpha];
    return '#' + channels.map(channel => channel.toString(16).padStart(2, '0')).join('');
}

/**
 * Finds mermaid codeblocks within the given element and mounts each as a live diagram.
 * @param element The element within which to find and mount mermaid codeblocks.
 */
export async function initMermaidScenes(element: HTMLElement): Promise<void> {
    const blocks: NodeListOf<HTMLElement> = element.querySelectorAll<HTMLElement>('code[data-mermaid]');
    const unmounted: HTMLElement[] = Array.from(blocks).filter(block => !('mermaidMounted' in block.dataset));
    if (unmounted.length === 0) {
        return;
    }

    for (const block of unmounted) {
        block.dataset.mermaidMounted = '';
        await mountDiagram(block);
    }
}

/**
 * Replaces a single mermaid codeblock with its rendered diagram, tabbed alongside a Source view unless the block is marked
 * `no-source`.
 * @param codeElement The `<code data-mermaid>` element to mount.
 */
async function mountDiagram(codeElement: HTMLElement): Promise<void> {
    const pre = codeElement.closest('pre');
    if (!pre?.parentElement) {
        console.error('Unexpected mermaid codeblock with no <pre> parent:', codeElement);
        return;
    }

    const source = codeElement.textContent ?? '';
    const noSource = 'noSource' in codeElement.dataset;
    const codeToolbar = pre.parentElement.classList.contains('code-toolbar') ? pre.parentElement : null;

    const wrapper = document.createElement('div');
    wrapper.className = 'mermaid-scene';

    const diagramPanel = document.createElement('div');
    diagramPanel.className = 'mermaid-scene-panel';
    diagramPanel.textContent = source;

    (codeToolbar ?? pre).replaceWith(wrapper);

    if (noSource) {
        // `no-source`: just the diagram, no tab chrome and no card framing either
        wrapper.classList.add('mermaid-scene--bare');
        wrapper.append(diagramPanel);
    } else {
        const {diagramTab, sourceTab, sourcePanel, tabList} = buildTabs();
        wrapper.append(tabList, diagramPanel, sourcePanel);
        // the source panel just wraps the code block as-is (header bar, copy button and all - already styled by
        // _prism-toolbar.css); `hidden` on the panel hides all of that along with it, so there's nothing further to
        // wire up per tab switch
        sourcePanel.append(codeToolbar ?? pre);

        diagramTab.addEventListener('click', () => activateTab(diagramTab, sourceTab, diagramPanel, sourcePanel));
        sourceTab.addEventListener('click', () => activateTab(sourceTab, diagramTab, sourcePanel, diagramPanel));
    }

    try {
        await mermaid.run({nodes: [diagramPanel]});
    } catch (error) {
        console.error('Failed to render mermaid diagram:', error);
    }
}

/**
 * Builds the Diagram/Source tab chrome for a single block, unpopulated. The wrapper and diagram panel are built by
 * the caller instead, since a `no-source` block needs those two but none of this.
 */
function buildTabs(): {
    tabList: HTMLElement;
    diagramTab: HTMLButtonElement;
    sourceTab: HTMLButtonElement;
    sourcePanel: HTMLElement;
} {
    const tabList = document.createElement('div');
    tabList.className = 'mermaid-scene-tabs';

    const tabGroup = document.createElement('div');
    tabGroup.className = 'scene-tab-group';
    tabGroup.setAttribute('role', 'tablist');

    const diagramTab = createTabButton('Diagram', true);
    const sourceTab = createTabButton('Source', false);
    tabGroup.append(diagramTab, sourceTab);
    tabList.append(tabGroup);

    const sourcePanel = document.createElement('div');
    sourcePanel.className = 'mermaid-source-panel';
    sourcePanel.hidden = true;

    return {tabList, diagramTab, sourceTab, sourcePanel};
}

function createTabButton(label: string, active: boolean): HTMLButtonElement {
    const button = document.createElement('button');
    button.type = 'button';
    button.className = active ? 'mermaid-tab active' : 'mermaid-tab';
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
