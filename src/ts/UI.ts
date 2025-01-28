import TimeUtility from "./TimeUtility";

declare const Prism: any;

class UI {
    /**
     * Forces all UI elements under the given element to update their rendering.
     * @param element The element to search for UI elements in.
     */
    public static updateUI(element?: Element) {
        element = element || document.body;
        UI.unescapeMarkTags(element);
        UI.addLineNumbers(element);
        UI.addHighlighting(element);
        UI.renderSpoilers(element);
        UI.renderTabs(element);
        UI.renderTimestamps(element);
        UI.updateProjectCards(element);
        UI.applyAnsi(element);
    }

    /**
     * Adds line numbers to all <pre> <code> blocks that have more than one line.
     * @param element The element to search for <pre> <code> blocks in.
     */
    public static addLineNumbers(element?: Element) {
        element = element || document.body;
        element.querySelectorAll("pre code").forEach((block) => {
            if (block.className.indexOf("|nolinenumbers") > 0) {
                block.className = block.className.replaceAll("|nolinenumbers", "");
                return;
            }

            /*let content = block.textContent;
            if (content.trim().split("\n").length > 1) {
                block.parentElement.classList.add("line-numbers");
            }*/
        });
    }

    /**
     * Adds syntax highlighting to all <pre> <code> blocks in the element.
     * @param element The element to search for <pre> <code> blocks in.
     */
    public static addHighlighting(element?: Element) {
        element = element || document.body;
        element.querySelectorAll("pre code").forEach((block) => {
            Prism.highlightAllUnder(block.parentElement);
        });
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

    /**
     * Unescapes all <mark> tags in <pre> <code> blocks.
     * @param element The element to search for <pre> <code> blocks in.
     */
    public static unescapeMarkTags(element?: Element) {
        element = element || document.body;
        element.querySelectorAll("pre code").forEach((block) => {
            let content = block.innerHTML;

            // but ugly fucking hack. I hate this
            content = content.replaceAll(/&lt;mark(.*?)&gt;/g, "<mark$1>");
            content = content.replaceAll("&lt;/mark&gt;", "</mark>");
            block.innerHTML = content;
        });
    }

    private static updateProjectCards(element?: Element) {
        element = element || document.body;
        element.querySelectorAll(".project-card .card-body p").forEach((p: HTMLParagraphElement) => {
            p.classList.add("card-text");
        });
    }

    private static applyAnsi(element?: Element) {
        element = element || document.body;
        element.querySelectorAll("pre code.language-ansi").forEach((child: HTMLElement) => {
            const originalHtml: string = child.innerHTML || "";
            child.innerHTML = ansiToHtml(originalHtml);
        });

        element.querySelectorAll(".code-toolbar .toolbar").forEach((toolbar: HTMLDivElement) => {
            const prevSibling = toolbar.previousElementSibling;
            const nextSibling = toolbar.nextElementSibling;

            if (!prevSibling && !nextSibling) {
                return;
            }

            if ((prevSibling && prevSibling.classList.contains("language-ansi")) ||
                (nextSibling && nextSibling.classList.contains("language-ansi"))) {
                toolbar.remove();
            }
        });

        function ansiToHtml(input: string): string {
            const ansiColorMap: { [key: string]: string } = {
                "0": "unset",
                "30": "#0c0c0c",
                "31": "#c50f1f",
                "32": "#13a10e",
                "33": "#c19c00",
                "34": "#0037da",
                "35": "#881798",
                "36": "#3a96dd",
                "37": "#cccccc",
                "90": "#767676"
            };

            let wasOpen = false;
            return input
                .replace(/\x1b\[(\d+?)m/g, (_, code) => {
                    if (code == "0") return `</span>`;
                    const color: string = ansiColorMap[code];
                    const prefix = wasOpen ? `</span>` : ``;
                    if (wasOpen) {
                        wasOpen = false;
                    }
                    if (color) {
                        wasOpen = true;
                    }
                    return color ? `${prefix}<span style="color:${color};">` : `</span>`;
                })
                .concat("</span>") // Close any open tags at the end
                .replace(/<\/span>(?=<\/span>)/g, ""); // Remove redundant closing tags
        }
    }
}

export default UI;