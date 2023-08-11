import BlogPost from "./BlogPost";
import Author from "./Author";
import TimeUtility from "./TimeUtility";

declare const bootstrap: any;
declare const katex: any;
declare const Prism: any;

class UI {
    public static get blogPostContainer(): HTMLDivElement {
        return document.querySelector("#all-blog-posts");
    }

    public static get blogPostTemplate(): HTMLDivElement {
        return document.querySelector("#blog-post-template");
    }

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
     * Creates a blog post card from the given template, post, and author.
     * @param template The Handlebars template to use.
     * @param post The blog post to use.
     * @param author The author of the blog post.
     */
    public static createBlogPostCard(template: any, post: BlogPost, author: Author): HTMLDivElement {
        const card = document.createElement("div") as HTMLDivElement;
        card.classList.add("card");
        card.classList.add("blog-card");
        card.classList.add("animate__animated");
        card.classList.add("animate__fadeIn");
        card.style.marginBottom = "50px";

        const body = template({
            post: {
                title: post.title,
                excerpt: post.excerpt,
                url: post.url,
                date: TimeUtility.formatRelativeTimestamp(post.published),
                formattedDate: post.formattedDate,
                date_humanized: `${post.updated ? "Updated" : "Published"} ${post.humanizedTimestamp}`,
                enable_comments: post.commentsEnabled,
                disqus_identifier: post.identifier,
                trimmed: post.trimmed,
            },
            author: {
                name: author.name,
                avatar: `https://gravatar.com/avatar/${author.avatarHash}?s=28`,
            }
        });
        card.innerHTML = body.trim();
        return card;
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
        UI.renderTeX(element);
        UI.renderTimestamps(element);
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
            let content = block.textContent;
            if (content.trim().split("\n").length > 1) {
                block.parentElement.classList.add("line-numbers");
            }
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
            content = content.replaceAll("&lt;mark&gt;", "<mark>");
            content = content.replaceAll("&lt;/mark&gt;", "</mark>");
            block.innerHTML = content;
        });
    }
}

export default UI;