import mermaid from 'mermaid';

mermaid.initialize({
    startOnLoad: false,
    theme: 'base',
    themeVariables: {
        darkMode: true,
        background: cssVar('--surface-1'),
        primaryColor: cssVar('--surface-2'),
        primaryTextColor: cssVar('--text-primary'),
        primaryBorderColor: cssVar('--accent'),
        secondaryColor: cssVar('--surface-2'),
        tertiaryColor: cssVar('--surface-2'),
        lineColor: cssVar('--text-secondary'),
        textColor: cssVar('--text-primary'),
        edgeLabelBackground: cssVar('--surface-1'),
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
    const toolbar = codeToolbar?.querySelector<HTMLElement>(':scope > .toolbar') ?? null;

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
        sourcePanel.append(codeToolbar ?? pre);

        if (toolbar) {
            toolbar.classList.add('scene-toolbar');
            toolbar.hidden = true; // Diagram tab is active by default
            tabList.append(toolbar);
        }

        diagramTab.addEventListener('click', () => {
            activateTab(diagramTab, sourceTab, diagramPanel, sourcePanel);
            if (toolbar) {
                toolbar.hidden = true;
            }
        });
        sourceTab.addEventListener('click', () => {
            activateTab(sourceTab, diagramTab, sourcePanel, diagramPanel);
            if (toolbar) {
                toolbar.hidden = false;
            }
        });
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
    tabList.setAttribute('role', 'tablist');

    const diagramTab = createTabButton('Diagram', true);
    const sourceTab = createTabButton('Source', false);
    tabList.append(diagramTab, sourceTab);

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
