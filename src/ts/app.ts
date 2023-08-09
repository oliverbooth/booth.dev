declare const bootstrap: any;
declare const katex: any;

(() => {
    document.querySelectorAll("pre code").forEach((block) => {
        let content = block.textContent;
        if (content.split("\n").length > 1) {
            block.parentElement.classList.add("line-numbers");
        }

        content = block.innerHTML;
        // @ts-ignore
        content = content.replaceAll("&lt;mark&gt;", "<mark>");
        // @ts-ignore
        content = content.replaceAll("&lt;/mark&gt;", "</mark>");
        block.innerHTML = content;
    });

    document.querySelectorAll("[title]").forEach((img) => {
        img.setAttribute("data-bs-toggle", "tooltip");
        img.setAttribute("data-bs-placement", "bottom");
        img.setAttribute("data-bs-html", "true");
        img.setAttribute("data-bs-title", img.getAttribute("title"));
    });

    const list = document.querySelectorAll('[data-bs-toggle="tooltip"]');
    list.forEach((el: Element) => new bootstrap.Tooltip(el));

    const tex = document.getElementsByClassName("math");
    Array.prototype.forEach.call(tex, function (el) {
        let content = el.textContent.trim();
        if (content.startsWith("\\[")) content = content.slice(2);
        if (content.endsWith("\\]")) content = content.slice(0, -2);

        katex.render(content, el);
    });
})();
