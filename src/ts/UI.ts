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
        UI.addLineNumbers(element);
        UI.addSyntaxHighlighting(element);
        UI.renderSpoilers(element);
        UI.renderTabs(element);
        UI.renderTimestamps(element);
        UI.updateProjectCards(element);
        UI.applyAnsi(element);
        UI.fixCanvas(element);
        UI.runTerminalTypewriters(element);
        UI.enableFilterPills(element);
        UI.enableCopyButtons(element);
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
    public static addSyntaxHighlighting(element?: Element) {
        element = element || document.body;
        element.querySelectorAll("pre code").forEach((block: HTMLElement) => {
            Prism.highlightAllUnder(block.parentElement);
            if (block.dataset.highlight) {
                applyCodeBlockHighlights(block);
            }
        });
    }

    /**
     * Adds copy functionality to all .copy-icon elements in the element.
     * @param element The element to search for .copy-icon elements in.
     */
    public static enableCopyButtons(element?: Element) {
        element = element || document.body;
        element.querySelectorAll<HTMLElement>(".copy-icon").forEach((icon) => {
            icon.addEventListener("click", () => {
                const row = icon.closest(".crypto-row");
                const addressEl = row?.querySelector<HTMLElement>(".crypto-address");
                const text = addressEl?.textContent?.trim();

                if (!text) return;

                navigator.clipboard.writeText(text).then(() => {
                    UI.showCopyFeedback(icon);
                }).catch(() => {
                    // clipboard API unavailable or permission denied — fail silently, icon just won't confirm
                });
            });
        });
    }

    private static showCopyFeedback(icon: HTMLElement): void {
        const originalClasses = icon.className;
        icon.classList.remove("ti-copy");
        icon.classList.add("ti-check");
        icon.style.color = "var(--success-text)";

        setTimeout(() => {
            icon.className = originalClasses;
            icon.style.color = "";
        }, 1200);
    }

    /**
     * Enables filter pills to toggle visibility of sections based on their data-state attribute.
     * @param element The element to search for filter rows in.
     */
    public static enableFilterPills(element?: Element) {
        element = element || document.body;
        element.querySelectorAll<HTMLElement>("[data-filter-scope]").forEach((scope) => {
            const filterRow = scope.querySelector<HTMLElement>(".filter-row");
            if (!filterRow) return;

            const pills = Array.from(filterRow.querySelectorAll<HTMLElement>(".pill"));
            const sections = Array.from(scope.querySelectorAll<HTMLElement>("[data-state]"));

            sections.forEach((section) => section.classList.add("is-visible"));

            pills.forEach((pill) => {
                pill.addEventListener("click", () => {
                    const filter = pill.dataset.filter ?? "all";
                    const filterKind = pill.dataset.filterKind ?? "post";

                    pills.forEach((p) => p.classList.remove("active"));
                    pill.classList.add("active");

                    sections.forEach((section) => {
                        const sectionKind = section.dataset.kind ?? "post";
                        const matches = sectionKind === filterKind
                            && (filterKind === "note" || filter === "all" || section.dataset.state === filter);
                        UI.setSectionVisible(section, matches);
                    });
                });
            });
        });
    }

    private static setSectionVisible(section: HTMLElement, visible: boolean): void {
        section.classList.toggle("is-collapsed", !visible);
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

    /**
     * Runs the typewriter animation for all hero terminals under the given element.
     * @param element The element to search for terminals in.
     */
    public static runTerminalTypewriters(element?: Element) {
        element = element || document.body;
        const containers = element.querySelectorAll<HTMLElement>("[data-terminal-typewriter]");

        document.fonts.ready.then(() => {
            containers.forEach((container) => {
                UI.runTerminalTypewriter(container);
            });
        });
    }

    private static async runTerminalTypewriter(container: HTMLElement): Promise<void> {
        const reducedMotion = window.matchMedia("(prefers-reduced-motion: reduce)").matches;
        const username = container.dataset.username ?? "user@host";

        const commandLines = Array.from(container.querySelectorAll<HTMLElement>("[data-command]"));
        const revealBlocks = Array.from(container.querySelectorAll<HTMLElement>("[data-reveal]"));
        const promptLine = container.querySelector<HTMLElement>("[data-prompt]");

        if (reducedMotion) {
            [...commandLines, ...revealBlocks].forEach((el) => el.classList.add("is-visible"));
            if (promptLine) promptLine.classList.add("is-visible");
            return; // static text already correct in markup, just reveal everything at once
        }

        const sequence: HTMLElement[] = [];
        container.childNodes.forEach((node) => {
            if (node instanceof HTMLElement) sequence.push(node);
        });

        for (const el of sequence) {
            if (el.dataset.command !== undefined) {
                await UI.typeCommand(el, username, el.dataset.command);
                await UI.sleep(150);
            } else if (el.hasAttribute("data-prompt")) {
                UI.renderPrompt(el, username);
                el.classList.add("is-visible");
                await UI.sleep(300);
            } else if (el.hasAttribute("data-reveal")) {
                el.classList.add("is-visible");
                await UI.sleep(300);
            }
        }
    }

    private static async typeCommand(el: HTMLElement, username: string, text: string): Promise<void> {
        el.innerHTML = `<span class="prompt">${username}</span><span class="path">:~$</span> `;
        el.classList.add("is-visible");
        for (const char of text) {
            el.innerHTML += char;
            await UI.sleep(35 + Math.random() * 25);
        }
    }

    private static renderStaticCommand(el: HTMLElement, username: string): void {
        const text = el.dataset.command ?? "";
        el.innerHTML = `<span class="prompt">${username}</span><span class="path">:~$</span> ${text}`;
    }

    private static renderPrompt(el: HTMLElement, username: string): void {
        el.innerHTML = `<span class="prompt">${username}</span><span class="path">:~$</span> <span class="cursor">&nbsp;</span>`;
    }

    private static sleep(ms: number): Promise<void> {
        return new Promise((resolve) => setTimeout(resolve, ms));
    }
}

export default UI;