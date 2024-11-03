import TimeUtility from "./TimeUtility";
import SiteTheme from "./SiteTheme";

declare const bootstrap: any;
declare const katex: any;
declare const Prism: any;

class UI {
    /**
     * Creates a <script> element that loads the Disqus comment counter.
     */
    public static createDisqusCounterScript(): HTMLScriptElement {
        const script = document.createElement("script");
        script.id = "dsq-count-scr";
        script.src = "https://oliverbooth-dev.disqus.com/count.js";
        script.async = true;
        return script;
    }

    /**
     * Gets the user's current-requested site theme.
     */
    public static getSiteTheme(): SiteTheme {
        const theme = getCookie("_theme");
        switch (theme) {
            case "dark":
                return SiteTheme.DARK;
            case "light":
                return SiteTheme.LIGHT;
            default:
                return SiteTheme.AUTO;
        }

        function getCookie(name: string): string {
            name = `${name}=`;
            const decodedCookie = decodeURIComponent(document.cookie);
            const cookies = decodedCookie.split(';');

            for (let index = 0; index < cookies.length; index++) {
                let current = cookies[index];

                while (current.charAt(0) == ' ') {
                    current = current.substring(1);
                }
                if (current.indexOf(name) == 0) {
                    return current.substring(name.length, current.length);
                }
            }

            return "";
        }
    }

    /**
     * Gets the user's current-requested site theme.
     */
    public static setSiteTheme(theme: SiteTheme) {
        const cookieName = "_theme";
        const expiryDays = 30;

        switch (theme) {
            case SiteTheme.DARK:
                setCookie(cookieName, "dark", expiryDays);
                break;
            case SiteTheme.LIGHT:
                setCookie(cookieName, "light", expiryDays);
                break;
            case SiteTheme.AUTO:
                setCookie(cookieName, "auto", expiryDays);
                break;
        }

        function setCookie(name: string, value: any, expiryDays: number) {
            const date = new Date();
            date.setTime(date.getTime() + (expiryDays * 24 * 60 * 60 * 1000));

            const expires = "expires=" + date.toUTCString();
            document.cookie = `${name}=${value};${expires};path=/`;
        }
    }

    /**
     * Forces all UI elements under the given element to update their rendering.
     * @param element The element to search for UI elements in.
     */
    public static updateUI(element?: Element) {
        element = element || document.body;
        UI.unescapeMarkTags(element);
        UI.addLineNumbers(element);
        UI.addHighlighting(element);
        UI.addBootstrapTooltips(element);
        UI.renderSpoilers(element);
        UI.renderTabs(element);
        UI.renderTeX(element);
        UI.renderTimestamps(element);
        UI.updateProjectCards(element);
    }

    /**
     * Adds Bootstrap tooltips to all elements with a title attribute.
     * @param element The element to search for elements with a title attribute in.
     */
    public static addBootstrapTooltips(element?: Element) {
        element = element || document.body;

        const list = element.querySelectorAll('[data-bs-toggle="tooltip"]');
        list.forEach((el: Element) => new bootstrap.Tooltip(el));

        element.querySelectorAll("[title]").forEach((el) => {
            el.setAttribute("data-bs-toggle", "tooltip");
            el.setAttribute("data-bs-placement", "bottom");
            el.setAttribute("data-bs-html", "true");
            el.setAttribute("data-bs-title", el.getAttribute("title"));

            new bootstrap.Tooltip(el);
        });
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
     * Renders all TeX in the document.
     * @param element The element to search for TeX in.
     */
    public static renderTeX(element?: Element) {
        element = element || document.body;
        const tex = element.getElementsByClassName("math");
        Array.from(tex).forEach(function (el: Element) {
            let content = el.textContent.trim();
            if (content.startsWith("\\[")) content = content.slice(2);
            if (content.endsWith("\\]")) content = content.slice(0, -2);

            katex.render(content, el);
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
}

export default UI;