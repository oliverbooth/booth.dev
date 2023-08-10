class UI {
    public static get blogPostContainer(): HTMLDivElement {
        return document.querySelector("#all-blog-posts");
    }

    public static get blogPostTemplate(): HTMLDivElement {
        return document.querySelector("#blog-post-template");
    }

    /**
     * Forces all UI elements to update.
     */
    public static updateUI() {
        UI.addLineNumbers();
        UI.unescapeMarkTags();
    }

    /**
     * Adds line numbers to all <pre> <code> blocks that have more than one line.
     */
    public static addLineNumbers() {
        document.querySelectorAll("pre code").forEach((block) => {
            let content = block.textContent;
            if (content.trim().split("\n").length > 1) {
                block.parentElement.classList.add("line-numbers");
            }
        });
    }

    /**
     * Unescapes all <mark> tags in <pre> <code> blocks.
     */
    public static unescapeMarkTags() {
        document.querySelectorAll("pre code").forEach((block) => {
            let content = block.innerHTML;
            content = content.replaceAll("&lt;mark&gt;", "<mark>");
            content = content.replaceAll("&lt;/mark&gt;", "</mark>");
            block.innerHTML = content;
        });
    }
}

export default UI;