import TimeUtility from "./TimeUtility";
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
        UI.renderTimestamps(element);
        UI.fixCanvas(element);
    }

    /**
     * Fixes canvas elements with data-engine attribute to fill their parent container.
     * @param element The element to search for canvas elements in.
     */
    public static fixCanvas(element?: Element) {
        element = element || document.body;

        // Fix any already-existing canvases
        element.querySelectorAll('canvas[data-engine]').forEach(c => {
            const el = c as HTMLCanvasElement;
            el.style.width = '100%';
            el.style.height = '100%';
        });

        // Watch for new canvases being added
        const observer = new MutationObserver(() => {
            element!.querySelectorAll('canvas[data-engine]').forEach(c => {
                const el = c as HTMLCanvasElement;
                el.style.width = '100%';
                el.style.height = '100%';
            });
        });

        observer.observe(element, { childList: true, subtree: true });
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

    /**
     * Renders Discord-style <t:timestamp:format> tags.
     * @param element The element to search for timestamps in.
     */
    public static renderTimestamps(element?: Element) {
        element = element || document.body;
        const timestamps = element.querySelectorAll("span[data-timestamp][data-format]");
        timestamps.forEach((timestamp) => {
            const seconds = parseInt(timestamp.getAttribute("data-timestamp"));
            const format = timestamp.getAttribute("data-format");
            const date = new Date(seconds * 1000);

            const shortTimeString = date.toLocaleTimeString([], {hour: "2-digit", minute: "2-digit"});
            const shortDateString = date.toLocaleDateString([], {day: "2-digit", month: "2-digit", year: "numeric"});
            const longTimeString = date.toLocaleTimeString([], {hour: "2-digit", minute: "2-digit", second: "2-digit"});
            const longDateString = date.toLocaleDateString([], {day: "numeric", month: "long", year: "numeric"});
            const weekday = date.toLocaleString([], {weekday: "long"});
            timestamp.setAttribute("title", `${weekday}, ${longDateString} ${shortTimeString}`);

            switch (format) {
                case "t":
                    timestamp.textContent = shortTimeString;
                    break;

                case "T":
                    timestamp.textContent = longTimeString;
                    break;

                case "d":
                    timestamp.textContent = shortDateString;
                    break;

                case "D":
                    timestamp.textContent = longDateString;
                    break;

                case "f":
                    timestamp.textContent = `${longDateString} at ${shortTimeString}`
                    break;

                case "F":
                    timestamp.textContent = `${weekday}, ${longDateString} at ${shortTimeString}`
                    break;

                case "R":
                    setInterval(() => {
                        timestamp.textContent = TimeUtility.formatRelativeTimestamp(date);
                    }, 1000);
                    break;
            }
        });
    }
}

export default UI;