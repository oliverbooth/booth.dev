import TimeUtility from "./utils.ts";
import {applyCodeBlockHighlights} from "./codeblock-highlight/highlighting.ts";

declare const Prism: any;

class UI {
    /**
     * Forces all UI elements under the given element to update their rendering.
     * @param element The element to search for UI elements in.
     */
    public static updateUI(element?: Element) {
        element = element || document.body;
        UI.renderSpoilers(element);
        UI.renderTabs(element);
    }

    /**
     * Renders all spoilers in the document.
     * @param element The element to search for spoilers in.
     */
    public static renderSpoilers(element?: Element) {
        element = element || document.body;
        const spoilers = element.querySelectorAll(".spoiler");
        spoilers.forEach((spoiler) => {
            spoiler.addEventListener("click", () => {
                spoiler.classList.add("spoiler-revealed");
            });
        });
    }

    /**
     * Renders tabs in the document.
     * @param element The element to search for tabs in.
     */
    public static renderTabs(element?: Element) {
        element = element || document.body;
        element.querySelectorAll("[role=\"tablist\"]").forEach(function (tabList: HTMLElement) {
            const identifier = tabList.dataset.identifier;
            const tabLinks = tabList.querySelectorAll(".nav-link");
            const tabPanes = element.querySelectorAll(`.tab-pane[data-identifier="${identifier}"]`);

            tabLinks.forEach(function (tabLink: Element) {
                tabLink.addEventListener("click", (ev: Event) => {
                    ev.preventDefault();

                    const controls = document.getElementById(tabLink.getAttribute("aria-controls"));

                    // switch "active" tab link
                    tabLinks.forEach(e => e.classList.remove("active"));
                    tabLink.classList.add("active");

                    // switch active tab itself
                    tabPanes.forEach(e => e.classList.remove("show", "active"));
                    controls.classList.add("show", "active");
                });
            });
        });
    }
}

export default UI;