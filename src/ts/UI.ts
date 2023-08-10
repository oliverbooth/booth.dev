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
     * Forces all UI elements to update.
     */
    public static updateUI(element?: Element) {
        element = element || document.body;
        UI.addLineNumbers(element);
        UI.addHighlighting(element);
        UI.addBootstrapTooltips(element);
        UI.renderTeX(element);
        UI.unescapeMarkTags(element);
    }

    /**
     * Adds Bootstrap tooltips to all elements with a title attribute.
     */
    public static addBootstrapTooltips(element?: Element) {
        element = element || document.body;
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
     * Renders all TeX in the document.
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
     * Unescapes all <mark> tags in <pre> <code> blocks.
     */
    public static unescapeMarkTags(element?: Element) {
        element = element || document.body;
        element.querySelectorAll("pre code").forEach((block) => {
            let content = block.innerHTML;
            content = content.replaceAll("&lt;mark&gt;", "<mark>");
            content = content.replaceAll("&lt;/mark&gt;", "</mark>");
            block.innerHTML = content;
        });
    }
}

export default UI;